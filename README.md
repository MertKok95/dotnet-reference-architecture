# 🚀 .NET Enterprise Reference Architecture

This repository serves as a production-ready, enterprise-grade Backend API reference architecture and cloud blueprint. It is built from the ground up using **.NET 10**, **Clean Architecture**, **Vertical Slice Architecture**, and **CQRS** principles to provide a highly scalable, testable, and loosely coupled foundation for modern distributed applications.

## 🏗️ Architecture & Project Structure

The project strictly follows the Dependency Inversion principle, ensuring that the core domain logic remains independent of external frameworks, databases, and cloud providers. The solution is organized into five main projects:

```text
DotNetCoreLab
├── src
│   ├── DotNetCoreLab.Domain          # Core Entities (Category, Product, User), Domain Events, and Repository Interfaces
│   ├── DotNetCoreLab.Application     # CQRS Features (Commands/Queries), MediatR, RequestValidationPipe, AutoMapper, DTOs
│   ├── DotNetCoreLab.Infrastructure  # EF Core (SQLite), MongoDB, Redis, RabbitMQ, AWS (S3 & SQS), Elasticsearch, JWT TokenService
│   └── DotNetCoreLab.Api             # REST Controllers, Custom Middlewares, GlobalExceptionHandler, Serilog
└── tests
    └── DotNetCoreLab.UnitTests       # Isolated Unit Tests using xUnit and Moq
```

## ✨ Core Engineering & Architectural Practices

This blueprint eliminates traditional "Spaghetti Code" by implementing enterprise-level design patterns:

- **CQRS & Vertical Slice Organization:** Read (Queries) and Write (Commands) operations are segregated and grouped by business features (`Categories`, `Products`, `Users`). Controllers remain thin, delegating all orchestration to MediatR handlers.
- **MediatR Pipeline Validation:** Business rule validation is removed from controllers and entities. Every incoming request passes through a centralized `RequestValidationPipe` behavior powered by **FluentValidation** before reaching the handler.
- **Global Exception Handling:** Try-catch blocks are eliminated from business logic. System-wide exceptions and validation failures are intercepted at the API boundary by `GlobalExceptionHandler` and returned as standardized `ProblemDetails` JSON responses.
- **Event-Driven Architecture & Cloud Messaging:** Side effects (cache invalidation, notifications) are decoupled from primary database transactions using in-memory MediatR notifications (`INotificationHandler`) as well as distributed message brokers via **RabbitMQ** (`RabbitMqPublisher`) and **AWS SQS** (`AwsSqsPublisher`, `AwsSqsConsumer`).
- **Polyglot Persistence & Cloud Storage:** Demonstrates working with relational databases (**SQLite / EF Core**), NoSQL document stores (**MongoDB**), distributed caching (**Redis**), and cloud object storage (**AWS S3**).
- **Centralized Logging & Full-Text Search:** Integrated with **Serilog** and **Elasticsearch** for structured application logging, performance monitoring, and high-speed search indexing.
- **Stateless Security:** Protected endpoints using **JWT (JSON Web Tokens)** generated and validated via a dedicated `TokenService` in the Infrastructure layer.

## 🛠️ Tech Stack

- **Framework:** .NET 10 (C# 14)
- **Architecture:** Clean Architecture, Vertical Slice, CQRS, Event-Driven Design
- **Databases & Caching:** SQLite (Entity Framework Core), MongoDB, Redis
- **Messaging & Cloud:** RabbitMQ, AWS SQS, AWS S3
- **Search & Logging:** Elasticsearch, Serilog
- **Security:** ASP.NET Core Identity / JWT Bearer Authentication
- **Libraries:** MediatR, FluentValidation, AutoMapper
- **Testing:** xUnit, Moq

## 🚀 Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (For running RabbitMQ, Redis, MongoDB, and Elasticsearch containers via `docker-compose.yml`)

### Installation & Execution

1. **Clone the repository:**
   ```bash
   git clone https://github.com/MertKok95/dotnet-reference-architecture.git
   cd dotnet-reference-architecture
   ```

2. **Start Infrastructure Services (Optional - Docker):**
   ```bash
   docker-compose up -d
   ```

3. **Apply EF Core Database Migrations:**
   ```bash
   dotnet ef database update --project src/DotNetCoreLab.Infrastructure --startup-project src/DotNetCoreLab.Api
   ```

4. **Run the API Project:**
   ```bash
   dotnet run --project src/DotNetCoreLab.Api
   ```

5. **Run Unit Tests:**
   ```bash
   dotnet test
   ```

Once the application is running, navigate to `http://localhost:5068/swagger` in your browser to explore and test the API endpoints.
