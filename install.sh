#!/bin/bash
set -e

# Config Service Installer

INSTALL_DIR="/opt/config-service"
SERVICE_NAME="config-service"
DATA_DIR="/var/lib/config-service"

echo ">>> Starting Config Service Installation..."

# 1. Install Dependencies
echo ">>> Installing dependencies..."
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET SDK..."
    apt-get update
    apt-get install -y wget
    
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x ./dotnet-install.sh
    ./dotnet-install.sh --channel 10.0 --install-dir /usr/local/share/dotnet
    
    # Link to global bin
    ln -sf /usr/local/share/dotnet/dotnet /usr/local/bin/dotnet
    
    # Add to path for root session immediately
    export PATH="$PATH:/usr/local/share/dotnet"
fi

apt-get install -y git

# 2. Setup Directory
echo ">>> Setting up directories..."
mkdir -p "$INSTALL_DIR"
mkdir -p "$DATA_DIR"

# 3. Clone/Copy Service
if [ -d "./ConfigService" ]; then
    echo ">>> Found local source, using it..."
    SOURCE_DIR="."
else
    echo ">>> Cloning Service from Public Repo..."
    TEMP_DIR=$(mktemp -d)
    git clone https://github.com/Jeffe747/ConfigService.git "$TEMP_DIR/ConfigService"
    SOURCE_DIR="$TEMP_DIR/ConfigService"
fi

# Stop service if running to allow update
echo ">>> Stopping existing service (if any)..."
systemctl stop $SERVICE_NAME || true

echo ">>> Building and Publishing Service..."
# Publish directly to install dir
dotnet publish "$SOURCE_DIR/ConfigService/ConfigService.csproj" -c Release -o "$INSTALL_DIR"

if [ -n "$TEMP_DIR" ]; then
    rm -rf "$TEMP_DIR"
fi

# 4. Create Service
echo ">>> Creating Systemd Service..."
cat > /etc/systemd/system/$SERVICE_NAME.service <<EOF
[Unit]
Description=Config Service
After=network.target

[Service]
WorkingDirectory=$INSTALL_DIR
ExecStart=/usr/local/bin/dotnet $INSTALL_DIR/ConfigService.dll
Restart=always
User=root
Environment=ASPNETCORE_URLS=http://*:5001
Environment=DataDirectory=$DATA_DIR
# Database Configuration (Set these on the server or uncomment and set here)
# Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=ConfigService;Trusted_Connection=True;TrustServerCertificate=True;
# Environment=DatabaseProvider=MSSQL

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable $SERVICE_NAME
systemctl start $SERVICE_NAME

echo "============================================"
echo "   INSTALLATION COMPLETE"
echo "============================================"
echo "Service Address: http://$(hostname -I | awk '{print $1}'):5001"
echo "Data Directory:  $DATA_DIR"
echo "============================================"
