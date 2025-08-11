# Backend Context for Survey Application

This document provides a comprehensive overview of the backend structure and functionality of the Survey Application. It is intended to be used as a context for generating a suitable frontend.

## Project Structure

The backend is a .NET Core application built with C#. The main project directory is `Survey`.

```
C:\Project\survey-app\
├───Survey\
│   ├───.gitignore
│   ├───appsettings.json
│   ├───Program.cs
│   ├───Survey.csproj
│   ├───Survey.http
│   ├───Attributes\
│   │   └───RequiredBindingAttribute.cs
│   ├───Controllers\
│   │   ├───LoginController.cs
│   │   ├───QuestionController.cs
│   │   ├───ResponseController.cs
│   │   ├───SurveyController.cs
│   │   ├───SurveyControllerV2.cs
│   │   ├───SurveyResultsController.cs
│   │   └───UserController.cs
│   ├───Data\
│   │   └───AppDbContext.cs
│   ├───Filters\
│   │   └───GlobalExceptionFilter.cs
│   ├───logs\
│   │   ├───log-20250728.txt
│   │   ├───log-20250729.txt
│   │   ├───log-20250730.txt
│   │   ├───log-20250731.txt
│   │   ├───log-20250801.txt
│   │   └───log-20250806.txt
│   ├───MappingProfiles\
│   │   ├───SurveyProfile.cs
│   │   ├───SurveyResponseProfile.cs
│   │   ├───SurveyResultsProfile.cs
│   │   └───UserProfile.cs
│   ├───Middleware\
│   │   └───GlobalExceptionHandlingMiddleware.cs
│   ├───Migrations\
│   │   ├───20250723173106_InitialSqlServerCreate.cs
│   │   ├───20250723173106_InitialSqlServerCreate.Designer.cs
│   │   ├───20250730062624_AddRefreshTokens.cs
│   │   ├───20250730062624_AddRefreshTokens.Designer.cs
│   │   ├───20250730063515_RemovePasswordSaltFromUserModel.cs
│   │   ├───20250730063515_RemovePasswordSaltFromUserModel.Designer.cs
│   │   └───AppDbContextModelSnapshot.cs
│   ├───Models\
│   │   ├───AuthRequestModel.cs
│   │   ├───PagedList.cs
│   │   ├───PaginationParams.cs
│   │   ├───RefreshToken.cs
│   │   ├───Survey.cs
│   │   ├───UserModel.cs
│   │   └───Dtos\
│   │       ├───AuthRequestDto.cs
│   │       ├───AuthResponseDto.cs
│   │       ├───RefreshTokenRequestDto.cs
│   │       └───SurveyDtos.cs
│   ├───Properties\
│   │   └───launchSettings.json
│   ├───Repositories\
│   │   ├───IQuestionRepository.cs
│   │   ├───IRepository.cs
│   │   ├───ISurveyRepository.cs
│   │   ├───ISurveyResponseRepository.cs
│   │   ├───IUnitOfWork.cs
│   │   ├───IUserRepository.cs
│   │   ├───QuestionRepository.cs
│   │   ├───Repository.cs
│   │   ├───SurveyRepository.cs
│   │   ├───SurveyResponseRepository.cs
│   │   ├───UnitOfWork.cs
│   │   └───UserRepository.cs
│   ├───Services\
│   │   ├───ILoginService.cs
│   │   ├───IRefreshTokenService.cs
│   │   ├───ISurveyResultsService.cs
│   │   ├───ISurveyService.cs
│   │   ├───IUserService.cs
│   │   ├───LoginService.cs
│   │   ├───RefreshTokenService.cs
│   │   ├───SurveyResultsService.cs
│   │   ├───SurveyService.cs
│   │   └───UserService.cs
│   ├───surbey-api-client-angular\
│   │   ├───index.ts
│   │   └───package.json
│   └───Swagger\
│       ├───ConfigureSwaggerOptions.cs
│       └───SwaggerDefaultValues.cs
```

## Key Components

### 1. Controllers

- **LoginController.cs**: Handles user authentication (login, registration, logout) and token management (refresh, revoke).
- **SurveyController.cs**: Manages survey creation, retrieval, updates, and deletion. (Admin role required)
- **QuestionController.cs**: Manages questions within a survey (add, update, delete). (Admin role required)
- **ResponseController.cs**: Handles submission and retrieval of survey responses.
- **SurveyResultsController.cs**: Provides endpoints for aggregated survey results, CSV export, and generating shareable links. (Admin role required)
- **UserController.cs**: Provides endpoints for users to view available surveys and their own responses.
- **SurveyControllerV2.cs**: A version 2 of the survey controller, currently a placeholder.

### 2. Services

- **LoginService.cs**: Implements the logic for user authentication and registration.
- **SurveyService.cs**: Implements the core business logic for managing surveys, questions, and responses.
- **SurveyResultsService.cs**: Implements the logic for generating survey results and reports.
- **RefreshTokenService.cs**: Handles the creation, validation, and revocation of refresh tokens.
- **UserService.cs**: Implements user-related business logic.

