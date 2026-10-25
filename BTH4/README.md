# Insurance Agency Microservices

This solution is prepared for "Bai thuc hanh so 4 - Case study cai dat kien truc microservices tren moi truong .NET Core".

## Solution structure

Each microservice is organized into three projects:

- `*.ApiDefinition`: public contracts, commands, queries, events and DTO definitions.
- `*.Implementation`: ASP.NET Core Web API implementation, controllers/adapters and MediatR handlers.
- `*.Tests`: unit and integration tests.

Implemented service folders:

- `AgentPortalApiGateway`
- `AuthService`
- `ChatService`
- `PricingService`
- `PolicyService`
- `PolicySearchService`
- `PaymentService`
- `ProductService`
- `DocumentService`

## Packages

MediatR is installed in every `*.Implementation` project and registered in `Program.cs`:

```csharp
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

## Infrastructure

The `docker-compose.yml` file defines local infrastructure required by the case study:

- PostgreSQL 16
- RabbitMQ 3 with management UI
- Elasticsearch 8 in single-node mode

The `postgres/createdatabases.sql` script creates one database per service.

## Useful commands

```powershell
dotnet restore InsuranceAgency.sln
dotnet build InsuranceAgency.sln
docker compose up -d
```

If GitHub submission is required, push this folder to a GitHub repository and invite `caotxuan@gmail.com`.
