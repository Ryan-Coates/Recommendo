# Production Deployment Guide

## Overview
This guide covers deploying Recommendo to a production server (e.g., recommendo.norn.uk).

## Architecture

```
Internet → Cloudflare/Reverse Proxy → Frontend (port 5001)
                                          ↓
                                    Nginx Proxy → API (port 8080/5002)
                                                     ↓
                                                 Database (port 5432)
```

## Prerequisites

1. Linux server with Docker and Docker Compose installed
2. Domain name (recommendo.norn.uk) pointing to your server
3. Cloudflare or reverse proxy configured

## Deployment Steps

### 1. Clone Repository
```bash
git clone <your-repo-url>
cd Recommendo
```

### 2. Configure Environment Variables

Create a `.env.production` file with secure settings:

```bash
# Database
POSTGRES_DB=recommendo
POSTGRES_USER=recommendo
POSTGRES_PASSWORD=<strong-password-here>

# API
JWT_SECRET=<generate-a-strong-secret-min-32-chars>
JWT_ISSUER=RecommendoAPI
JWT_AUDIENCE=RecommendoApp
```

### 3. Update docker-compose.yml for Production

Ensure the following settings in `docker-compose.yml`:

```yaml
api:
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=recommendo;Username=recommendo;Password=<your-db-password>
    - JwtSettings__Secret=<your-jwt-secret>
```

### 4. Deploy

```bash
# On Linux/Mac
chmod +x deploy.sh
./deploy.sh

# Or using docker-compose directly
docker-compose up -d --build
```

### 5. Configure Reverse Proxy

#### Option A: Nginx Reverse Proxy

If using Nginx as a reverse proxy in front of Docker:

```nginx
server {
    listen 80;
    server_name recommendo.norn.uk;
    
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### Option B: Cloudflare Tunnel

```bash
# Install cloudflared
# Configure tunnel to point to localhost:5001
cloudflared tunnel --url http://localhost:5001
```

## How It Works

### Local Development (localhost)
- Frontend detects `window.location.hostname === 'localhost'`
- Makes API calls directly to `http://localhost:5002/api/`

### Production (recommendo.norn.uk)
- Frontend detects non-localhost hostname
- Uses relative URLs: `/api/` 
- Nginx inside frontend container proxies to `http://api:8080/api/`
- Docker network allows frontend→api communication

## Troubleshooting

### Network Error on Production

**Symptom**: Works on localhost but shows "Network Error" on recommendo.norn.uk

**Causes**:
1. API container not running
2. Docker network issue
3. Nginx can't reach `api:8080`

**Solutions**:

```bash
# Check all containers are running
docker-compose ps

# Check logs
docker-compose logs api
docker-compose logs frontend

# Ensure containers are on same network
docker network inspect recommendo_recommendo-network

# Test API from frontend container
docker exec recommendo-frontend-1 wget -O- http://api:8080/api/health
```

### Check API Logs

```bash
# View application logs
docker exec recommendo-api-1 cat /app/logs/recommendo-$(date +%Y%m%d).log

# Or tail live logs
docker-compose logs -f api
```

### CORS Issues

If you see CORS errors, ensure your domain is in the allowed origins in `Program.cs`:

```csharp
policy.WithOrigins("http://localhost:5001", "https://recommendo.norn.uk")
```

## Security Checklist

- [ ] Change default database password
- [ ] Generate strong JWT secret (minimum 32 characters)
- [ ] Enable HTTPS (use Cloudflare or Let's Encrypt)
- [ ] Set ASPNETCORE_ENVIRONMENT=Production
- [ ] Regularly update Docker images
- [ ] Enable database backups
- [ ] Review and rotate secrets periodically

## Monitoring

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f frontend

# Application logs with timestamps
docker exec recommendo-api-1 tail -f /app/logs/recommendo-$(date +%Y%m%d).log
```

### Check Health
```bash
# API health
curl http://localhost:5002/api/health

# Database connection
docker exec recommendo-db-1 pg_isready -U recommendo
```

## Backup and Restore

### Backup Database
```bash
docker exec recommendo-db-1 pg_dump -U recommendo recommendo > backup_$(date +%Y%m%d).sql
```

### Restore Database
```bash
cat backup_20260203.sql | docker exec -i recommendo-db-1 psql -U recommendo recommendo
```

## Updating

```bash
# Pull latest changes
git pull

# Rebuild and restart
docker-compose down
docker-compose up -d --build

# Migrations run automatically on startup
```
