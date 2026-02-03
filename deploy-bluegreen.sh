#!/bin/bash

# Blue-Green Deployment Script for Recommendo
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
BLUE_COMPOSE="docker-compose.blue.yml"
GREEN_COMPOSE="docker-compose.green.yml"
LOG_FILE="deployment-$(date +%Y%m%d-%H%M%S).log"

# Functions
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$LOG_FILE"
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1" | tee -a "$LOG_FILE"
}

check_prerequisites() {
    log "Checking prerequisites..."
    
    if ! command -v docker &> /dev/null; then
        error "Docker is not installed"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        error "Docker Compose is not installed"
        exit 1
    fi
    
    if [ ! -f ".env" ]; then
        error ".env file not found"
        exit 1
    fi
    
    log "Prerequisites check passed"
}

backup_database() {
    log "Backing up database..."
    
    BACKUP_DIR="./backups"
    mkdir -p "$BACKUP_DIR"
    
    BACKUP_FILE="$BACKUP_DIR/db-backup-$(date +%Y%m%d-%H%M%S).sql"
    
    docker exec recommendo-db-blue pg_dump -U recommendo recommendo > "$BACKUP_FILE" 2>/dev/null || {
        warning "Database backup failed or no existing database"
        return 0
    }
    
    log "Database backed up to $BACKUP_FILE"
    
    # Keep only last 10 backups
    ls -t "$BACKUP_DIR"/db-backup-*.sql | tail -n +11 | xargs -r rm
}

check_disk_space() {
    log "Checking disk space..."
    
    AVAILABLE=$(df -h . | awk 'NR==2 {print $4}' | sed 's/G//')
    
    if (( $(echo "$AVAILABLE < 5" | bc -l) )); then
        error "Insufficient disk space. Available: ${AVAILABLE}GB"
        exit 1
    fi
    
    log "Disk space check passed. Available: ${AVAILABLE}GB"
}

run_migrations() {
    local ENV=$1
    log "Running database migrations for $ENV environment..."
    
    local CONTAINER=""
    if [ "$ENV" == "blue" ]; then
        CONTAINER="recommendo-api-blue"
    else
        CONTAINER="recommendo-api-green"
    fi
    
    # Wait for container to be ready
    sleep 10
    
    docker exec "$CONTAINER" dotnet ef database update --no-build || {
        error "Migration failed"
        return 1
    }
    
    log "Migrations completed successfully"
}

deploy_environment() {
    local ENV=$1
    log "Deploying to $ENV environment..."
    
    local COMPOSE_FILE=""
    if [ "$ENV" == "blue" ]; then
        COMPOSE_FILE="$BLUE_COMPOSE"
    else
        COMPOSE_FILE="$GREEN_COMPOSE"
    fi
    
    if [ ! -f "$COMPOSE_FILE" ]; then
        error "Compose file $COMPOSE_FILE not found"
        exit 1
    fi
    
    # Pull latest images
    log "Pulling latest images..."
    docker-compose -f "$COMPOSE_FILE" pull || true
    
    # Build images
    log "Building images..."
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    
    # Start containers
    log "Starting $ENV containers..."
    docker-compose -f "$COMPOSE_FILE" up -d
    
    # Wait for services to be healthy
    log "Waiting for services to be healthy..."
    local MAX_ATTEMPTS=30
    local ATTEMPT=0
    
    while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
        if docker ps --filter "label=com.recommendo.environment=$ENV" --filter "health=healthy" | grep -q "recommendo"; then
            log "$ENV environment is healthy"
            return 0
        fi
        
        ATTEMPT=$((ATTEMPT + 1))
        sleep 5
    done
    
    error "$ENV environment failed to become healthy"
    return 1
}

cleanup_environment() {
    local ENV=$1
    log "Cleaning up $ENV environment..."
    
    local COMPOSE_FILE=""
    if [ "$ENV" == "blue" ]; then
        COMPOSE_FILE="$BLUE_COMPOSE"
    else
        COMPOSE_FILE="$GREEN_COMPOSE"
    fi
    
    docker-compose -f "$COMPOSE_FILE" down
    
    # Remove unused images
    docker image prune -f
    
    log "$ENV environment cleaned up"
}

get_current_environment() {
    if docker ps --filter "name=recommendo-api-blue" --filter "status=running" | grep -q "recommendo-api-blue"; then
        echo "blue"
    elif docker ps --filter "name=recommendo-api-green" --filter "status=running" | grep -q "recommendo-api-green"; then
        echo "green"
    else
        echo "none"
    fi
}

show_status() {
    log "Current deployment status:"
    
    local CURRENT=$(get_current_environment)
    log "Active environment: $CURRENT"
    
    echo ""
    log "Running containers:"
    docker ps --filter "name=recommendo" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    echo ""
    log "Container health:"
    docker ps --filter "name=recommendo" --format "table {{.Names}}\t{{.Status}}"
}

# Main script
main() {
    local COMMAND=$1
    local ENVIRONMENT=$2
    
    case $COMMAND in
        deploy)
            if [ -z "$ENVIRONMENT" ]; then
                error "Usage: $0 deploy [blue|green]"
                exit 1
            fi
            
            log "Starting deployment to $ENVIRONMENT environment"
            check_prerequisites
            backup_database
            check_disk_space
            deploy_environment "$ENVIRONMENT"
            run_migrations "$ENVIRONMENT"
            log "Deployment to $ENVIRONMENT completed successfully"
            ;;
            
        cleanup)
            if [ -z "$ENVIRONMENT" ]; then
                error "Usage: $0 cleanup [blue|green]"
                exit 1
            fi
            
            cleanup_environment "$ENVIRONMENT"
            ;;
            
        status)
            show_status
            ;;
            
        *)
            echo "Usage: $0 {deploy|cleanup|status} [environment]"
            echo ""
            echo "Commands:"
            echo "  deploy [blue|green]   - Deploy to specified environment"
            echo "  cleanup [blue|green]  - Cleanup specified environment"
            echo "  status                - Show current deployment status"
            exit 1
            ;;
    esac
}

# Run main function
main "$@"
