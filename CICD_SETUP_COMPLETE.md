# CI/CD Setup Complete ✅

The complete CI/CD infrastructure has been implemented for the Recommendo application.

## What's Been Created

### 1. GitHub Actions Workflows
- **`.github/workflows/ci.yml`** - Automated testing on every push/PR
  - Backend tests with PostgreSQL
  - Frontend tests with coverage reporting
  - Docker image builds
  - Security scanning (Trivy + TruffleHog)
  
- **`.github/workflows/cd-production.yml`** - Production deployment
  - Triggered manually via workflow_dispatch
  - Gradual traffic rollout: 10% → 50% → 100%
  - Automatic rollback on failure

- **`.github/dependabot.yml`** - Automated dependency updates

### 2. Test Projects

#### Backend Tests (C# xUnit)
- **`api/Recommendo.Api.Tests/`** - Test project with xUnit, Moq, FluentAssertions
  - `Services/AuthServiceTests.cs` - 6 tests for authentication
  - `Services/FriendServiceTests.cs` - 10 tests for friend management
  - Uses in-memory database for fast, isolated testing

#### Frontend Tests (Jest + React Testing Library)
- **`frontend/jest.config.js`** - Jest configuration
- **`frontend/src/setupTests.ts`** - Test environment setup
- **`frontend/src/__tests__/`** - Test files
  - `Friends.test.tsx` - Component testing
  - `client.test.ts` - API client testing

### 3. Blue-Green Deployment Scripts

- **`deploy-bluegreen.sh`** - Main deployment orchestrator
  - Database backups before deployment
  - Health checks after deployment
  - Automatic migration running
  - Comprehensive logging

- **`scripts/health-check.sh`** - Validates deployment health
  - Container status checks
  - API endpoint validation
  - Response time monitoring
  - Error log scanning

- **`scripts/smoke-tests.sh`** - 10 automated functional tests
  - User registration & login
  - Friend search & requests
  - Invite link generation
  - Database connectivity
  - Performance validation

- **`scripts/traffic-switch.sh`** - Nginx traffic management
  - Gradual traffic shifting
  - Weighted load balancing
  - Zero-downtime deployments

- **`scripts/rollback.sh`** - Emergency rollback capability
  - Instant revert to previous environment
  - Database restore option

### 4. Docker Compose Configurations

- **`docker-compose.test.yml`** - Test environment (ports 5002/3002)
- **`docker-compose.blue.yml`** - Blue environment (ports 5000/3000)
- **`docker-compose.green.yml`** - Green environment (ports 5001/3001)

All environments share the production database with proper connection pooling and health checks.

## Next Steps to Get Started

### 1. Install Test Dependencies

**Backend:**
```bash
cd api/Recommendo.Api.Tests
dotnet restore
```

**Frontend:**
```bash
cd frontend
npm install
```

### 2. Run Tests Locally

**Backend:**
```bash
cd api/Recommendo.Api.Tests
dotnet test
```

**Frontend:**
```bash
cd frontend
npm test
```

### 3. Set Up GitHub Secrets

Go to your repository → Settings → Secrets and variables → Actions, and add:

- `PRODUCTION_SSH_KEY` - SSH private key for server access
- `PRODUCTION_HOST` - Your production server IP/hostname
- `PRODUCTION_USER` - SSH username (e.g., 'ubuntu')
- `JWT_SECRET` - Your JWT signing key (min 32 chars)
- `DB_PASSWORD` - PostgreSQL database password

### 4. Configure Branch Protection

1. Go to repository Settings → Branches
2. Add protection rule for `main` branch:
   - ✅ Require status checks before merging
   - ✅ Select: `backend-tests`, `frontend-tests`, `docker-build`
   - ✅ Require branches to be up to date

### 5. Install Nginx on Production Server

