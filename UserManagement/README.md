# TradePort Backend Identity Service

This repository contains the backend identity service for the TradePort application. It is built using ASP.NET Core and provides authentication and user management functionalities.

## Table of Contents

- [Getting Started](#getting-started)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Contributing](#contributing)
- [License](#license)

## Getting Started

These instructions will help you set up and run the backend identity service on your local machine for development and testing purposes.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Node.js](https://nodejs.org/) (for frontend development)
- [Google OAuth 2.0 Client ID](https://developers.google.com/identity/sign-in/web/sign-in)

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/TradePortProject/tradeport-backend-identity
   cd tradeport-backend-identity
   ```

2. Restore the dependencies:

   ```bash
   dotnet restore
   ```

3. Set up the database:

   ```bash
   dotnet ef database update
   ```

## Configuration

1. Create an `appsettings.Development.json` file in the `tradeport-backend-identity` directory with the following content:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=tradeportdb;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "Jwt": {
       "Key": "your-jwt-key",
       "Issuer": "https://localhost:3016/",
       "Audience": "https://localhost:3016/"
     },
     "GoogleOAuth": {
       "ClientId": "your-google-client-id"
     }
   }
   ```

2. Update the `DefaultConnection` string with your SQL Server instance details.

3. Replace `your-jwt-key` with a secure key for JWT token generation.

4. Replace `your-google-client-id` with your Google OAuth 2.0 Client ID.

## Running the Application

1. Start the backend service:

   ```bash
   dotnet run
   ```

2. The backend service will be available at `https://localhost:7237`.

## API Endpoints

### User Management

- `POST /api/user/validategoogleuser`: Validate a Google user using their ID token.
- `POST /api/user/registeruser`: Register a new user.

### Swagger

- Swagger UI is available at `https://localhost:7237/swagger`.

## Contributing

Contributions are welcome! Please read the [contributing guidelines](CONTRIBUTING.md) for more information.
