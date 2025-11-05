#!/bin/bash
# Quick setup script - Run this AFTER adding yourself to docker group

echo "=========================================="
echo "ERecruitment SQL Server Docker Setup"
echo "=========================================="
echo ""

# Check if user is in docker group
if groups | grep -q docker; then
    echo "✓ User is in docker group"
else
    echo "✗ User is NOT in docker group"
    echo "  Please run: sudo usermod -aG docker \$USER"
    echo "  Then log out and log back in, or run: newgrp docker"
    exit 1
fi

# Check if docker is accessible
if docker ps &>/dev/null; then
    echo "✓ Docker is accessible"
else
    echo "✗ Cannot access Docker"
    echo "  Make sure Docker daemon is running: sudo systemctl start docker"
    exit 1
fi

# Check if container exists
if docker ps -a | grep -q erecruitment-sqlserver; then
    echo "✓ Container exists"
    if docker ps | grep -q erecruitment-sqlserver; then
        echo "✓ Container is running"
    else
        echo "Starting existing container..."
        docker start erecruitment-sqlserver
        echo "✓ Container started"
    fi
else
    echo "Creating new SQL Server container..."
    docker run -d --name erecruitment-sqlserver \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
        -e "MSSQL_PID=Developer" \
        -p 1433:1433 \
        mcr.microsoft.com/mssql/server:2022-latest
    echo "✓ Container created"
fi

echo ""
echo "Waiting for SQL Server to be ready..."
sleep 5

# Wait for SQL Server (max 60 seconds)
for i in {1..12}; do
    if docker exec erecruitment-sqlserver /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P YourStrong!Passw0rd \
        -C -Q "SELECT 1" &>/dev/null 2>&1; then
        echo "✓ SQL Server is ready!"
        break
    fi
    if [ $i -eq 12 ]; then
        echo "✗ SQL Server did not become ready in time"
        echo "  Check container logs: docker logs erecruitment-sqlserver"
        exit 1
    fi
    echo "  Waiting... ($i/12)"
    sleep 5
done

echo ""
echo "Running migrations..."
echo ""

# Run migrations
echo "Migrating ApplicationDbContext..."
if dotnet ef database update --context ApplicationDbContext; then
    echo "✓ ApplicationDbContext migrations completed"
else
    echo "✗ ApplicationDbContext migrations failed"
    exit 1
fi

echo ""
echo "Migrating ApplicationIdentityDbContext..."
if dotnet ef database update --context ApplicationIdentityDbContext; then
    echo "✓ ApplicationIdentityDbContext migrations completed"
else
    echo "✗ ApplicationIdentityDbContext migrations failed"
    exit 1
fi

echo ""
echo "=========================================="
echo "✓ Setup completed successfully!"
echo "=========================================="
echo ""
echo "You can now start your application with:"
echo "  dotnet run"
echo ""
echo "The database will be available at:"
echo "  Server: localhost,1433"
echo "  Database: eRecruitment"
echo "  Username: sa"
echo "  Password: YourStrong!Passw0rd"
