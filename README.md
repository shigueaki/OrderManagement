<img width="1546" height="519" alt="image" src="https://github.com/user-attachments/assets/693bf02a-85f6-43b3-b17f-ac8143162e15" />

<img width="1547" height="601" alt="image" src="https://github.com/user-attachments/assets/da3d2efb-9733-430c-94d7-561207e6c3e7" />


# 📦 Order Management System

A full-stack order management system with real-time updates, built as a technical challenge showcasing modern software architecture patterns.

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3-blue)
![Azure Service Bus](https://img.shields.io/badge/Azure-Service%20Bus-blue)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Running the Project](#-running-the-project)
- [API Documentation](#-api-documentation)
- [Testing](#-testing)
- [Project Structure](#-project-structure)
- [Bonus Features](#-bonus-features)
- [Environment Variables](#-environment-variables)
- [Troubleshooting](#-troubleshooting)

---

## 🎯 Overview

This system allows users to **create**, **list**, and **view orders**. When an order is created, a message is published to **Azure Service Bus** using the **Outbox Pattern** for transactional reliability. A background **Worker** consumes messages, processes orders, and updates their status in real-time via **SignalR WebSockets**.

### Order Status Flow
┌─────────┐     ┌────────────┐     ┌───────────┐ ┌──────────┐ ┌──────────┐ ┌───────────┐
│ Pending │ ──→ │ Processing │ ──→ │ Completed │ │ (create) │ │ (worker) │ │(worker+5s)│ 
└─────────┘     └────────────┘     └───────────┘ └──────────┘ └──────────┘ └───────────┘

---

## 🛠 Tech Stack

<img width="356" height="259" alt="image" src="https://github.com/user-attachments/assets/f5327110-e490-490c-9f46-026e9240d6f2" />

---

### ✨ Features

Core Features

    ✅ Create Orders — Form with validation
    ✅ List Orders — Responsive table with status badges
    ✅ View Order Details — Full details with status history timeline
    ✅ Real-time Updates — SignalR WebSocket notifications
    ✅ Async Processing — Worker processes orders via Service Bus

📋 Prerequisites

Before you begin, make sure you have the following installed:

<img width="356" height="144" alt="image" src="https://github.com/user-attachments/assets/c640f71b-1660-4285-86af-b65cfb393c43" />

Optional

<img width="356" height="144" alt="image" src="https://github.com/user-attachments/assets/ee071513-3ee2-4b59-99bd-cc40b3724c0c" />

Verify Installation

# Check .NET
dotnet --version
# Expected: 8.0.x

# Check Node.js
node --version
# Expected: v18.x.x or higher

# Check npm
npm --version
# Expected: 9.x.x or higher

# Check PostgreSQL
psql --version
# Expected: psql (PostgreSQL) 15.x or higher

# Check Git
git --version

🚀 Installation
Step 1: Clone the Repository

git clone https://github.com/YOUR_USERNAME/OrderManagement.git
cd OrderManagement

Step 2: Setup PostgreSQL Database

Option A: Using the SQL script (Recommended)

# Connect to PostgreSQL and run the init script
psql -U postgres -f database/init.sql

Option B: Using pgAdmin

    Open pgAdmin
    Right-click on Databases → Create → Database
    Name: ordermanagement → Click Save
    Right-click on ordermanagement → Query Tool
    Open and execute database/init.sql

Option C: Manual (minimal)

-- Connect to PostgreSQL
psql -U postgres

-- Create database
CREATE DATABASE ordermanagement;

-- Exit (the API will create tables automatically via EF migrations)
\q

Step 3: Configure Backend

    Create the development settings file:
# Copy the template
copy src\OrderManagement.API\appsettings.json src\OrderManagement.API\appsettings.Development.json

Edit src/OrderManagement.API/appsettings.Development.json:

{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ordermanagement;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  },
  "AzureServiceBus": {
    "ConnectionString": "",
    "QueueName": "orders"
  },
  "Frontend": {
    "Url": "http://localhost:5173"
  }
}

    Replace YOUR_POSTGRES_PASSWORD with your actual PostgreSQL password.

    💡 Azure Service Bus is optional. Leave ConnectionString empty to use the in-memory fallback. The API will log messages locally instead of sending them to a queue.

    If using the Worker, also create:
    copy src\OrderManagement.Worker\appsettings.json src\OrderManagement.Worker\appsettings.Development.json
Edit with the same connection string and Service Bus settings.

Step 4: Install Backend Dependencies

# From the root OrderManagement folder
dotnet restore

Step 5: Run Database Migrations

# Install EF tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration (skip if migration already exists)
dotnet ef migrations add InitialCreate ^
    --project src/OrderManagement.Infrastructure ^
    --startup-project src/OrderManagement.API ^
    --output-dir Persistence/Migrations

# Apply migration
dotnet ef database update ^
    --project src/OrderManagement.Infrastructure ^
    --startup-project src/OrderManagement.API

# Install EF tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration (skip if migration already exists)
dotnet ef migrations add InitialCreate ^
    --project src/OrderManagement.Infrastructure ^
    --startup-project src/OrderManagement.API ^
    --output-dir Persistence/Migrations

# Apply migration
dotnet ef database update ^
    --project src/OrderManagement.Infrastructure ^
    --startup-project src/OrderManagement.API

Note: The API also applies migrations automatically on startup, so this step is optional.

Step 6: Install Frontend Dependencies
cd frontend
npm install
cd ..

▶️ Running the Project

You need 2 terminals (or 3 if using the Worker with Azure Service Bus).
Terminal 1: Start the API
cd src/OrderManagement.API
dotnet run
Expected output:
[INF] Applying database migrations...
[INF] Database migrations applied successfully.
[INF] Order Management API starting...
[INF] 🚀 API is listening on: http://localhost:5000
[INF] 📖 Swagger UI: http://localhost:5000/swagger
[INF] ❤️ Health Check: http://localhost:5000/health

Terminal 2: Start the Frontend
cd frontend
npm run dev
Expected output:
  VITE v5.x.x  ready in 500ms

  ➜  Local:   http://localhost:5173/

Terminal 3 (Optional): Start the Worker

    Only needed if Azure Service Bus is configured.
cd src/OrderManagement.Worker
dotnet run

Expected output:
[INF] Order Consumer Worker starting...
[INF] Order Consumer Worker started. Listening for messages...

Access the Application

<img width="522" height="173" alt="image" src="https://github.com/user-attachments/assets/d597e530-f481-4316-8822-5ffb95079492" />

📡 API Documentation

<img width="607" height="202" alt="image" src="https://github.com/user-attachments/assets/94004147-2388-4699-a08d-5daf31e5cc07" />


Create Order Request
POST /api/orders
Content-Type: application/json

{
  "customerName": "John Doe",
  "productName": "Laptop Pro",
  "value": 2500.00
}

Create Order Response
HTTP/1.1 201 Created

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "productName": "Laptop Pro",
  "value": 2500.00,
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null,
  "statusHistory": [
    {
      "id": "...",
      "status": "Pending",
      "changedAt": "2024-01-15T10:30:00Z"
    }
  ]
}

Get Order Response (after processing)
HTTP/1.1 200 OK

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "productName": "Laptop Pro",
  "value": 2500.00,
  "status": "Completed",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:12Z",
  "statusHistory": [
    {
      "id": "...",
      "status": "Pending",
      "changedAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": "...",
      "status": "Processing",
      "changedAt": "2024-01-15T10:30:05Z"
    },
    {
      "id": "...",
      "status": "Completed",
      "changedAt": "2024-01-15T10:30:12Z"
    }
  ]
}

Validation Error Response
HTTP/1.1 400 Bad Request

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "errors": {
    "CustomerName": ["Customer name is required."],
    "Value": ["Value must be greater than zero."]
  }
}

Testing with cURL
# Create an order
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d "{\"customerName\": \"John Doe\", \"productName\": \"Laptop\", \"value\": 1500.00}"

# List all orders
curl http://localhost:5000/api/orders

# Get order by ID
curl http://localhost:5000/api/orders/YOUR_ORDER_ID_HERE

# Health check
curl http://localhost:5000/health

🧪 Testing
Run All Tests

# From root folder
dotnet test

Run Unit Tests Only
dotnet test tests/OrderManagement.UnitTests

Run Integration Tests Only
dotnet test tests/OrderManagement.IntegrationTests

Run with Verbose Output
dotnet test --verbosity detailed

Run with Code Coverage
dotnet test --collect:"XPlat Code Coverage"
