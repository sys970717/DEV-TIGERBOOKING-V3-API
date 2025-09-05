
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


### Additional Context
- 기술/아키텍처
  - .NET 8 + EF Core + PostgreSQL
  - Clean Architecture 유지: Domain → Application ←(Infra/Presentation). 상위 계층으로 의존성 역류 금지.
  - 모든 엔티티는 BaseEntity 상속, C# 속성은 PascalCase, DB 컬럼은 snake_case로 명시 매핑.
- DB 정책(핵심)
  - 물리적 FK/트리거/캐스케이드 금지. 관계는 “논리적”으로만 관리(애플리케이션에서 검증/보장).
  - 인덱스/유니크 제약만 최소한으로 사용. 소프트삭제(IsDeleted) 고려 시 HasFilter('is_deleted = false') 사용.
  - 삭제는 소프트삭제만 사용(IsDeleted, DeletedTz, DeletedBy). 캐스케이드 삭제 금지.
- EF Core 매핑 규칙
  - 각 엔티티마다 별도 Configuration 생성(IEntityTypeConfiguration<t>).</t>
  - 테이블/스키마 명시(ToTable), 모든 컬럼명 명시(특히 BaseEntity: id, created_tz, updated_tz, created_by, updated_by, is_deleted, deleted_tz, deleted_by).
  - 네비게이션은 선택적. Include 남용 금지. FK 값은 스칼라(예: ChannelId)로 다루고, 필요 시만 연결 설정하며 물리 FK 생성하지 않음.
- 리포지토리/서비스
  - 새 리포지토리/서비스는 반드시 인터페이스(I* 접두) 제공.
  - DbContext는 Infrastructure 내부에 캡슐화. 상위 계층으로 누수 금지.
  - 트랜잭션은 Application 계층에서 경계를 정의(핵심 유스케이스 단위).
- 쿼리/소프트삭제
  - 조회 시 기본적으로 IsDeleted = false 조건을 적용(리포지토리 레벨에서 명시).
  - 유니크 인덱스는 소프트삭제 필터 포함: .HasFilter('is_deleted = false').
- 환경/설정
  - ASPNETCORE_ENVIRONMENT로 appsettings.{env}.json 사용(loc/dev/stg/prd). 수동 변경 전제.
  - API 응답은 camelCase(JSON). 컨트롤러에서 과도한 로직 금지.
- 마이그레이션/DDL
  - DDL은 간략 유지. 필요 시 마이그레이션 생성하되 물리 FK/캐스케이드 생성하지 않음.
  - 스키마별로 분리하고, DBA/타부서 유지보수 고려해 스크립트 가독성/단순성 우선.
- 네이밍/컨벤션
  - 스키마는 용도별(tb_admin 등), 테이블/컬럼은 snake_case.
  - 관계 검증/무결성은 애플리케이션 로직에서 수행하고, DB는 최소 제약만 유지.
- 변경 시 체크리스트
  - 엔티티 추가/변경 → Configuration, DTO, 인덱스(필요 최소), 소프트삭제 필터 반영.
  - 네비게이션/Include 최소화, 성능 문제 시 Projection(Select)로 대체.