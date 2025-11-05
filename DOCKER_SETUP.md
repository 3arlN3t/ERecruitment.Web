# Docker SQL Server Setup Guide

## Prerequisites

1. **Add your user to the docker group** (requires sudo):
   ```bash
   sudo usermod -aG docker $USER
   ```
   Then log out and log back in for the changes to take effect.

   **OR** use `sudo` for all docker commands.

2. **Start the SQL Server container**:
   ```bash
   # If you added yourself to docker group:
   ./setup-docker-db.sh
   
   # OR if using sudo:
   sudo docker run -d --name erecruitment-sqlserver \
     -e "ACCEPT_EULA=Y" \
     -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
     -e "MSSQL_PID=Developer" \
     -p 1433:1433 \
     mcr.microsoft.com/mssql/server:2022-latest
   ```

3. **Wait for SQL Server to be ready** (about 10-30 seconds):
   ```bash
   # Check if container is running:
   docker ps | grep erecruitment-sqlserver
   # OR with sudo:
   sudo docker ps | grep erecruitment-sqlserver
   ```

4. **Run migrations**:
   ```bash
   # Migrations will run automatically when you start the application
   # OR run manually:
   dotnet ef database update --context ApplicationDbContext
   dotnet ef database update --context ApplicationIdentityDbContext
   ```

## Connection Details

- **Server**: localhost,1433
- **Database**: eRecruitment
- **Username**: sa
- **Password**: YourStrong!Passw0rd
- **Connection String**: Already configured in `appsettings.json`

## Stopping the Container

```bash
docker stop erecruitment-sqlserver
# OR with sudo:
sudo docker stop erecruitment-sqlserver
```

## Starting Existing Container

```bash
docker start erecruitment-sqlserver
# OR with sudo:
sudo docker start erecruitment-sqlserver
```

## Removing the Container

```bash
docker rm -f erecruitment-sqlserver
# OR with sudo:
sudo docker rm -f erecruitment-sqlserver
```