```bash
# SSH to production server
ssh user@your-server

# Install Nginx
sudo apt update
sudo apt install nginx -y

# Enable and start Nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

### 6. Deploy Initial Environment

```bash
# Make scripts executable (first time only)
chmod +x deploy-bluegreen.sh scripts/*.sh

# Deploy to blue environment
./deploy-bluegreen.sh deploy blue

# Run health checks
./scripts/health-check.sh blue

# Run smoke tests
./scripts/smoke-tests.sh blue

# If all passes, route traffic
./scripts/traffic-switch.sh blue 100
```

### 7. Test the Workflow

1. Create a new branch: `git checkout -b feature/test-ci`
2. Make a small change and commit
3. Push: `git push origin feature/test-ci`
4. Create a pull request
5. Watch the CI pipeline run automatically
6. After merge, trigger production deployment manually via GitHub Actions

## How to Use

### Running CI Checks
CI runs automatically on every push and pull request. No manual action needed.

### Deploying to Production

**Via GitHub Actions (Recommended):**
1. Go to Actions tab → "Production Deployment"
2. Click "Run workflow"
3. Monitor progress in GitHub Actions

**Via SSH (Advanced):**
```bash
# Deploy to inactive environment
./deploy-bluegreen.sh deploy green

# Test the deployment
./scripts/smoke-tests.sh green

# Gradual traffic switch
./scripts/traffic-switch.sh green 10  # 10% traffic
./scripts/traffic-switch.sh green 50  # 50% traffic
./scripts/traffic-switch.sh green 100 # 100% traffic

# If issues occur
./scripts/rollback.sh blue  # Instant rollback
```

### Checking Deployment Status
```bash
./deploy-bluegreen.sh status
```

### Emergency Rollback
```bash
./scripts/rollback.sh [previous-environment]
```

## Architecture

### Blue-Green Deployment Flow

```
┌──────────────────────────────────────────────────────┐
│                    Nginx (Port 80)                   │
│              Traffic Distribution Layer               │
└────────────┬─────────────────────────┬───────────────┘
             │                         │
             ▼                         ▼
    ┌────────────────┐       ┌────────────────┐
    │  Blue (5000)   │       │ Green (5001)   │
    │  Production    │       │  Staging/New   │
    └────────┬───────┘       └────────┬───────┘
             │                         │
             └────────────┬────────────┘
                          ▼
                 ┌────────────────┐
                 │   PostgreSQL   │
                 │   (Shared DB)  │
                 └────────────────┘
```

### CI/CD Pipeline Flow

```
Code Push
    │
    ├─► Backend Tests (PostgreSQL + xUnit)
    ├─► Frontend Tests (Jest + Coverage)
    ├─► Docker Build Validation
    └─► Security Scan (Trivy + TruffleHog)
         │
         ▼
    Tests Pass? ──Yes──► Merge Allowed
         │
        No
         │
         ▼
    Block Merge
```

### Deployment Pipeline

```
Manual Trigger
    │
    ├─► Check Prerequisites
    ├─► Backup Database
    ├─► Deploy to Inactive Environment
    ├─► Run Migrations
    ├─► Health Checks
    ├─► Smoke Tests
    │
    ├─► Switch 10% Traffic ──► Monitor
    ├─► Switch 50% Traffic ──► Monitor
    └─► Switch 100% Traffic ──► Complete
         │
    Issues? ──Yes──► Automatic Rollback
```

## Test Coverage

### Backend Tests (10 tests)
- ✅ User registration with valid data
- ✅ Duplicate email prevention
- ✅ User login with valid credentials
- ✅ Invalid password rejection
- ✅ Friend request creation
- ✅ Self-friending prevention
- ✅ Friend request acceptance (bidirectional)
- ✅ Friend request rejection
- ✅ User search functionality
- ✅ Pending request retrieval

### Frontend Tests (3 tests)
- ✅ Friends list rendering
- ✅ Empty state display
- ✅ Pending requests tab functionality

### Smoke Tests (10 tests)
- ✅ User registration endpoint
- ✅ User login endpoint
- ✅ Invite generation
- ✅ Friends list retrieval
- ✅ User search
- ✅ Friend request sending
- ✅ Database connectivity
- ✅ API response times (<500ms)
- ✅ Error rate monitoring
- ✅ Container health status

## Monitoring & Observability

All scripts provide comprehensive logging:
- Timestamped log entries
- Color-coded output (success/warning/error)
- Detailed error messages
- Performance metrics
- Health check results

Logs are stored in:
- `logs/deployment-[timestamp].log`
- `logs/health-check-[env]-[timestamp].log`
- `logs/smoke-tests-[env]-[timestamp].log`

## Security Features

- ✅ Secrets managed via GitHub Actions
- ✅ Database backup before each deployment
- ✅ Vulnerability scanning (Trivy)
- ✅ Secret detection (TruffleHog)
- ✅ Automated dependency updates (Dependabot)
- ✅ Branch protection with required checks
- ✅ No credentials in code or configs

## Performance Considerations

- Gradual traffic switching minimizes risk
- Health checks run every 30 seconds
- Database connection pooling (100 max connections)
- Shared database eliminates data sync issues
- Zero-downtime deployments
- Instant rollback capability (<30 seconds)

## Troubleshooting

### CI Tests Fail
```bash
# Run tests locally
cd api/Recommendo.Api.Tests && dotnet test
cd frontend && npm test
```

### Deployment Fails
```bash
# Check logs
tail -f logs/deployment-*.log

# Verify prerequisites
./deploy-bluegreen.sh status

# Check Docker
docker ps
docker logs recommendo-api-blue
```

### Health Checks Fail
```bash
# Detailed health check
./scripts/health-check.sh blue

# Check individual services
curl http://localhost:5000/health
docker exec recommendo-db-1 pg_isready
```

### Need to Rollback
```bash
# Immediate rollback to previous environment
./scripts/rollback.sh blue

# Check status
./deploy-bluegreen.sh status
```

## Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [xUnit Documentation](https://xunit.net/)
- [Jest Documentation](https://jestjs.io/)
- [Nginx Documentation](https://nginx.org/en/docs/)

## Support

For issues or questions:
1. Check logs in `logs/` directory
2. Run `./deploy-bluegreen.sh status` for current state
3. Review GitHub Actions logs for CI/CD failures
4. Verify all GitHub secrets are set correctly

---

**Status:** ✅ All infrastructure files created and ready to use
**Next:** Run tests locally, set up GitHub secrets, and test the CI pipeline
