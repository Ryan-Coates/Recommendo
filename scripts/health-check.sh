#!/bin/bash

# Health Check Script for Recommendo
set -e

ENV=${1:-blue}
MAX_RETRIES=10
RETRY_DELAY=5

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[HEALTH CHECK]${NC} $1"
}

error() {
    echo -e "${RED}[HEALTH CHECK ERROR]${NC} $1"
}

# Determine ports based on environment
if [ "$ENV" == "blue" ]; then
    API_PORT=5000
    FRONTEND_PORT=3000
elif [ "$ENV" == "green" ]; then
    API_PORT=5001
    FRONTEND_PORT=3001
else
    error "Invalid environment: $ENV. Use 'blue' or 'green'"
    exit 1
fi

log "Checking health of $ENV environment (API: $API_PORT, Frontend: $FRONTEND_PORT)"

# Check API health endpoint
check_api() {
    local RETRY=0
    while [ $RETRY -lt $MAX_RETRIES ]; do
        if curl -sf "http://localhost:$API_PORT/health" > /dev/null 2>&1; then
            log "✓ API health check passed"
            return 0
        fi
        
        RETRY=$((RETRY + 1))
        if [ $RETRY -lt $MAX_RETRIES ]; then
            log "API not ready, retrying in $RETRY_DELAY seconds... ($RETRY/$MAX_RETRIES)"
            sleep $RETRY_DELAY
        fi
    done
    
    error "✗ API health check failed after $MAX_RETRIES attempts"
    return 1
}

# Check Frontend
check_frontend() {
    local RETRY=0
    while [ $RETRY -lt $MAX_RETRIES ]; do
        if curl -sf "http://localhost:$FRONTEND_PORT" > /dev/null 2>&1; then
            log "✓ Frontend health check passed"
            return 0
        fi
        
        RETRY=$((RETRY + 1))
        if [ $RETRY -lt $MAX_RETRIES ]; then
            log "Frontend not ready, retrying in $RETRY_DELAY seconds... ($RETRY/$MAX_RETRIES)"
            sleep $RETRY_DELAY
        fi
    done
    
    error "✗ Frontend health check failed after $MAX_RETRIES attempts"
    return 1
}

# Check Docker containers
check_containers() {
    local API_CONTAINER="recommendo-api-$ENV"
    local FRONTEND_CONTAINER="recommendo-frontend-$ENV"
    
    if ! docker ps --filter "name=$API_CONTAINER" --filter "status=running" | grep -q "$API_CONTAINER"; then
        error "✗ API container not running"
        return 1
    fi
    log "✓ API container is running"
    
    if ! docker ps --filter "name=$FRONTEND_CONTAINER" --filter "status=running" | grep -q "$FRONTEND_CONTAINER"; then
        error "✗ Frontend container not running"
        return 1
    fi
    log "✓ Frontend container is running"
    
    return 0
}

# Check container health status
check_container_health() {
    local API_CONTAINER="recommendo-api-$ENV"
    local FRONTEND_CONTAINER="recommendo-frontend-$ENV"
    
    local API_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' "$API_CONTAINER" 2>/dev/null || echo "none")
    local FRONTEND_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' "$FRONTEND_CONTAINER" 2>/dev/null || echo "none")
    
    if [ "$API_HEALTH" != "healthy" ] && [ "$API_HEALTH" != "none" ]; then
        error "✗ API container health status: $API_HEALTH"
        return 1
    fi
    log "✓ API container health status: $API_HEALTH"
    
    if [ "$FRONTEND_HEALTH" != "healthy" ] && [ "$FRONTEND_HEALTH" != "none" ]; then
        error "✗ Frontend container health status: $FRONTEND_HEALTH"
        return 1
    fi
    log "✓ Frontend container health status: $FRONTEND_HEALTH"
    
    return 0
}

# Check response time
check_response_time() {
    local START=$(date +%s%N)
    curl -sf "http://localhost:$API_PORT/health" > /dev/null 2>&1
    local END=$(date +%s%N)
    
    local DURATION=$((($END - $START) / 1000000))
    
    if [ $DURATION -gt 1000 ]; then
        error "✗ API response time too slow: ${DURATION}ms"
        return 1
    fi
    
    log "✓ API response time: ${DURATION}ms"
    return 0
}

# Check for errors in logs
check_error_logs() {
    local API_CONTAINER="recommendo-api-$ENV"
    
    local ERROR_COUNT=$(docker logs "$API_CONTAINER" --since 5m 2>&1 | grep -i "error\|exception\|fatal" | wc -l)
    
    if [ $ERROR_COUNT -gt 10 ]; then
        error "✗ High number of errors in logs: $ERROR_COUNT"
        docker logs "$API_CONTAINER" --tail 20
        return 1
    fi
    
    log "✓ Error log check passed (errors: $ERROR_COUNT)"
    return 0
}

# Main health check
main() {
    local EXIT_CODE=0
    
    log "Starting comprehensive health check for $ENV environment..."
    echo ""
    
    check_containers || EXIT_CODE=1
    check_container_health || EXIT_CODE=1
    check_api || EXIT_CODE=1
    check_frontend || EXIT_CODE=1
    check_response_time || EXIT_CODE=1
    check_error_logs || EXIT_CODE=1
    
    echo ""
    if [ $EXIT_CODE -eq 0 ]; then
        log "========================================="
        log "All health checks passed! ✓"
        log "========================================="
    else
        error "========================================="
        error "Some health checks failed! ✗"
        error "========================================="
    fi
    
    exit $EXIT_CODE
}

main
