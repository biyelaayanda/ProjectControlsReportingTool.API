# Project Controls Reporting Tool - API

A comprehensive .NET 8 Web API for project controls reporting with enterprise-grade authentication and security features.

## Features

- **Authentication & Security**
  - JWT token-based authentication
  - HMACSHA512 password hashing
  - Role-based access control (Executive, Manager, GeneralStaff)
  - Rate limiting and DDoS protection
  - Security headers and CORS configuration

- **Database Integration**
  - Entity Framework Core with SQL Server
  - Automated migrations
  - Stored procedures for optimal performance
  - Comprehensive audit logging

- **API Endpoints**
  - User management (registration, authentication)
  - Report generation and management
  - Department and role-based data access
  - Health monitoring

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

### Setup

1. **Database Setup**
   ```bash
   # Update connection string in appsettings.json
   # Run database initialization
   sqlcmd -S "localhost" -d "ProjectControlsReportingToolDB" -E -i "Database\DatabaseInitialization.sql"
   ```

2. **Configuration**
   ```bash
   # Copy appsettings.Development.json and update:
   # - ConnectionStrings:DefaultConnection
   # - JwtSettings:Secret (use a secure 256-bit key)
   ```

3. **Run Application**
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Access**
   - API: `http://localhost:5039/api`
   - Swagger UI: `http://localhost:5039/swagger`
   - Health Check: `http://localhost:5039/api/health`

## Default Test Accounts

| Email | Password | Role | Department |
|-------|----------|------|------------|
| admin@test.com | Password123! | Executive | Project Support |
| manager@test.com | Password123! | Manager | Project Support |
| staff@test.com | Password123! | GeneralStaff | Project Support |

> **Security Note**: Change these default passwords in production!

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/me` - Current user info
- `POST /api/auth/change-password` - Change password
- `PUT /api/auth/profile` - Update profile

### Reports
- `GET /api/reports` - List reports
- `POST /api/reports` - Create report
- `GET /api/reports/{id}` - Get report details
- `PUT /api/reports/{id}` - Update report
- `DELETE /api/reports/{id}` - Delete report

### Users (Admin only)
- `GET /api/users` - List users
- `GET /api/users/{id}` - Get user details
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## Security Features

- **JWT Authentication**: Secure token-based authentication with configurable expiration
- **Password Security**: HMACSHA512 hashing with unique salts per user
- **Rate Limiting**: Configurable request throttling to prevent abuse
- **Security Headers**: OWASP-recommended security headers
- **CORS**: Configurable cross-origin resource sharing
- **Audit Logging**: Comprehensive action tracking for compliance

## Configuration

Key configuration sections in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectControlsReportingToolDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-here",
    "ExpirationInHours": 168
  },
  "RateLimiting": {
    "RequestsPerMinute": 100,
    "BurstSize": 20
  }
}
```

## Production Deployment

1. **Security Checklist**
   - [ ] Update JWT secret key
   - [ ] Change default user passwords
   - [ ] Configure proper CORS origins
   - [ ] Set up HTTPS
   - [ ] Configure logging

2. **Database**
   - [ ] Use production SQL Server instance
   - [ ] Run database initialization script
   - [ ] Set up backup strategy

3. **Monitoring**
   - [ ] Configure health checks
   - [ ] Set up application insights
   - [ ] Monitor rate limiting metrics

## Development

### Project Structure
```
├── Business/Services/          # Business logic services
├── Controllers/               # API controllers
├── Data/                     # Entity Framework context
├── DTOs/                     # Data transfer objects
├── Extensions/               # Extension methods
├── Interfaces/               # Service interfaces
├── Middleware/               # Custom middleware
├── Models/                   # Entity models
├── Repositories/             # Data access layer
└── Database/                 # Database scripts
```

### Adding New Features
1. Create models in `Models/`
2. Add DTOs in `DTOs/`
3. Implement repositories in `Repositories/`
4. Create services in `Business/Services/`
5. Add controllers in `Controllers/`
6. Update database with migrations

## Support

For issues and feature requests, please contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: August 2025
