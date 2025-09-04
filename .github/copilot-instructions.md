
1. Q&A language: When chatting with me, always answer in Korean.
2. Architecture guard: Before coding, confirm no Clean Architecture violations (dependency direction, separation of concerns).
3. Stack baseline: .NET 8 + EF Core only.
4. APIs: There are two internal APIs: Admin (관리자) and User (end users).
5. Environments: Use appsettings.{env}.json with env keys: loc(dev local), dev, stg, prd.
6. Entity fields: C# properties PascalCase; map to DB with explicit column names.
7. JSON response: Responses are JSON with camelCase.
8. Interfaces: Every new Service/Repository must have an interface prefixed with I (e.g., IUserService, IOrderRepository).
9. We will keep only three launch profiles (http, https, IIS Express), and configure environments by manually changing the ASPNETCORE_ENVIRONMENT value.
10. When creating an Entity, it must inherit from BaseEntity.

## USING DB Context
All databases I will use are PostgreSQL, and each use case is isolated in its own logical schema.

## Source of Truth
- Read and follow **`.Requirements/`** as the single source of truth for features, data models, and flows.
- Two top-level contexts under `.Requirements/`:
  - `FE/` — end-user (customer-facing) routes and features.
  - `Admin/` — backoffice (administrator) routes and features.
- Each **domain** is defined as a separate **Markdown** file under the appropriate context.  
  Example:

- **DB constraints**: Do **not** create DB-level FKs; enforce relations in application code.
