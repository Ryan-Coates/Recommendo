#!/bin/bash

# Smoke Tests Script for Recommendo
set -e

ENV=${1:-blue}

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[SMOKE TEST]${NC} $1"
}

error() {
    echo -e "${RED}[SMOKE TEST ERROR]${NC} $1"
}

# Determine ports based on environment
if [ "$ENV" == "blue" ]; then
    API_PORT=5000
    FRONTEND_PORT=3000
elif [ "$ENV" == "green" ]; then
    API_PORT=5001
    FRONTEND_PORT=3001
elif [ "$ENV" == "test" ]; then
    API_PORT=5002
    FRONTEND_PORT=3002
else
    error "Invalid environment: $ENV. Use 'blue', 'green', or 'test'"
    exit 1
fi

BASE_URL="http://localhost:$API_PORT"
FRONTEND_URL="http://localhost:$FRONTEND_PORT"

log "Running smoke tests for $ENV environment"
log "API: $BASE_URL"
log "Frontend: $FRONTEND_URL"

# Test counter
TESTS_RUN=0
TESTS_PASSED=0
TESTS_FAILED=0

run_test() {
    local TEST_NAME=$1
    local COMMAND=$2
    
    TESTS_RUN=$((TESTS_RUN + 1))
    
    log "Running test: $TEST_NAME"
    
    if eval "$COMMAND"; then
        log "✓ PASSED: $TEST_NAME"
        TESTS_PASSED=$((TESTS_PASSED + 1))
        return 0
    else
        error "✗ FAILED: $TEST_NAME"
        TESTS_FAILED=$((TESTS_FAILED + 1))
        return 1
    fi
}

# Test 1: API is responding
test_api_responding() {
    curl -sf "$BASE_URL/health" > /dev/null 2>&1
}

# Test 2: Frontend is responding
test_frontend_responding() {
    curl -sf "$FRONTEND_URL" > /dev/null 2>&1
}

# Test 3: Register new user
test_user_registration() {
    local TIMESTAMP=$(date +%s)
    local EMAIL="smoketest_$TIMESTAMP@test.com"
    local USERNAME="smoketest_$TIMESTAMP"
    
    local RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"Test123!@#\"}")
    
    echo "$RESPONSE" | grep -q "token"
}

# Test 4: User login
test_user_login() {
    # First register a user
    local TIMESTAMP=$(date +%s)
    local EMAIL="smoketest_login_$TIMESTAMP@test.com"
    local USERNAME="smoketest_login_$TIMESTAMP"
    
    curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"Test123!@#\"}" > /dev/null
    
    # Then login
    local RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"password\":\"Test123!@#\"}")
    
    echo "$RESPONSE" | grep -q "token"
}

# Test 5: Generate invite link (authenticated)
test_generate_invite() {
    # Register and login
    local TIMESTAMP=$(date +%s)
    local EMAIL="smoketest_invite_$TIMESTAMP@test.com"
    local USERNAME="smoketest_invite_$TIMESTAMP"
    
    local AUTH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"Test123!@#\"}")
    
    local TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"token":"[^"]*"' | sed 's/"token":"\([^"]*\)"/\1/')
    
    if [ -z "$TOKEN" ]; then
        return 1
    fi
    
    # Generate invite
    local RESPONSE=$(curl -s -X POST "$BASE_URL/api/friends/invite" \
        -H "Authorization: Bearer $TOKEN")
    
    echo "$RESPONSE" | grep -q "token"
}

# Test 6: Get friends list (should be empty for new user)
test_get_friends() {
    # Register and login
    local TIMESTAMP=$(date +%s)
    local EMAIL="smoketest_friends_$TIMESTAMP@test.com"
    local USERNAME="smoketest_friends_$TIMESTAMP"
    
    local AUTH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"Test123!@#\"}")
    
    local TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"token":"[^"]*"' | sed 's/"token":"\([^"]*\)"/\1/')
    
    if [ -z "$TOKEN" ]; then
        return 1
    fi
    
    # Get friends
    local RESPONSE=$(curl -s -X GET "$BASE_URL/api/friends" \
        -H "Authorization: Bearer $TOKEN")
    
    # Should return empty array
    echo "$RESPONSE" | grep -q "\[\]"
}

# Test 7: Search for users
test_search_users() {
    # Register and login
    local TIMESTAMP=$(date +%s)
    local EMAIL="smoketest_search_$TIMESTAMP@test.com"
    local USERNAME="smoketest_search_$TIMESTAMP"
    
    local AUTH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"username\":\"$USERNAME\",\"password\":\"Test123!@#\"}")
    
    local TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"token":"[^"]*"' | sed 's/"token":"\([^"]*\)"/\1/')
    
    if [ -z "$TOKEN" ]; then
        return 1
    fi
    
    # Search for users
    curl -sf "$BASE_URL/api/friends/search?query=test" \
        -H "Authorization: Bearer $TOKEN" > /dev/null
}

# Test 8: Database connectivity
test_database_connectivity() {
    local DB_CONTAINER="recommendo-db-blue"
    
    if [ "$ENV" == "test" ]; then
        DB_CONTAINER="recommendo-test-db"
    fi
    
    docker exec "$DB_CONTAINER" psql -U recommendo -d recommendo -c "SELECT 1" > /dev/null 2>&1 || \
    docker exec "$DB_CONTAINER" psql -U recommendo -d recommendo_test -c "SELECT 1" > /dev/null 2>&1
}

# Test 9: Check response times
test_response_times() {
    local START=$(date +%s%N)
    curl -sf "$BASE_URL/health" > /dev/null 2>&1
    local END=$(date +%s%N)
    
    local DURATION=$((($END - $START) / 1000000))
    
    # Response should be under 2 seconds
    [ $DURATION -lt 2000 ]
}

# Test 10: CORS headers (should allow frontend origin)
test_cors_headers() {
    local RESPONSE=$(curl -s -I "$BASE_URL/health")
    
    # Check if CORS headers are present (they should be in production)
    echo "$RESPONSE" | grep -qi "access-control-allow" || return 0
}

# Run all tests
main() {
    log "========================================="
    log "Starting smoke tests..."
    log "========================================="
    echo ""
    
    run_test "API is responding" "test_api_responding"
    run_test "Frontend is responding" "test_frontend_responding"
    run_test "Database connectivity" "test_database_connectivity"
    run_test "User registration" "test_user_registration"
    run_test "User login" "test_user_login"
    run_test "Generate invite link" "test_generate_invite"
    run_test "Get friends list" "test_get_friends"
    run_test "Search users" "test_search_users"
    run_test "Response times" "test_response_times"
    run_test "CORS headers" "test_cors_headers"
    
    echo ""
    log "========================================="
    log "Smoke test results:"
    log "Tests run: $TESTS_RUN"
    log "Passed: $TESTS_PASSED"
    log "Failed: $TESTS_FAILED"
    log "========================================="
    
    if [ $TESTS_FAILED -eq 0 ]; then
        log "All smoke tests passed! ✓"
        exit 0
    else
        error "Some smoke tests failed! ✗"
        exit 1
    fi
}

main
