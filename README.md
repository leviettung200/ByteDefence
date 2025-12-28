# ByteDefence GraphQL + Blazor

Azure Functions (isolated) GraphQL API with SignalR notifications and a Blazor WebAssembly client.

## Prerequisites
- .NET SDK 8.0
- Node-free (Blazor WASM only)
- Docker (for compose)

## Quickstart (Docker)
```bash
cd d:/PROJECTS/ByteDefence
"/c/Program Files/dotnet/dotnet" build
docker compose up --build
```
Services:
- API: http://localhost:7071/api/graphql (login at http://localhost:7071/api/auth/login)
- SignalR: ws/http http://localhost:5000/hubs/notifications
- Web UI: http://localhost:8080

## Auth
- Login payload: `{ "username": "admin", "password": "admin123" }` or `user`/`user123`.
- Attach header: `Authorization: Bearer <token>` to GraphQL and SignalR.

## Local (without Docker)
```bash
"/c/Program Files/dotnet/dotnet" run --project src/ByteDefence.SignalR/ByteDefence.SignalR.csproj --urls http://localhost:5000
"/c/Program Files/dotnet/dotnet" run --project src/ByteDefence.Api/ByteDefence.Api.csproj
"/c/Program Files/dotnet/dotnet" run --project src/ByteDefence.Web/ByteDefence.Web.csproj
```
Update `wwwroot/appsettings.json` if you change ports.

## GraphQL samples
Login first to get a token. Then, for Banana Cake Pop/Postman:

Query orders:
```graphql
query {
  orders { id title status updatedAt total items { id name quantity price } }
  orderStats { draft pending approved completed cancelled total }
}
```

Create order:
```graphql
mutation {
  createOrder(input: { title: "New Order", status: Pending, items: [{ name: "Device", quantity: 1, price: 99 }] }) {
    id title status total
  }
}
```

## Real-time
SignalR hub: `/hubs/notifications`. Server emits `OrderCreated`, `OrderUpdated`, `OrderDeleted`. Client joins `order-{id}` groups.

## IaC
`infra/main.bicep` seeds a Function App, SignalR, and Static Web App placeholders for Azure. Parameters: `location`, `environment`, `functionAppName`, `signalrName`, `staticSiteName`.

## Deployment notes
- For Azure Functions container: build with `src/ByteDefence.Api/Dockerfile` and push to ACR, then deploy to Linux Function App with custom container.
- Static files build via `src/ByteDefence.Web/Dockerfile` (nginx).

## Testing headers
- Add `Authorization: Bearer <token>` to GraphQL requests.
- For CORS in Docker, web uses container URLs automatically; local dev defaults to localhost ports.
