# ACS Brick Management System

A Blazor Server application for managing bricks, manufacturers, orders, and user access in a role-based workflow.

## Live Application

**Production URL:** https://acs-brick-management.onrender.com/

## Overview

This solution is built with **.NET 10** and **Blazor Server**. It provides CRUD operations across core business entities and includes automated testing (unit, integration, and end-to-end) to support quality and deployment confidence.

## Application Explanation

The ACS Brick Management System is an internal operations platform for construction supply workflows. It centralizes the management of:

- **Manufacturers** (supplier records and contact details)
- **Bricks** (catalog data such as size, material, strength, and pricing)
- **Orders** (multi-line order tracking and fulfillment progress)
- **Users and roles** (access control for Admin, Editor, and Standard users)

After authentication, users work through module-based pages to search, create, update, and manage records. The application links entities together (for example, bricks to manufacturers and orders to bricks) so teams can move from catalog management to procurement tracking in one system.

## Key Features

- Authentication and session persistence for signed-in users
- Role-based access control (Admin, Editor, Standard)
- User management (admin-only)
- Manufacturer management
- Brick catalog management
- Order and delivery tracking
- Search and filtering in data-heavy screens
- Database schema migration on application startup

## Technology Stack

- **Framework:** ASP.NET Core Blazor Server (.NET 10)
- **Database:** PostgreSQL (via Npgsql)
- **Migrations:** FluentMigrator
- **Testing:** xUnit v3, FluentAssertions, bUnit, Playwright, Moq
- **CI/CD:** GitHub Actions
- **Containerization:** Docker

## Solution Structure

```text
SoftwareEngineeringDevOps.slnx
├─ SoftwareEngineeringDevOps/              # Main Blazor Server app
│  ├─ App/                                 # Domain/application logic (Auth, Bricks, Orders, Users, etc.)
│  ├─ Components/                          # Razor components and pages
│  ├─ Migrator/                            # Embedded SQL migrations + schema definitions
│  ├─ wwwroot/                             # Static assets
│  └─ Program.cs                           # DI and app startup pipeline
└─ SoftwareEngineeringDevOps.Tests/        # Unit, integration, and E2E tests
```

## Prerequisites

- **.NET SDK 10.0.x**
- **PostgreSQL** (or compatible hosted PostgreSQL service)
- (Optional) **Playwright browser dependencies** for E2E tests
- (Optional) **Docker** for containerized runs

## Configuration

The application expects database settings through environment variables:

- `DB_HOST`
- `DB_PORT`
- `DB_NAME`
- `DB_USERID`
- `DB_PASSWORD`

> The app builds its database connection string from these variables at runtime.

## Getting Started

### 1) Restore dependencies

```bash
dotnet restore
```

### 2) Set environment variables

Configure the `DB_*` variables for your PostgreSQL instance.

### 3) Run the app

```bash
dotnet run --project SoftwareEngineeringDevOps/SoftwareEngineeringDevOps.csproj
```

By default, local development runs on:

- `http://localhost:5181`
- `https://localhost:7144`

## Database Migrations

Migrations execute during startup (`Program.cs`) to keep schema up to date.

## Testing

Run all tests:

```bash
dotnet test SoftwareEngineeringDevOps.Tests/SoftwareEngineeringDevOps.Tests.csproj
```

Common filtered runs:

```bash
# Unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Integration tests (bUnit)
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# End-to-end tests (Playwright)
dotnet test --filter "FullyQualifiedName~E2E"
```

For detailed test-suite documentation, see:  
`SoftwareEngineeringDevOps.Tests/README.md`

## CI/CD

GitHub Actions workflow: `.github/workflows/test-suite.yml`

Pipeline includes:

- Unit tests
- Integration component tests (bUnit)
- End-to-end tests (Playwright with PostgreSQL service)
- Test artifact publishing (TRX + coverage outputs)

## Deployment

The project includes a production-ready `Dockerfile` and is currently deployed on Render:

https://acs-brick-management.onrender.com/

## Security Notes

- Do not commit real secrets or production credentials.
- Use environment variables or secret stores for sensitive configuration.
- Rotate any exposed credentials immediately if they were ever committed.
