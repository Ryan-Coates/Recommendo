# Recommendo

Recommend movies, books, games and more to friends and family.

## Tech stack
- PWA front end (Vite + React + TypeScript)(port 5001)
- .NET 8 API (port 5002)
- PostgreSQL database (port 5003)

All run on Docker and deployable with a single PowerShell command.

## Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- PowerShell (Windows) or PowerShell Core (Mac/Linux)

### Deploy the entire stack

```powershell
.\deploy.ps1
```

That's it! The script will:
1. Build the frontend and API Docker images
2. Start the PostgreSQL database
3. Run database migrations automatically
4. Start all services

### Access the application

- **Frontend**: http://localhost:5001
- **API**: http://localhost:5002
- **Swagger API Docs**: http://localhost:5002/swagger
- **Database**: localhost:5003

### Manual Deployment (if script fails)

If the automated deployment script has issues, you can deploy manually:

```powershell
# 1. Start the database first
docker compose up -d db

# 2. Wait for database to be healthy (30 seconds)
Start-Sleep -Seconds 30

# 3. Build and start the API
docker compose up -d --build api

# 4. Build and start the frontend
docker compose up -d --build frontend

# 5. Check status
docker compose ps
```

### Troubleshooting

**Issue: Build fails with npm errors**
```powershell
# Frontend build failed - try building locally first
cd frontend
npm install
cd ..
docker compose up -d --build frontend
```

**Issue: API build fails**
```powershell
# Check .NET SDK is available
cd api/Recommendo.Api
dotnet restore
dotnet build
cd ../..
```

**Issue: Containers not starting**
```powershell
# Check logs
docker compose logs

# Restart specific service
docker compose restart api
```

**Issue: Port conflicts**
```powershell
# Check if ports are already in use
netstat -ano | findstr "5001 5002 5003"

# If ports are in use, stop other containers or modify docker-compose.yml ports
```

## Development

### Without Docker

#### Backend (.NET API)
```bash
cd api/Recommendo.Api
dotnet restore
dotnet ef database update
dotnet run
```

#### Frontend (React)
```bash
cd frontend
npm install
npm run dev
```

## Deployment Commands

```powershell
# Deploy everything
.\deploy.ps1

# Stop containers
.\deploy.ps1 -Stop

# Stop and remove containers and volumes
.\deploy.ps1 -Down

# View logs
.\deploy.ps1 -Logs

# Clean build artifacts
.\deploy.ps1 -Clean

# Start without rebuilding images
.\deploy.ps1 -NoBuild

# Clean and rebuild everything
.\deploy.ps1 -Clean -Build
```

## The Concept

Don't you hate it when you are chatting to friends and they recommend something to you and you think "wow that sounds good" but when you are looking for something to watch you have forgotten? That's what Recommendo is for - friends can add recommendations, you can recommend them too and flag them as watched.

## Features

### Authentication
- User registration and login
- JWT-based authentication
- Secure password hashing with BCrypt

### Friends Management
- Generate invite links (7-day expiry)
- Accept friend invitations
- View friends list
- Remove friends

### Recommendations
- Create recommendations for friends
- Multiple recommendation types: Movies, Books, Games, TV Shows, Podcasts, Music
- Filter by type and friend
- Track status: Unseen, In Progress, Watched
- Add descriptions and notes
- Delete recommendations

## Tech Details

### Frontend
- **Framework**: Vite + React 18 + TypeScript
- **Routing**: React Router v6
- **Data Fetching**: TanStack Query (React Query)
- **HTTP Client**: Axios
- **PWA**: vite-plugin-pwa with Workbox
- **Styling**: Custom CSS with responsive design

### Backend
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL 15 with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Password Hashing**: BCrypt.Net
- **API Documentation**: Swagger/OpenAPI

### Database Schema
- **Users**: Authentication and profile data
- **Friendships**: Bidirectional friend relationships
- **InviteLinks**: Time-limited invitation tokens
- **Recommendations**: Media recommendations with status tracking
- **RecommendationNotes**: Optional notes on recommendations

## Project Structure

```
Recommendo/
├── api/
│   ├── Dockerfile
│   └── Recommendo.Api/
│       ├── Controllers/        # API endpoints
│       ├── Services/          # Business logic
│       ├── Models/            # Entity models
│       ├── DTOs/              # Data transfer objects
│       ├── Data/              # Database context
│       └── Configuration/     # App settings
├── frontend/
│   ├── Dockerfile
│   ├── nginx.conf
│   └── src/
│       ├── api/              # API client
│       ├── components/       # React components
│       ├── contexts/         # React contexts
│       ├── pages/            # Page components
│       └── types.ts          # TypeScript types
├── docker-compose.yml
├── deploy.ps1               # Deployment script
└── README.md
```

## Environment Variables

### Frontend
- `VITE_API_URL`: API endpoint URL (default: http://localhost:5002)

### Backend
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `JwtSettings__Secret`: JWT signing secret (minimum 32 characters)
- `JwtSettings__Issuer`: JWT issuer
- `JwtSettings__Audience`: JWT audience
- `JwtSettings__ExpiryInMinutes`: Token expiration time

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/me` - Get current user

### Friends
- `GET /api/friends` - Get friends list
- `POST /api/friends/invite` - Generate invite link
- `POST /api/friends/invite/accept` - Accept invite
- `DELETE /api/friends/{id}` - Remove friend

### Recommendations
- `GET /api/recommendations` - Get recommendations (with filters)
- `GET /api/recommendations/{id}` - Get single recommendation
- `POST /api/recommendations` - Create recommendation
- `PUT /api/recommendations/{id}` - Update recommendation status
- `DELETE /api/recommendations/{id}` - Delete recommendation
- `GET /api/recommendations/types` - Get available types

## License

MIT
