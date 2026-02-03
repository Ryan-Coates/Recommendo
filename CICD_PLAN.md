# CI/CD Implementation Plan for Recommendo

## Overview
This plan implements a robust CI/CD pipeline with:
1. **Pre-merge validation** via GitHub Actions
2. **Blue-Green deployment** for zero-downtime production releases
3. **Automated testing and validation** at each stage

---

## Phase 1: GitHub Actions - Pre-Merge Testing

### 1.1 Testing Strategy

#### Backend Tests (API)
- **Unit Tests**: Test individual services, controllers, and data access
- **Integration Tests**: Test database operations and API endpoints
- **Code Quality**: Linting and code analysis

#### Frontend Tests
- **Unit Tests**: Test React components and utilities
- **Integration Tests**: Test API integration
- **Build Validation**: Ensure the app builds successfully
- **Linting**: ESLint and TypeScript checks

#### Database Tests
- **Migration Tests**: Ensure migrations run successfully
- **Schema Validation**: Verify database schema integrity

### 1.2 GitHub Actions Workflow Structure

```yaml
# .github/workflows/ci.yml
name: CI Pipeline

on:
  pull_request:
    branches: [main, develop]
  push:
    branches: [develop]

jobs:
  # Job 1: Backend Tests
  backend-tests:
    - Setup .NET 8.0
    - Restore dependencies
    - Run unit tests
    - Run integration tests (with test database)
    - Code coverage report
    - Upload artifacts

  # Job 2: Frontend Tests
  frontend-tests:
    - Setup Node.js
    - Install dependencies
    - Run ESLint
    - Run unit tests
    - Build production bundle
    - Upload artifacts

  # Job 3: Docker Build Test
  docker-build:
    - Build Docker images
    - Test docker-compose configuration
    - Verify all services start correctly

  # Job 4: Security Scanning
  security:
    - Scan dependencies for vulnerabilities
    - Check for secrets in code
    - SAST (Static Application Security Testing)
```

### 1.3 Required Test Implementation

**Backend Tests to Create:**
- `AuthServiceTests.cs`
- `FriendServiceTests.cs`
- `RecommendationServiceTests.cs`
- `ControllersIntegrationTests.cs`

**Frontend Tests to Create:**
- `Auth.test.tsx`
- `Friends.test.tsx`
- `Home.test.tsx`
- `api/client.test.ts`

---

## Phase 2: Blue-Green Deployment Strategy

### 2.1 Architecture Overview

```
Production Server:
├── Port 80/443: Nginx Reverse Proxy (Traffic Router)
├── Blue Environment (Currently Active)
│   ├── API: Port 5000
│   ├── Frontend: Port 3000
│   └── Database: Port 5432
└── Green Environment (Deployment Target)
    ├── API: Port 5001
    ├── Frontend: Port 3001
    └── Database: Shared with Blue (with migration strategy)
```

### 2.2 Deployment Flow

#### Step 1: Pre-Deployment Validation
```bash
1. Check current production health
2. Backup database
3. Check disk space and resources
4. Verify deployment artifacts
```

#### Step 2: Deploy to Green Environment
```bash
1. Pull latest Docker images
2. Run database migrations (if any) in safe mode
3. Start Green environment containers
4. Wait for health checks to pass
```

#### Step 3: Automated Validation Tests
```bash
1. Health check endpoints (API /health, Frontend /)
2. Smoke tests:
   - User registration/login
   - Friend request workflow
   - Recommendation creation
   - Database connectivity
3. Performance benchmarks:
   - Response time < threshold
   - Memory usage < threshold
   - No error logs
4. API contract tests
```

#### Step 4: Traffic Switching
```bash
1. If all tests pass:
   - Update Nginx to route 10% traffic to Green
   - Monitor for 5 minutes
   - If stable, route 50% traffic
   - Monitor for 5 minutes
   - If stable, route 100% traffic
2. If any test fails:
   - Keep traffic on Blue
   - Alert team
   - Collect logs for debugging
```

#### Step 5: Post-Deployment
```bash
1. Monitor Green environment for 15 minutes
2. Keep Blue running for quick rollback
3. After validation period:
   - Stop Blue environment
   - Tag Green as new Blue
   - Clean up old containers
```

### 2.3 Rollback Strategy
```bash
If issues detected in Green:
1. Immediately switch Nginx back to Blue
2. Stop Green containers
3. Alert team with error logs
4. Investigate and fix issues
```

