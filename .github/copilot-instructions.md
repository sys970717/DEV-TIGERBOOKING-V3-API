# Repository Copilot Instructions

## Global Defaults
- More Important : Q&A language: When chatting with me, always answer in Korean.
- Timezone: Asia/Seoul (KST). Use ISO 8601 timestamps.

---
## Action: GitHub Action Slack Notification – Work Start & End
- **When:**
  - **Start:** When I (the user) ask a question or request that causes you (Copilot) to begin a task.
    → Trigger the `copilot-task-start` GitHub Action.
  - **End:** When the task triggered by my question/request has been fully completed.
    → Trigger the `copilot-task-end` GitHub Action.

**Behavior**:
- At the start and end of a task, you must trigger the corresponding GitHub Action workflow to send a Slack notification.
- The workflow is defined in `.github/workflows/task-slack.yml`.
- It is triggered by a `repository_dispatch` event.

**Implementation Details**:
- To trigger the workflow, send a `POST` request to the GitHub API endpoint: `/repos/sys970717/DEV-TIGERBOOKING-V3-API/dispatches`.
- You must use a GitHub token with `repo` scope for authentication. This token is available in the `GH_TOKEN` environment variable.
- The request body must specify the `event_type` (`copilot-task-start` or `copilot-task-end`) and a `client_payload` containing a `summary` of the task.

**Example `curl` command to trigger the start event:**
```bash
curl -L \
  -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GH_TOKEN" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/sys970717/DEV-TIGERBOOKING-V3-API/dispatches \
  -d '{"event_type":"copilot-task-start","client_payload":{"summary":"Brief summary of the task."}}'
```

- The GitHub Action workflow is responsible for constructing and sending the actual Slack message. The information below is for reference about the workflow's behavior.

**Channel:** `C09DV5N5Z6E`
**Username:** `TB-v3-api-copilot`
**Icon:** `:tiger:`

---


## Project-Specific Rules for DEV-TIGERBOOKING-V3-API
1. Follow the requirements in `.Requirements/` as the single source of truth for features, data models, and flows.
2. Two top-level contexts under `.Requirements/`:
   - `FE/` — end-user (customer-facing) routes and features.
   - `Admin/` — backoffice (administrator) routes and features.
3. Each **domain** is defined as a separate **Markdown** file under the appropriate context.  
   Example: `.Requirements/FE/user.md` defines the User domain for end users.
4. **DB constraints**: Do **not** create DB-level FKs; enforce relations in application code.
5. **Soft delete**: Use `IsDeleted`, `DeletedTz`, `DeletedBy`. No physical cascade deletes.
6. **EF Core mapping**:
   - Each entity has its own `IEntityTypeConfiguration<T>`.
   - Explicitly map table/schema names and all column names (especially BaseEntity fields).
   - Navigation properties are optional; avoid overusing `Include`. Use scalar FK values (e.g., `ChannelId`) and configure relationships only as needed without creating physical FKs.
7. **Repositories/Services**:
   - Every new repository/service must have an interface prefixed with `I` (e.g., `IUserService`, `IOrderRepository`).
   - DbContext is encapsulated within Infrastructure; do not leak it to upper layers.
   - Define transaction boundaries in the Application layer (per core use case).
8. **Queries/Soft delete**:
   - Apply `IsDeleted = false` filter by default in repositories.
   - Unique indexes must include the soft delete filter: `.HasFilter("is_deleted = false")`.
9. **Environments/Settings**:
   - Use `appsettings.{env}.json` with `ASPNETCORE_ENVIRONMENT` (loc/dev/stg/prd). Manual switching only.
   - API responses are in camelCase (JSON). Avoid excessive logic in controllers.
10. **Migrations/DDL**:
    - Keep DDL minimal. Create migrations as needed but do not create physical FKs or cascades.
    - Separate by schema and prioritize script readability/simplicity for DBA/other teams.
11. **Naming/Conventions**:
    - Use schema names by purpose (e.g., `tb_admin`), and snake_case for tables/columns.
    - Enforce relationships and integrity in application logic; keep DB constraints minimal.
12. **Change Checklist**:
    - When adding/changing entities, update Configuration, DTOs, indexes (minimum necessary), and soft delete filters.
    - Minimize navigation properties/Includes; use Projections (Select) for performance issues.
13. **DB Schema**: Refer to "DEV-TIGERBOOKING-V3-API/Table명세서" for table definitions.

## USING DB Context
All databases I will use are PostgreSQL, and each use case is isolated in its own logical schema.

When using the DbContext, please follow these rules:
- Use the appropriate schema-specific DbContext (e.g., `TbAdminDbContext` for `tb_admin` schema).
- Do not use the base `TigerBookingDbContext` directly in application code.
- Ensure that all queries and commands are executed within the context of the correct schema to maintain data integrity and separation.
- Avoid cross-schema queries unless absolutely necessary, and handle them with care to prevent data leakage or integrity issues.
- Always apply the soft delete filter (`IsDeleted = false`) in your queries unless you have a specific reason to include deleted records.
- When defining relationships, do not create physical foreign keys in the database; enforce relationships in the application layer.
- Follow the repository and service patterns as outlined in the project-specific rules to encapsulate data access logic.
- Ensure that any changes to the DbContext or entity configurations are reviewed and tested to maintain consistency across the application.

## Example: Adding a New Entity with Relationships
When adding a new entity that has relationships with existing entities, follow these steps:
1. Define the new entity class in the appropriate domain folder.
2. Create an `IEntityTypeConfiguration<T>` implementation for the new entity to map it
    - Specify table and column names explicitly.
    - Configure indexes, including soft delete filters.
    - Define navigation properties as needed, but avoid creating physical foreign keys.
3. Update the corresponding DbContext to include a `DbSet<T>` for the new entity.
4. Create repository and service interfaces and implementations for the new entity.
5. Ensure that all queries and commands respect the soft delete logic and schema boundaries.
6. Write unit and integration tests to verify the new entity's behavior and relationships.

## Example: User Entity with SocialAuth Relationship
When implementing the `User` entity that has an optional relationship with the `SocialAuth` entity, follow these steps:
1. Define the `User` entity class with a nullable navigation property to `SocialAuth`.
2. Create an `IEntityTypeConfiguration<User>` implementation to map the `User` entity
    - Specify table and column names explicitly.
    - Configure the unique index on `(channel_id, email)` with the soft delete filter.
    - Ignore the `SocialAuth` navigation property to prevent EF Core from creating a shadow foreign key.
3. Update the `TbAdminDbContext` to include a `DbSet<User>`.
4. Create repository and service interfaces and implementations for the `User` entity.
5. Ensure that all queries and commands respect the soft delete logic and schema boundaries.
6. Write unit and integration tests to verify the `User` entity's behavior and its relationship with `SocialAuth`.
