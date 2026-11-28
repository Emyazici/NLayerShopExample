# NLayerShop

[![CI](https://github.com/Emyazici/NLayerShopExample/actions/workflows/ci.yml/badge.svg)](https://github.com/Emyazici/NLayerShopExample/actions/workflows/ci.yml)

A small e-commerce backend built with a classic **N-Layer architecture** on .NET 8.
It covers products, categories and orders, with validation, RFC 9457 error responses,
filtering, optimistic concurrency, soft delete and an atomic order flow that updates stock.

**Stack:** ASP.NET Core 8 Web API · EF Core 9 · SQL Server · AutoMapper · FluentValidation · Serilog · xUnit · Testcontainers · Docker · GitHub Actions

## Quick start

```bash
git clone https://github.com/Emyazici/NLayerShopExample.git
cd NLayerShopExample
docker compose up -d --build
```

Open **http://localhost:8080/swagger**. In Development the API applies migrations and seeds
4 categories and 21 products on startup. Stop with `docker compose down -v`.

> The Docker setup is for local demos only: it runs in the `Development` environment (Swagger enabled,
> seed data, exception details in 500 responses) and is not suitable for production.

Tests (Docker must be running for the integration tests):

```bash
dotnet test
```

## Architecture

```
            ┌──────────────────────┐
            │   NLayerShop.Api     │  Controllers, exception handling, DI, Swagger, Serilog
            └──────────┬───────────┘
                       │
            ┌──────────▼───────────┐
            │ NLayerShop.Business  │  Services (business rules), validators, mapping, exceptions
            └──────────┬───────────┘
                       │
            ┌──────────▼───────────┐
            │   NLayerShop.Data    │  DbContext, configurations, migrations, generic repository, UoW, seed
            └──────────┬───────────┘
                       │
            ┌──────────▼───────────┐
            │  NLayerShop.Domain   │  Entities (anemic), enums
            └──────────────────────┘

  NLayerShop.Contracts: DTOs and paging/query requests shared by Api and Business
```

Dependencies point downwards only: **Api → Business → Data → Domain** (+ Contracts).

## Endpoints

| Method | Route | Description | Responses |
|---|---|---|---|
| GET | `/api/products` | Paged list with filtering and sorting | 200, 400 |
| GET | `/api/products/{id}` | Product detail (includes `rowVersion`) | 200, 404 |
| POST | `/api/products` | Create product | 201, 400, 422 |
| PUT | `/api/products/{id}` | Full replace, requires current `rowVersion` | 204, 400, 404, 409, 422 |
| DELETE | `/api/products/{id}` | Soft delete | 204, 404 |
| GET | `/api/categories` | Paged list | 200, 400 |
| GET | `/api/categories/{id}` | Category detail | 200, 404 |
| POST | `/api/categories` | Create category | 201, 400 |
| PUT | `/api/categories/{id}` | Update category | 204, 400, 404 |
| DELETE | `/api/categories/{id}` | Soft delete (products are detached) | 204, 404 |
| GET | `/api/orders` | Paged list, optional `status` filter | 200, 400 |
| GET | `/api/orders/{id}` | Order detail with items | 200, 404 |
| POST | `/api/orders` | Place an order (decrements stock) | 201, 400, 404, 409, 422 |
| POST | `/api/orders/{id}/cancel` | Cancel a pending order (restores stock) | 204, 404, 409, 422 |
| POST | `/api/orders/{id}/complete` | Complete a pending order | 204, 404, 409, 422 |

Product list query parameters: `page`, `size` (1–100), `search`, `categoryId`, `minPrice`, `maxPrice`,
`inStock`, `sortBy` (`name`, `price`, `createdAt`), `sortDir` (`asc`, `desc`).

```
GET /api/products?search=mouse&minPrice=10&sortBy=price&sortDir=desc&page=1&size=10
```

The response includes `totalCount`, `totalPages`, `hasNext` and `hasPrevious`.

Errors are returned as `application/problem+json`:
400 validation (with an `errors` dictionary) · 404 not found · 409 concurrency conflict · 422 business rule violation.

## Example: placing an order

```http
POST /api/orders
Content-Type: application/json

{
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 2, "quantity": 1 }
  ]
}
```

```http
HTTP/1.1 201 Created
Location: /api/orders/1

{
  "id": 1,
  "orderNumber": "ORD-20261124-D5E4DB91",
  "status": "Pending",
  "totalAmount": 139.88,
  "createdAt": "2026-11-24T19:30:00",
  "updatedAt": null,
  "items": [
    { "productId": 1, "productName": "Wireless Mouse", "unitPrice": 24.99, "quantity": 2, "lineTotal": 49.98 },
    { "productId": 2, "productName": "Mechanical Keyboard", "unitPrice": 89.90, "quantity": 1, "lineTotal": 89.90 }
  ]
}
```

Asking for more than the available stock returns `422` and nothing is changed:

```json
{ "status": 422, "title": "Business rule violation", "detail": "Insufficient stock for product 'Mouse' (id 2): requested 10, available 3." }
```

## Design decisions

**Why N-Layer?** The domain is small and mostly data-centric, so a layered structure with anemic entities
and business rules in services is the simplest thing that stays readable and testable. It is a deliberate
contrast to my LibraryApp project, which uses Clean Architecture + DDD (rich domain model, domain events)
for a domain that justifies it. Here there is no MediatR, CQRS or domain events on purpose.

**Why Generic Repository + Unit of Work?** The generic repository keeps CRUD access uniform without one
repository class per entity. The Unit of Work is what makes multi-table operations safe: all repositories
share one `DbContext`, and the service decides when to commit.

**Atomic orders.** Placing an order loads all products in one query, checks existence and stock,
decrements stock, snapshots product name and price into order items, and commits everything with
**a single `SaveChangesAsync`**. Either the order, its items and all stock updates are saved, or none are.
Cancelling restores stock in the same way. Snapshots mean later price changes or product deletion never
alter past orders.

**Concurrency.** Products carry a SQL Server `rowversion`. Clients send it back on `PUT`; a stale value
returns `409`. The same token protects stock: if two orders race for the same product, the second one
fails with `409` instead of overselling. A client that gets `409` should re-read the product and retry;
there is deliberately no server-side retry.

**Other choices.** Soft delete with global query filters; `CreatedAt`/`UpdatedAt` set in `SaveChangesAsync`;
reads use `AsNoTracking` + `ProjectTo` so only needed columns are queried; validation runs explicitly in
services (`ValidateAndThrowAsync`), not through MVC auto-validation.

## Project layout

```
NLayerShop.Api/            Web API host
NLayerShop.Business/       Services, validators, mapping, querying extensions
NLayerShop.Contracts/      DTOs, paging and query requests
NLayerShop.Data/           EF Core context, configurations, migrations, repositories, seed
NLayerShop.Domain/         Entities and enums
tests/NLayerShop.UnitTests/         Validators and order rules (SQLite in-memory)
tests/NLayerShop.IntegrationTests/  API tests against SQL Server via Testcontainers
```

## License

MIT
