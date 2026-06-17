🏢 First ERP System - Full Stack .NET + React + Microservices

A comprehensive, production-ready Enterprise Resource Planning (ERP) system built with a modern tech stack. This project demonstrates advanced software architecture evolution, from monolithic development to microservices, including real-world third-party integrations and Docker containerization.
📂 Repository Structure (Monorepo)

This repository contains both the main ERP system and the evolving Microservices architecture:

    FirstErpSystem.Api/ - The main .NET 8 Monolithic Backend API.
    FirstErpSystem.React/ - The modern React 18 + Vite Frontend SPA.
    FirstErpSystem.Frontend/ - The legacy jQuery/AJAX dashboard (Phase 3).
    ErpMicroservices/ - The new Clean Architecture Microservices solution (Phase 7).

🛠️ Tech Stack

Backend:

    .NET 8 Web API
    Entity Framework Core 8 (Code-First Approach)
    SQL Server (Dockerized)
    JWT (JSON Web Token) Authentication & Role Authorization
    Clean Architecture & CQRS Pattern (MediatR) [Phase 7]

Frontend:

    React 18 + Vite (Modern SPA)
    Axios & React Router DOM
    Bootstrap 5 & Bootstrap Icons
    Context API for State Management
    jQuery + AJAX (Legacy Dashboard - Phase 3)

DevOps & Architecture:

    Docker & Docker Compose (Multi-container orchestration)
    Nginx (Reverse Proxy for React)
    GitHub Actions (CI/CD Pipeline)
    Clean Architecture (Domain-driven design) [Phase 7]
    CQRS Pattern (MediatR) [Phase 7]
    YARP API Gateway (Microservices Routing) [Phase 7]

Third-Party Integrations:

    MailKit (Automated Email Notifications)
    BulkSMS BD API (SMS Alerts)
    SSLCommerz, bKash, Nagad (Payment Gateways)

✨ Core Features

    🔐 Authentication & Authorization: JWT-based secure login with Role management (Admin, Manager, Staff).
    📦 Inventory Management: Product CRUD, Stock IN/OUT, Low stock alerts, Reorder level tracking.
    🛒 Purchase Orders (PO): Draft → Approved → Received workflow. Auto Stock-IN upon receiving items.
    💰 Sales Orders (SO): Draft → Confirmed → Invoiced → Paid workflow. Auto Stock-OUT upon payment.
    📧 Smart Notifications: Automatic Email to suppliers/customers on Approve/Invoice. SMS alerts on successful payments.
    💳 Payment Integration: Support for Cash, Bank Transfer, SSLCommerz, bKash, and Nagad.
    🐳 Dockerized: One command (docker compose up) to run the entire stack (SQL Server + API + React + Nginx).
    🏗️ Microservices Ready: Independent Inventory Service with API Gateway routing.

🏗️ Architecture Journey (The 7 Phases)

This project was built incrementally to simulate real-world enterprise software evolution:

    Phase 1: Environment Setup & Health Check API (.NET 8 + Docker SQL Server).
    Phase 2: Employee API with EF Core, SQL Server Migrations & JWT Authentication.
    Phase 3: Classic Frontend Dashboard using HTML/CSS, jQuery & AJAX.
    Phase 4: Core ERP Modules (Inventory, PO, SO) + Email (MailKit), SMS (BulkSMS), Payment (SSLCommerz) Integrations.
    Phase 5: Migration to Modern SPA using React 18, Vite, Context API & Axios.
    Phase 6: Containerization with Docker, Multi-stage builds, Nginx Proxy & GitHub Actions CI/CD.
    Phase 7: Scaling to Microservices with Clean Architecture, CQRS (MediatR), and YARP API Gateway.

🚀 Getting Started
Prerequisites

    .NET 8 SDK
    Node.js 20+
    Docker & Docker Compose
    SQL Server (or use the Docker container)

Option 1: Run Full Monolith ERP with Docker (Recommended)
Clone the repository:

git clone https://github.com/himel-sarker/FirstErpSystem.git

cd FirstErpSystem

Create a .env file in the root directory (see .env.example for reference) with your secrets.
Start the application:

docker compose up --build

Access the app:

     Frontend (React): http://localhost
     Backend Swagger: http://localhost:5123/swagger

Option 2: Run Locally (Development)
Start SQL Server Docker container:
 
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
 
Update appsettings.Development.json with your local DB credentials.
Start Backend:
 
cd FirstErpSystem.Api
dotnet ef database update
dotnet run
 

Start Frontend:

cd FirstErpSystem.React
npm install
npm run dev
 
Option 3: Run the Phase 7 Microservice Locally
Navigate to the ErpMicroservices folder:

cd ErpMicroservices
 
Update InventoryService.Api/appsettings.json with your local DB credentials, then apply migrations and run:

dotnet ef database update --project InventoryService.Infrastructure --startup-project InventoryService.Api
cd InventoryService.Api
dotnet run

     Microservice Swagger: http://localhost:<PORT>/swagger

🔒 Security Best Practices

     Secrets (Passwords, API Keys) are never committed to the repository.
     appsettings.Development.json is ignored via .gitignore for local development.
     Docker uses .env file to inject secrets at runtime via Environment Variables.

👨‍💻 Author

Himel Sarker

     📧 Email: himelsarker.softdev@gmail.com
     💼 LinkedIn: https://www.linkedin.com/in/himel-sarker/
     🐙 GitHub: https://github.com/himel-sarker

📝 License

This project is licensed under the MIT License - see the LICENSE file for details.
