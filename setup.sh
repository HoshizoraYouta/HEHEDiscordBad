#!/bin/bash

# Discord-like Application Setup Script

echo "🚀 Setting up Discord-like Application..."
echo ""

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Check PostgreSQL
if command -v psql &> /dev/null; then
    echo "✅ PostgreSQL found"
else
    echo "⚠️  PostgreSQL not found. Please ensure PostgreSQL is installed and running."
fi

echo ""
echo "📦 Restoring packages..."
dotnet restore

echo ""
echo "🏗️  Building solution..."
dotnet build

echo ""
echo "💾 Setting up database..."
echo "Please ensure PostgreSQL is running and the database 'discordapp' exists."
echo "To create the database, run: createdb discordapp"
echo ""

read -p "Press Enter to run migrations or Ctrl+C to cancel..."

cd src/Backend/DiscordApp.Infrastructure
dotnet ef database update --startup-project ../DiscordApp.API/DiscordApp.API.csproj
cd ../../..

echo ""
echo "✅ Setup complete!"
echo ""
echo "To run the application:"
echo ""
echo "Backend (Terminal 1):"
echo "  cd src/Backend/DiscordApp.API && dotnet run"
echo ""
echo "Frontend (Terminal 2):"
echo "  cd src/Frontend/DiscordApp.Client && dotnet run"
echo ""
echo "Then open the frontend URL in your browser (typically http://localhost:5173)"
echo ""