---

## Phase 3: Implementation Files Needed

### 3.1 GitHub Actions Workflows

**Files to Create:**
1. `.github/workflows/ci.yml` - Main CI pipeline
2. `.github/workflows/cd-staging.yml` - Deploy to staging
3. `.github/workflows/cd-production.yml` - Deploy to production (manual trigger)

### 3.2 Deployment Scripts

**Files to Create:**
1. `deploy-bluegreen.sh` - Main blue-green deployment script
2. `scripts/health-check.sh` - Health validation script
3. `scripts/smoke-tests.sh` - Automated smoke tests
4. `scripts/traffic-switch.sh` - Nginx traffic switching
5. `scripts/rollback.sh` - Emergency rollback script

### 3.3 Docker Configurations

**Files to Update/Create:**
1. `docker-compose.blue.yml` - Blue environment
2. `docker-compose.green.yml` - Green environment
3. `docker-compose.test.yml` - Test environment for CI
4. `nginx-proxy.conf` - Reverse proxy configuration

### 3.4 Test Files

**Backend (C#/.NET):**
1. `api/Recommendo.Api.Tests/` (new directory)
   - `Services/AuthServiceTests.cs`
   - `Services/FriendServiceTests.cs`
   - `Services/RecommendationServiceTests.cs`
   - `Controllers/AuthControllerTests.cs`
   - `Controllers/FriendsControllerTests.cs`
   - `Controllers/RecommendationsControllerTests.cs`
   - `IntegrationTests/ApiIntegrationTests.cs`

**Frontend (TypeScript/React):**
1. `frontend/src/__tests__/` (new directory)
   - `components/Header.test.tsx`
   - `pages/Auth.test.tsx`
   - `pages/Friends.test.tsx`
   - `pages/Home.test.tsx`
   - `api/client.test.ts`
   - `api/friends.test.ts`

### 3.5 Configuration Files

**Files to Create:**
1. `.github/dependabot.yml` - Automated dependency updates
2. `api/Recommendo.Api.Tests/Recommendo.Api.Tests.csproj` - Test project file
3. `frontend/jest.config.js` - Jest configuration
4. `frontend/setupTests.ts` - Test setup
5. `api/.env.test` - Test environment variables
6. `frontend/.env.test` - Frontend test environment

---

## Phase 4: Monitoring & Observability

### 4.1 Logging
- Structured logging in API (Serilog)
- Frontend error tracking (e.g., Sentry integration)
- Centralized log aggregation

### 4.2 Metrics
- Application metrics (response times, error rates)
- Infrastructure metrics (CPU, memory, disk)
- Business metrics (registrations, recommendations)

### 4.3 Alerting
- Failed deployments
- High error rates
- Performance degradation
- Database issues

---

## Phase 5: Implementation Timeline

### Week 1: Foundation
- [ ] Set up test projects
- [ ] Create basic unit tests
- [ ] Set up GitHub Actions workflow structure
- [ ] Configure test databases

### Week 2: Testing
- [ ] Implement backend unit tests (80% coverage target)
- [ ] Implement frontend unit tests
- [ ] Create integration tests
- [ ] Set up code coverage reporting

### Week 3: CI Pipeline
- [ ] Complete GitHub Actions workflows
- [ ] Add security scanning
- [ ] Set up branch protection rules
- [ ] Configure PR checks

### Week 4: Blue-Green Deployment
- [ ] Create deployment scripts
- [ ] Set up blue-green infrastructure
- [ ] Implement health checks
- [ ] Create smoke test suite

### Week 5: Validation & Documentation
- [ ] Test complete CI/CD pipeline
- [ ] Write deployment runbooks
- [ ] Train team on new process
- [ ] Document rollback procedures

---

## Phase 6: Production Deployment Configuration

### 6.1 Nginx Reverse Proxy Setup

```nginx
# /etc/nginx/sites-available/recommendo
upstream blue_api {
    server localhost:5000;
}

upstream green_api {
    server localhost:5001;
}

upstream blue_frontend {
    server localhost:3000;
}

upstream green_frontend {
    server localhost:3001;
}

# Traffic routing based on weight
server {
    listen 80;
    server_name yourdomain.com;

    location /api/ {
        # Weight determines traffic split
        # Initially: 100% blue, 0% green
        proxy_pass http://blue_api;
        
        # Health checks
        health_check interval=5s fails=3 passes=2;
    }

    location / {
        proxy_pass http://blue_frontend;
        health_check interval=5s fails=3 passes=2;
    }
}
```

### 6.2 Database Migration Strategy

**Backward-Compatible Migrations:**
1. Always make migrations backward-compatible
2. Never drop columns immediately
3. Use feature flags for schema changes
4. Run migrations before deployment

**Migration Flow:**
```
1. Deploy migration (adds new columns, keeps old)
2. Deploy Green with new code
3. Validate Green works
4. Switch traffic to Green
5. After 24h stability: Remove old columns in next deployment
```

---

## Phase 7: Security Considerations

### 7.1 Secrets Management
- Use GitHub Secrets for CI/CD
- Never commit secrets to repository
- Rotate secrets regularly
- Use environment-specific secrets

### 7.2 Access Control
- Limit who can trigger production deployments
- Require PR reviews before merge
- Use signed commits
- Enable 2FA for all team members

### 7.3 Dependency Scanning
- Automated vulnerability scanning
- Regular dependency updates via Dependabot
- Security advisories monitoring

---

## Phase 8: Success Metrics

### 8.1 CI/CD Performance
- Time from commit to deployment: < 15 minutes
- Test success rate: > 95%
- Deployment success rate: > 99%
- Mean time to rollback: < 2 minutes

### 8.2 Quality Metrics
- Code coverage: > 80%
- Zero critical security vulnerabilities
- All tests passing before merge
- Automated checks prevent bad merges

### 8.3 Business Metrics
- Deployment frequency: Multiple times per day
- Lead time for changes: < 1 day
- Mean time to recovery: < 15 minutes
- Change failure rate: < 5%

---

## Next Steps

1. **Review this plan** with the team
2. **Prioritize phases** based on immediate needs
3. **Assign ownership** for each phase
4. **Set up development environment** for testing
5. **Begin with Phase 1** (GitHub Actions)

---

## Resources Needed

### Infrastructure
- GitHub Actions minutes (or self-hosted runners)
- Production server with capacity for blue-green environments
- Monitoring/observability tools (optional but recommended)

### Team
- DevOps engineer for infrastructure setup
- Developers for test implementation
- QA for validation test scenarios

### Tools
- GitHub (version control and CI/CD)
- Docker & Docker Compose
- Nginx (reverse proxy)
- PostgreSQL (database)
- Optional: Sentry (error tracking), Datadog (monitoring)

---

## Questions to Address

1. **Server Resources**: Does production server have capacity for running both blue and green?
2. **Database Strategy**: Should we use separate databases or shared with migration strategy?
3. **Domain/SSL**: Do we need SSL certificate management in the deployment?
4. **Monitoring**: What level of observability do we want?
5. **Staging Environment**: Should we have a permanent staging environment separate from blue-green?
6. **Manual Approval**: Should production deployments require manual approval in GitHub Actions?

---

## Risk Mitigation

### Risk: Database Migration Failures
- **Mitigation**: Test migrations in staging first, always backup before migration, use backward-compatible migrations

### Risk: Both Environments Using Same Database
- **Mitigation**: Use connection pooling, ensure migrations are non-blocking, monitor database load

### Risk: Incomplete Rollback
- **Mitigation**: Automated rollback scripts, keep blue environment running during validation period

### Risk: Traffic Routing Issues
- **Mitigation**: Test traffic switching in staging, implement gradual rollout, monitor error rates

### Risk: Test Flakiness
- **Mitigation**: Identify and fix flaky tests immediately, retry logic for integration tests, isolated test environments

---

## Appendix: Example Commands

### Manual Blue-Green Deployment
```bash
# Deploy to green
./deploy-bluegreen.sh deploy green

# Run validation
./scripts/smoke-tests.sh green

# Switch traffic (gradual)
./scripts/traffic-switch.sh green 10
sleep 300
./scripts/traffic-switch.sh green 50
sleep 300
./scripts/traffic-switch.sh green 100

# Cleanup blue
./deploy-bluegreen.sh cleanup blue
```

### Emergency Rollback
```bash
# Instant rollback to blue
./scripts/rollback.sh blue

# Check status
./scripts/health-check.sh blue
```

### Running Tests Locally
```bash
# Backend tests
cd api/Recommendo.Api.Tests
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd frontend
npm test -- --coverage

# Integration tests
docker-compose -f docker-compose.test.yml up -d
./scripts/smoke-tests.sh test
docker-compose -f docker-compose.test.yml down
```
