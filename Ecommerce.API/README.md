# Ecommerce Backend API (.NET 8)

The backend component of the Ecommerce application serves as a robust, secure, and highly scalable RESTful Application Programming Interface (API) built with **ASP.NET Core 8.0**. 

## 🏗️ Architecture & Patterns

The backend strictly adheres to **Clean Architecture** principles, dividing the solution into three main layers: `Ecommerce.API` (Presentation), `Ecommerce.Core` (Domain/Interfaces), and `Ecommerce.Infrastructure` (Data/External Services).

### Key Design Patterns Used:
*   **Repository & Unit of Work:** Abstracts data access, providing a decoupled interface for database interactions and atomic transactions.
*   **CQRS (Command Query Responsibility Segregation):** Mediated via `MediatR` to separate read and write operations, keeping controllers extremely lean.
*   **Dependency Injection (DI):** Heavy usage of built-in .NET DI container, extended using `Scrutor` for automatic assembly scanning and registration.
*   **DTO Pattern:** (Data Transfer Objects) used tightly coupled with `AutoMapper` to avoid exposing raw domain entities to the client.

## 📦 Libraries & Packages

*   **Entity Framework Core 8:** ORM used for interacting with SQL Server.
*   **MediatR:** For implementing the Mediator pattern and CQRS.
*   **AutoMapper:** For seamless object-to-object mapping.
*   **Serilog:** Used for structured logging to the console and daily rolling text files.
*   **Hangfire:** For managing and dashboarding background jobs.
*   **SignalR:** Enables real-time capabilities for the built-in chat module.
*   **AspNetCore.HealthChecks:** Provides endpoints to monitor the health of SQL Server, Redis, and Hangfire.
*   **Swashbuckle / Swagger:** API Documentation and exploration.

## 🔧 Core Services & Integrations

*   **Identity & Auth:** Uses ASP.NET Core Identity with JWT Bearer Token validation. It also integrates **Google One Tap/Social Login**.
*   **Caching (Redis/Valkey):** Utilized for caching product catalogs and maintaining user basket states.
*   **Messaging (Kafka via Aiven):** Event-driven architecture for distributed messaging and background processes.
*   **Payment Gateway:** Integrated with **Stripe** to process checkouts securely.
*   **Rate Limiting:** Uses .NET 8 built-in `RateLimitingMiddleware` with distinct policies (`CustomerCart`, `Products`, `Authentication`, `Global`) applying different limits.
