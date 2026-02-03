#!/bin/bash

# Traffic Switch Script for Blue-Green Deployment
set -e

ENV=${1:-green}
PERCENTAGE=${2:-100}

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[TRAFFIC SWITCH]${NC} $1"
}

error() {
    echo -e "${RED}[TRAFFIC SWITCH ERROR]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[TRAFFIC SWITCH WARNING]${NC} $1"
}

# Nginx configuration path
NGINX_CONF="/etc/nginx/sites-available/recommendo"
NGINX_ENABLED="/etc/nginx/sites-enabled/recommendo"

if [ "$ENV" != "blue" ] && [ "$ENV" != "green" ]; then
    error "Invalid environment: $ENV. Use 'blue' or 'green'"
    exit 1
fi

if [ $PERCENTAGE -lt 0 ] || [ $PERCENTAGE -gt 100 ]; then
    error "Invalid percentage: $PERCENTAGE. Use 0-100"
    exit 1
fi

log "Switching $PERCENTAGE% of traffic to $ENV environment"

# Calculate the opposite environment
if [ "$ENV" == "blue" ]; then
    OLD_ENV="green"
    NEW_API_PORT=5000
    NEW_FRONTEND_PORT=3000
    OLD_API_PORT=5001
    OLD_FRONTEND_PORT=3001
else
    OLD_ENV="blue"
    NEW_API_PORT=5001
    NEW_FRONTEND_PORT=3001
    OLD_API_PORT=5000
    OLD_FRONTEND_PORT=3000
fi

OLD_PERCENTAGE=$((100 - PERCENTAGE))

# Check if Nginx is installed
if ! command -v nginx &> /dev/null; then
    warning "Nginx not installed. Skipping traffic switch (development mode)"
    log "In production, configure Nginx as a reverse proxy"
    exit 0
fi

# Generate Nginx configuration
generate_nginx_config() {
    cat > /tmp/recommendo.conf << EOF
# Recommendo Nginx Configuration - Blue-Green Deployment

upstream api_servers {
    server localhost:$NEW_API_PORT weight=$PERCENTAGE max_fails=3 fail_timeout=30s;
    server localhost:$OLD_API_PORT weight=$OLD_PERCENTAGE max_fails=3 fail_timeout=30s;
}

upstream frontend_servers {
    server localhost:$NEW_FRONTEND_PORT weight=$PERCENTAGE max_fails=3 fail_timeout=30s;
    server localhost:$OLD_FRONTEND_PORT weight=$OLD_PERCENTAGE max_fails=3 fail_timeout=30s;
}

server {
    listen 80;
    server_name localhost;
    
    # API endpoints
    location /api/ {
        proxy_pass http://api_servers;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Health check
        proxy_next_upstream error timeout http_500 http_502 http_503;
    }
    
    # Health check endpoint
    location /health {
        proxy_pass http://api_servers;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
    }
    
    # Frontend
    location / {
        proxy_pass http://frontend_servers;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Health check
        proxy_next_upstream error timeout http_500 http_502 http_503;
    }
    
    # Logging
    access_log /var/log/nginx/recommendo-access.log;
    error_log /var/log/nginx/recommendo-error.log;
}
EOF
}

# Apply Nginx configuration
apply_nginx_config() {
    log "Generating Nginx configuration..."
    generate_nginx_config
    
    # Test configuration
    log "Testing Nginx configuration..."
    if sudo nginx -t -c /tmp/recommendo.conf 2>/dev/null; then
        log "Nginx configuration is valid"
    else
        error "Invalid Nginx configuration"
        return 1
    fi
    
    # Backup current configuration
    if [ -f "$NGINX_CONF" ]; then
        sudo cp "$NGINX_CONF" "$NGINX_CONF.backup.$(date +%Y%m%d-%H%M%S)"
    fi
    
    # Apply configuration
    log "Applying Nginx configuration..."
    sudo cp /tmp/recommendo.conf "$NGINX_CONF"
    
    if [ ! -L "$NGINX_ENABLED" ]; then
        sudo ln -sf "$NGINX_CONF" "$NGINX_ENABLED"
    fi
    
    # Reload Nginx
    log "Reloading Nginx..."
    sudo nginx -s reload
    
    log "Traffic switch completed!"
    log "Current distribution: $ENV ($PERCENTAGE%), $OLD_ENV ($OLD_PERCENTAGE%)"
}

# Main
main() {
    log "========================================="
    log "Traffic Switch Configuration"
    log "========================================="
    log "Target environment: $ENV"
    log "Traffic percentage: $PERCENTAGE%"
    log "Previous environment: $OLD_ENV ($OLD_PERCENTAGE%)"
    echo ""
    
    apply_nginx_config
    
    echo ""
    log "========================================="
    log "Traffic switch successful!"
    log "========================================="
}

main
