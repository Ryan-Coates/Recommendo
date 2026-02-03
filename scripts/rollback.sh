#!/bin/bash

# Emergency Rollback Script
set -e

TARGET_ENV=${1:-blue}

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[ROLLBACK]${NC} $1"
}

error() {
    echo -e "${RED}[ROLLBACK ERROR]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[ROLLBACK WARNING]${NC} $1"
}

if [ "$TARGET_ENV" != "blue" ] && [ "$TARGET_ENV" != "green" ]; then
    error "Invalid environment: $TARGET_ENV. Use 'blue' or 'green'"
    exit 1
fi

log "========================================="
log "EMERGENCY ROLLBACK TO $TARGET_ENV"
log "========================================="
warning "This will immediately switch all traffic to $TARGET_ENV environment"

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR/.."

# Switch 100% traffic immediately
log "Switching 100% traffic to $TARGET_ENV..."
./scripts/traffic-switch.sh "$TARGET_ENV" 100

# Check health of target environment
log "Verifying health of $TARGET_ENV environment..."
if ./scripts/health-check.sh "$TARGET_ENV"; then
    log "✓ $TARGET_ENV environment is healthy"
else
    error "✗ $TARGET_ENV environment health check failed!"
    warning "Manual intervention required!"
    exit 1
fi

# Stop the failed environment
if [ "$TARGET_ENV" == "blue" ]; then
    FAILED_ENV="green"
else
    FAILED_ENV="blue"
fi

log "Stopping $FAILED_ENV environment..."
./deploy-bluegreen.sh cleanup "$FAILED_ENV" || warning "Failed to stop $FAILED_ENV environment"

log "========================================="
log "Rollback to $TARGET_ENV completed!"
log "========================================="
log "Please investigate the failure in $FAILED_ENV environment"
log "Check logs: docker logs recommendo-api-$FAILED_ENV"
