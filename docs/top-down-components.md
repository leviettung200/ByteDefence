# Top-Down Component Explanation

This guide walks from the solution level down to the important classes and methods so you can trace how a request moves through ByteDefence.

## Solution map
- `ByteDefence.Api` – Azure Functions host with the GraphQL endpoint and domain services.
- `ByteDefence.SignalR` – ASP.NET Core app that hosts the SignalR hub used for real-time updates.
- `ByteDefence.Web` – Blazor WebAssembly frontend that calls the GraphQL API and listens to SignalR.
- `ByteDefence.Shared` – Shared models/DTOs (orders, users, payload records) referenced by all projects.

## Backend: ByteDefence.Api (Azure Functions + HotChocolate)
- **Startup (`Program.cs`)**
  - Registers CORS middleware, configures EF Core (in-memory or Cosmos), Application Insights, and all services.
  - Wires GraphQL server with query/mutation types, data loader, filtering/sorting, and custom `GraphQLErrorFilter`.
- **HTTP entry (`Functions/GraphQLFunction.Run`)**
  - Handles GET for Playground info; POST deserializes the GraphQL body.
  - `ExtractUserFromToken` reads Bearer JWT, validates it, and injects `CurrentUser`/`CurrentRole` into request global state.
  - Builds an `OperationRequest` (variables, operation name) and executes it with the HotChocolate executor.
- **Database (`Data/AppDbContext`)**
  - Defines DbSets for `Order`, `OrderItem`, `User`; applies constraints and seeds deterministic data (admin/user, sample orders/items).
- **Services**
  - `AuthService.GenerateToken/ValidateToken` issues and verifies JWTs with issuer/audience checks.
  - `UserService.GetByIdAsync/GetByUsernameAsync/ValidateCredentialsAsync` fetches users and verifies BCrypt password hashes.
  - `OrderService` methods orchestrate data + notifications:
    - `GetAllAsync/GetByIdAsync` include items and creators.
    - `CreateAsync/UpdateAsync/DeleteAsync` mutate orders and broadcast changes.
    - `AddItemAsync/RemoveItemAsync/GetOrderOwnerByItemIdAsync` handle item-level changes and authorization helpers.
  - Notification implementations (`LocalNotificationService`, `AzureSignalRNotificationService`) expose `BroadcastOrderCreated/Updated/Deleted`, posting to the SignalR hub (local HTTP endpoint or Azure SignalR REST).
- **GraphQL schema**
  - Queries (`OrderQueryResolver`):
    - `GetOrders` filters by caller role (admin vs owner) with HotChocolate filtering/sorting.
    - `GetOrder` enforces owner/admin rule.
    - `GetMe` returns current user.
    - `GetOrderStats` runs four concurrent DbContextFactory queries (orders/users/pending/total value) with `Task.WhenAll`.
  - Mutations:
    - `AuthMutationResolver.Login` validates credentials and returns `LoginPayload` with JWT.
    - `OrderMutationResolver.CreateOrder/UpdateOrder/DeleteOrder/AddOrderItem/RemoveOrderItem` perform CRUD with inline validation and owner/admin checks, returning typed payloads with error messages.
  - Types:
    - `OrderType` projects fields and uses `OrderResolvers.GetCreatedByAsync` with `UserByIdDataLoader` to avoid N+1 lookups; exposes computed `Total`.
    - `OrderItemType` adds `subtotal` resolver; hides navigation IDs.
    - `UserType` hides password hash and orders.
  - DataLoader: `UserByIdDataLoader.LoadBatchAsync` batches user lookups via DbContextFactory.
- **Middleware**
  - `CorsMiddleware` handles OPTIONS preflight and appends allowed origin/headers/methods to responses.
- **Error handling**
  - `GraphQLErrorFilter.OnError` maps exceptions to GraphQL codes (UNAUTHENTICATED/FORBIDDEN/INVALID_OPERATION/etc.) and hides internal details outside Development.

## Real-time: ByteDefence.SignalR
- **Startup (`Program.cs`)**
  - Configures JWT bearer auth (supports access_token in query for SignalR), CORS, and SignalR.
  - Maps `NotificationHub` at `/hubs/notifications` and a POST `/api/broadcast` endpoint that forwards messages from the Functions app to clients (optional header key guard).
  - Health endpoint `/health`.
- **Hub (`Hubs/NotificationHub`)**
  - Logs `OnConnectedAsync/OnDisconnectedAsync`.
  - Group helpers `JoinOrderGroup/LeaveOrderGroup` for per-order updates and `JoinAllOrdersGroup/LeaveAllOrdersGroup` for list updates.

## Frontend: ByteDefence.Web (Blazor WASM)
- **Composition (`Program.cs`)** wires HttpClient, Blazored LocalStorage, auth state provider, GraphQL client, order service, SignalR service, and local `OrderState`/`ToastService`.
- **Auth**
  - `AuthService.Get/Set/ClearToken` persists JWT in local storage and notifies subscribers.
  - `CustomAuthStateProvider.GetAuthenticationStateAsync` reads token, checks expiry, builds claims principal, and clears invalid tokens.
- **GraphQL client/services**
  - `GraphQLClient.QueryAsync/MutateAsync` posts GraphQL requests to configured `Api:Url`, attaching Bearer token when present.
  - `OrderService` wraps concrete queries/mutations (`GetOrdersAsync`, `GetOrderAsync`, `CreateOrderAsync`, `UpdateOrderAsync`, `DeleteOrderAsync`, `AddOrderItemAsync`, `RemoveOrderItemAsync`, `GetOrderStatsAsync`) and maps responses into DTOs for UI use.
- **Real-time client**
  - `SignalRService` builds a reconnecting hub connection to `SignalR:HubUrl`, exposes events `OnOrderCreated/Updated/Deleted` and group join/leave helpers; raises connection state changes.
  - `OrderState` (in-memory cache) and `ToastService` are updated from both GraphQL responses and SignalR events.
- **Pages/components**
  - `Login` exchanges credentials for JWT via `login` mutation then stores the token.
  - `Orders` page loads list via `OrderService`, subscribes to SignalR (all-orders group), and shows loading/error/empty states while reflecting live updates.
  - `OrderDetail` fetches a single order, joins its SignalR group, and lets users remove/add items or delete the order.
  - `CreateOrder`/`EditOrder` issue corresponding mutations and navigate back to detail/list with toasts.
  - Shared UI such as `ConnectionStatus` reflect hub state.

## Shared library: ByteDefence.Shared
- **Models** `Order`, `OrderItem`, `User` with enums `OrderStatus`/`UserRole`; `Order.Total` computes line totals.
- **DTOs** GraphQL inputs/payloads (e.g., `CreateOrderInput`, `LoginPayload`), notification DTOs, and `SignalRMessage` contract reused by API and hub.

## End-to-end request paths
- **Login**: `Login` page ➜ `AuthMutationResolver.Login` ➜ `AuthService.GenerateToken` ➜ token stored by `AuthService` (frontend) ➜ `CustomAuthStateProvider` refreshes identity.
- **GraphQL query/mutation**: Blazor page calls `OrderService` ➜ `GraphQLClient.ExecuteAsync` with Bearer token ➜ `GraphQLFunction.Run` injects user into HotChocolate global state ➜ resolvers enforce ownership/role checks ➜ `OrderService` (API) writes DB and notifies SignalR ➜ `SignalRService` (frontend) receives update and `OrderState`/toasts refresh UI.
