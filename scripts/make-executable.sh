#!/bin/bash

# Make scripts executable
chmod +x deploy-bluegreen.sh
chmod +x scripts/*.sh

echo "Scripts are now executable!"
echo ""
echo "Available commands:"
echo "  ./deploy-bluegreen.sh deploy [blue|green]  - Deploy to environment"
echo "  ./deploy-bluegreen.sh status               - Show deployment status"
echo "  ./scripts/health-check.sh [blue|green]     - Check health"
echo "  ./scripts/smoke-tests.sh [blue|green]      - Run smoke tests"
echo "  ./scripts/traffic-switch.sh [env] [%]      - Switch traffic"
echo "  ./scripts/rollback.sh [blue|green]         - Emergency rollback"