### 3. Repositories

- The project uses the **Repository and Unit of Work patterns** to abstract data access.
- **IRepository.cs**: A generic repository interface with common data access methods.
- **Repository.cs**: A generic repository implementation.
- **IUnitOfWork.cs**: Defines the contract for the Unit of Work pattern.
- **UnitOfWork.cs**: Implements the Unit of Work pattern to manage transactions.
- Specific repositories for each entity: `SurveyRepository`, `UserRepository`, `SurveyResponseRepository`, `QuestionRepository`.

### 4. Data

- **AppDbContext.cs**: The Entity Framework Core database context, defining the database schema and relationships.
- **Migrations**: EF Core migrations for managing database schema changes.

### 5. Models

- **Survey.cs**: Defines the core domain models: `SurveyModel`, `QuestionModel`, `SurveyResponseModel`, `QuestionResultModel`, `SurveyResultsModel`, `QuestionResultModel`.
- **UserModel.cs**: Defines the user model.
- **RefreshToken.cs**: Defines the refresh token model.
- **Dtos**: Data Transfer Objects used for API communication, separating the API layer from the domain models.
  - **AuthRequestDto.cs, AuthResponseDto.cs, RefreshTokenRequestDto.cs**: DTOs for authentication.
  - **SurveyDtos.cs**: A comprehensive set of DTOs for surveys, questions, and responses, including create, update, and display DTOs.

### 6. Authentication and Authorization

- **JWT (JSON Web Tokens)** are used for authentication.
- The `LoginController` handles token generation.
- The `[Authorize]` attribute is used to protect endpoints.
- Two roles are defined: "Admin" and "User".

### 7. API Versioning

- The API is versioned using `Asp.Versioning.Mvc`.
- The current version is "1.0".
- A version 2.0 is planned, as indicated by `SurveyControllerV2.cs`.

### 8. Error Handling

- **GlobalExceptionHandlingMiddleware.cs**: A global middleware for handling unhandled exceptions.
- **GlobalExceptionFilter.cs**: An exception filter for handling exceptions in MVC actions.

### 9. Configuration

- **appsettings.json**: Contains configuration for the database connection string, JWT settings, and Serilog logging.

### 10. API Client Generation

- The `surbey-api-client-angular` directory contains a generated Angular TypeScript client for the API, created using NSwag. This provides a convenient way to consume the API from an Angular frontend.

## API Endpoints

The following is a summary of the available API endpoints.

### Authentication (`/api/auth`)

- `POST /register`: Register a new user.
- `POST /login`: Log in a user and get JWT and refresh tokens.
- `POST /refresh`: Refresh an access token using a refresh token.
- `POST /revoke`: Revoke a refresh token.
- `GET /user`: Get information about the currently authenticated user.
- `POST /logout`: Log out the current user.

### Surveys (`/api/v{version}/Survey`)

- `POST /`: Create a new survey (Admin).
- `GET /`: Get all surveys (paginated).
- `GET /{id}`: Get a survey by ID.
- `PUT /{id}`: Update a survey (Admin).
- `DELETE /{id}`: Delete a survey (Admin).

### Questions (`/api/surveys/{surveyId}/questions`)

- `POST /`: Add a question to a survey (Admin).
- `GET /`: Get all questions for a survey.
- `POST /{questionId}`: Update a question (Admin).
- `DELETE /{questionId}`: Delete a question (Admin).

### Responses (`/api/surveys/{surveyId}/responses`)

- `POST /`: Submit a response to a survey.
- `GET /`: Get all responses for a survey (Admin).
- `GET /{responseId}`: Get a specific response by ID (Admin).

### Survey Results (`/api/surveys/{surveyId}`)

- `GET /results`: Get aggregated results for a survey (Admin).
- `GET /export/csv`: Export survey responses to CSV (Admin).
- `GET /share-link`: Generate a shareable link for a survey (Admin).

### User (`/api/user`)

- `GET /surveys`: Get all available (active) surveys for the current user.
- `GET /surveys/{id}`: Get a specific active survey by ID.
- `GET /responses`: Get all responses submitted by the current user.

## Database Schema

The database contains the following tables:

- **Users**: Stores user information (email, password hash, role).
- **Surveys**: Stores survey information (title, description, start/end dates, etc.).
- **Questions**: Stores survey questions.
- **SurveyResponses**: Stores user responses to surveys.
- **RefreshTokens**: Stores refresh tokens for JWT authentication.

## Conclusion

This document provides a detailed overview of the backend of the Survey Application. The backend is a well-structured .NET Core application with a clear separation of concerns, robust authentication and authorization, and a comprehensive set of API endpoints for managing surveys, questions, and responses. The generated Angular API client in the `surbey-api-client-angular` directory should be used as a starting point for frontend development.
