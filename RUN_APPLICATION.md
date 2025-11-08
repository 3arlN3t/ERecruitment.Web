# How to Run the ERecruitment Web Application

## Prerequisites

✅ **Already Done:**
- Docker is installed and running
- SQL Server container is running on `localhost:1433`
- Database connection: `Server=localhost,1433;Database=ERecruitment;User Id=sa;Password=YourStrongPassword123;TrustServerCertificate=True;`

---

## Starting the Application

### Option 1: Using dotnet CLI (Recommended for Development)

```bash
cd /home/ole/Documents/ERecruitment.Web
dotnet run
```

**Default Ports:**
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Option 2: Using dotnet run with specific port

```bash
cd /home/ole/Documents/ERecruitment.Web
dotnet run --urls "http://localhost:5000"
```

### Option 3: Using IIS Express (if installed)

```bash
dotnet run --launch-profile IIS
```

---

## Application URLs

Once the application is running, you can access it at:

### Main Application
- **Home Page:** `http://localhost:5000/`
- **Dashboard:** `http://localhost:5000/applicant/dashboard`

### Authentication
- **Login:** `http://localhost:5000/account/login`
- **Register:** `http://localhost:5000/account/register`

### Admin Features
- **Admin Dashboard:** `http://localhost:5000/admin`
- **Applications Management:** `http://localhost:5000/admin/applications`

---

## Default Credentials

### Admin Account
- **Email:** admin@example.com (create during first registration)
- **Password:** (Use your own secure password)
- **Access Code:** admin123

### Test Applicant Account
- **Email:** test@example.com
- **Password:** TestPassword123

---

## Verification Steps

### 1. Verify Application is Running

```bash
# Check if the application is listening on port 5000
netstat -tlnp | grep 5000

# Or use curl to test the home page
curl http://localhost:5000/
```

### 2. Check Database Connection

The application will automatically:
1. Create the database if it doesn't exist
2. Run all pending migrations
3. Seed initial data if needed

### 3. Verify SQL Server Connection

```bash
# Test SQL Server connection
docker exec erecruitment-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "SELECT @@VERSION"
```

### 4. Check Logs

Application logs are displayed in the console. Look for:
```
Application started.
Press Ctrl+C to shut down.
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/
```

---

## Common Ports Used

| Component | Port | Service |
|-----------|------|---------|
| Web Application | 5000 | ASP.NET Core (HTTP) |
| Web Application | 5001 | ASP.NET Core (HTTPS) |
| SQL Server | 1433 | Database |
| IIS | 80 | If using IIS Express |

---

## Stopping the Application

Press `Ctrl+C` in the terminal where the application is running.

```bash
Ctrl+C
```

To kill the process if it's running in the background:

```bash
pkill -f "dotnet run"
```

---

## Configuration

### Database Connection String

Located in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ERecruitment;User Id=sa;Password=YourStrongPassword123;TrustServerCertificate=True;"
  }
}
```

### Logging Configuration

Located in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

## Troubleshooting

### Port Already in Use

```bash
# Find what's using port 5000
sudo lsof -i :5000

# Kill the process
sudo kill -9 <PID>
```

### Database Connection Error

```bash
# Verify SQL Server is running
docker ps | grep erecruitment-sqlserver

# Restart SQL Server if needed
docker stop erecruitment-sqlserver
docker start erecruitment-sqlserver
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### No Migrations Applied

```bash
# Apply pending migrations
dotnet ef database update
```

---

## Application Features

Once running, you can access:

### For Applicants
- ✅ User registration and authentication
- ✅ Job search and browsing
- ✅ Application submission
- ✅ Killer question screening
- ✅ Profile management
- ✅ Application status tracking

### For Administrators
- ✅ Application management
- ✅ Bulk operations
- ✅ Export functionality
- ✅ Analytics and reporting
- ✅ System administration

---

## Error Handling (Phase 2)

The application now includes comprehensive exception-based error handling:

### Error Pages
- **404 Not Found:** Resource not found
- **401 Unauthorized:** Authentication required
- **403 Forbidden:** Permission denied
- **422 Unprocessable Entity:** Validation errors
- **500 Internal Server Error:** System errors

### Error Messages
All errors are logged to the console and include:
- Timestamp
- Error type
- Request details
- Full stack trace (development only)

---

## Next Steps

1. ✅ Start the application: `dotnet run`
2. ✅ Open browser: `http://localhost:5000`
3. ✅ Register a test account
4. ✅ Test the application workflows
5. ✅ Check error handling by triggering error scenarios

---

## Support

For issues or questions:
1. Check the console logs
2. Verify database is running
3. Review error pages
4. Check the Phase 2 completion report

---

**Last Updated:** November 7, 2025  
**Status:** ✅ Ready to Run

