# User Authentication Service

## Overview

This project is a backend web service for user management, including user authentication, API key generation, and validation. It is designed with extensibility and scalability in mind, offering endpoints for user creation, updates, password validation, and more. The service is secured through API key management to control and protect access to the services.

## Features

- **User Management**: 
  - **Get**: Retrieve user details by ID or fetch all users.
  - **Create**: Add new users.
  - **Update**: Modify existing user information.
  - **Delete**: Remove users from the system.
- **API Key Management**: 
  - **Generate**: Secure API key generation for clients.
  - **Validate**: Verify the validity of provided API keys.
- **Password Validation**: Passwords are hashed using SHA-256 and stored as PasswordHashes in the database, which can also be validated with API.
- **Error Handling**: Return appropriate status codes and error messages for each operation.
- **Asynchronous Operations**: All operations are asynchronous for better performance.

## Architecture and Components

The project follows a modular and scalable architecture with well-separated concerns for easy maintainability.

- **Controllers**: Handle incoming API requests and route them to corresponding services.

  - **ApiKeysController**: Manages API key generation and validation.

  - **UserController**: Manages user-related operations such as retrieving, creating, updating, and deleting users, as well as validating passwords.

- **Services**: Contain the core business logic for processing API key management, user handling, and password validation.

- **Models and DTOs**: Represent the data structures for API requests, responses, and internal data. DTOs are used to encapsulate and simplify data transfer between layers of the application.

- **Extension Methods**: The project uses extension methods for middleware and service management to extend existing classes without modifying their source code. 

- **Utilities**: Helper functions such as ApiKeyGenerator and PasswordHasher are used to generate secure API keys and hash passwords to secure the application.

- **Error Handling**: Catches exceptions such as argument errors, not found errors, and general errors, and returns appropriate responses.

### Design Principles

- **Modular design**: Separates the API logic into controllers and services to maintain a clear separation of concerns.
- **Dependency Injection**: Promotes loose coupling by injecting services into controllers, allowing easy testing and maintenance.
- **Asynchronous programming**: Ensures better responsiveness and non-blocking operations for scalable performance.
- **Consistent error handling**: Provides meaningful error messages and status codes for all endpoints.
- **Route Naming**: Clear and consistent naming conventions for API routes.

## Setup and Deployment

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/downloads)
- (Optional) [Visual Studio 2022 or IntelliJ alternative](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)
- (Optional) [Download SQL Server Management Studio (SSMS) 20.2 or other alternatives](https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms)

### Installation

1. **Clone repository**:
   ```bash
   git clone https://github.com/ZdjelarFilip/UserAuthServiceSolution
   cd UserAuthServiceSolution
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

### Running Locally

1. **Build project**:
   ```bash
   dotnet build
   ```

2. **Run application in one terminal**:
   ```bash
   dotnet run --project UserAuthService/UserAuthService.csproj
   ```

3. **Test application in another terminal**:
   ```bash
   dotnet test UserAuthService.Tests/UserAuthService.Tests.csproj
   ```

The app will be accessible at `http://localhost:5025` (Swagger)


## API Endpoints
![Swagger](https://i.imgur.com/2wYUM2z.png)

To use the APIs, you must authenticate using the keys of an active client. By default, the seeder method populates the database with sample users and API keys. You can use these in the "Authorize" button above the APIs in Swagger.
- You can use this key for testing : *keySampleForTesting*

### ApiKeysController

While authenticated, you can generate new API keys and validate them.

- **Generate API Key**: Generates a new API key for the new clientId.

  ```http
  POST /api/ApiKeys/GenerateApiKey?clientId={clientId}
  ```

- **Validate API Key**: Validates an API key provided in the request.
  ```http
  POST /api/ApiKeys/ValidateApiKey?apiKey={apiKey}
  ```


### UsersController

Here, you can retrieve user information, create, update, and delete users. You can also validate passwords.

- **Get All Users**: Retrieves a list of all users
  ```http
  GET /api/User/GetUsers
  ```

- **Get User by ID**: Retrieves details of a user by their id.
  ```http
  GET /api/User/GetUser?id={userId}
  ```

- **Create User**: Creates a new user in the system. 
  ```http
  POST /api/User/PostUser
  ```

- **Update User**: Updates an existing user by their id.
  ```http
  PUT /api/User/PutUser?id={userId}
  ```

- **Delete User**: Deletes a user by their id.
  ```http
  DELETE /api/User/DeleteUser?id={userId}
  ```

- **Validate Password**: Validates user credentials using username and password.
  ```http
  POST /api/User/ValidatePassword
  ```  

## Error Handling
All endpoints return appropriate HTTP status codes:

- **400 Bad Request**: For invalid parameters or request bodies.
- **404 Not Found**: When a resource (e.g., user) cannot be found.
- **500 Internal Server Error**: For unexpected internal errors.

## Database
Since we are using EF Core Migrations to automatically create the database and populate tables with data (which runs when the app starts), there is no need for external setup scripts to create the databases. It is recommended to use SSMS (or any other alternative) to access the database tables and data. The login credentials for SSMS can be found in the appsettings.json file.

![SSMS](https://i.imgur.com/mQwIeIk.png)

Database with all three tables.

![Database explorer](https://i.imgur.com/VeG9E0R.png)

Migrations history table.

![EF_Migrations](https://i.imgur.com/29Xsubj.png)

API Keys table. Here you can use the **Key** to validate in Swagger.

![API Keys tbl](https://i.imgur.com/QUX8w2u.png)

Users table. Here you can use the Users data and their PasswordHashes.

![Users tbl](https://i.imgur.com/PZlPhUD.png)


## Logging

Logging is done with ApiLoggingMiddleware in the following format and is saved in a log file (in the folder UserAuthService/Logs/) every single day:

        _logger.LogInformation("INFO | {Time} | Client IP: {ClientIp} | Client: {ClientName} |" +
            " Host: {HostName} | API Method: {Method} {Path} | Request Params: {QueryParams} |" +
            " Message: Request received",


For real-time testing we can also watch the logs in the console below.

![Logging in console](https://i.imgur.com/29DIHeU.png)


## Tests
![Tests ran](https://i.imgur.com/okmzYZ4.png)
- **Frameworks used**: xUnit and Moq
- **Coverage**: All Core functionalities (for Controllers and Services) are covered here.
