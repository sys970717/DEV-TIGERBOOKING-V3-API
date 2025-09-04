# Admin / channel.md — 스펙 템플릿

## 1) 개요(Overview)
- **목적**: 관리자(Admin)가 채널과 1-depth 서브채널을 생성/조회/수정/비활성/삭제(소프트)할 수 있게 한다.
- **범위**: 채널 메타(코드/이름/계약일자/ratio/정렬순서/활성화 상태) 관리. 다층(2+ depth)은 **미포함**.
- **비고**: DB에는 **FK를 생성하지 않음**, 무결성은 애플리케이션(EF Core)에서 보장.

## 2) 용어(Glossary)
- **채널(Channel)**: 서비스 운영 단위. 루트 또는 서브(부모 1-depth).
- **루트 채널**: `parent_channel_id = NULL`.
- **서브 채널**: `parent_channel_id != NULL`이며, 그 부모는 반드시 루트.

## 3) 역할/권한(Roles & Permissions)
- **Platform Admin**: 모든 채널 CRUD 가능.
- **Channel Admin**: 특정 채널(및 그 서브)만 조회/수정 가능. 루트 생성/삭제 권한은 정책에 따라 제한 가능.
- 권한 체크는 API 진입 시 토큰/세션의 클레임으로 평가.

## 4) 라우트(관리자 화면)
- 목록: `/admin/channels`
- 생성: `/admin/channels/new`
- 상세: `/admin/channels/:id`
- 수정: `/admin/channels/:id/edit`

### 5.1 channel 엔티티(참고 스키마)
| 필드 | 타입 | 필수 | 기본값 | 설명 |
|---|---|---:|---|---|
| id | BIGINT | ✔︎ | identity | PK |
| parent_channel_id | BIGINT |  |  | 부모 채널 ID (루트는 NULL) |
| code | VARCHAR(100) | ✔︎ |  | 채널 코드 |
| name | VARCHAR(100) | ✔︎ |  | 채널명 |
| is_active | BOOLEAN | ✔︎ | **TRUE** | 사용 여부(기본 활성) |
| contract_date | DATE |  |  | 계약일자 |
| ratio | DECIMAL(15,6) | ✔︎ | 0 | 비율(0~1 권장, 앱에서 검증) |
| sort_order | INTEGER | ✔︎ | 0 | 정렬 우선순위(오름차순) |
| created_tz | TIMESTAMPTZ | ✔︎ | now() | 생성 시각 |
| updated_tz | TIMESTAMPTZ | ✔︎ | now() | 수정 시각 |
| created_by | VARCHAR(100) | ✔︎ | '' | 생성자 |
| updated_by | VARCHAR(100) | ✔︎ | '' | 수정자 |
| is_deleted | BOOLEAN | ✔︎ | FALSE | 소프트 삭제 플래그 |
| deleted_tz | TIMESTAMPTZ |  |  | 소프트 삭제 시각 |
| deleted_by | VARCHAR(100) |  |  | 소프트 삭제 수행자 |

**인덱스/유니크 규칙**
- 루트 전역 유니크: `code` (조건: `parent_channel_id IS NULL`)
- 서브 유니크: `(parent_channel_id, code)` (조건: `parent_channel_id IS NOT NULL`)
- 조회: `(parent_channel_id, sort_order)`

## 6) 유효성/비즈니스 규칙(Validation & Rules)
1. **깊이 제한**: 서브의 부모는 반드시 루트여야 한다. (부모의 `parent_channel_id`는 NULL이어야 함)
2. **코드 유니크**:  
   - 루트 끼리는 `code` 전역 유니크  
   - 서브는 **동일 부모 내** `code` 유니크
3. **삭제 규칙**: 루트 삭제는 **직계 서브가 있으면 금지**(먼저 서브 정리). 삭제는 기본 **소프트 삭제**.
4. **활성/비활성**:  
   - 루트 비활성 시, 서브 노출 정책은 선택: (A) 허용, (B) 함께 비활성. 기본은 **A(독립)**, 변경 시 정책에 기록.
5. **정렬 규칙**: 동일 부모 내 `sort_order` 오름차순 → `name` 보조 정렬.

## 7) 목록/검색/정렬/페이지네이션
- **필터**: `is_active`, `parent_only(루트만)`, `parent_id`, `code`, `name`, `date_from/date_to(계약일자)`
- **정렬**: 기본 `parent_channel_id ASC, sort_order ASC, name ASC`
- **페이지네이션**: `page`(기본 1), `page_size`(기본 20, 최대 100)

## 8) API 계약(예시)
> 경로는 예시이며, 실제 라우팅은 프로젝트 컨벤션에 맞춘다. 응답은 UTC ISO-8601.

### 8.1 List
- **GET** `/api/admin/channels`
- **Query**: `page, page_size, is_active, parent_only, parent_id, code, name, date_from, date_to`
- **200**:
```json
{
  "items": [
    {
      "id": 1,
      "parent_channel_id": null,
      "code": "MAIN",
      "name": "메인 채널",
      "is_active": false,
      "contract_date": "2025-09-01",
      "ratio": "0.000000",
      "sort_order": 0,
      "created_tz": "2025-09-04T01:23:45Z",
      "updated_tz": "2025-09-04T01:23:45Z"
    }
  ],
  "page": 1,
  "page_size": 20,
  "total": 1
}

### 8.2 Create

POST /api/admin/channels

Body

{
  "parent_channel_id": null,
  "code": "MAIN",
  "name": "메인 채널",
  "contract_date": "2025-09-01",
  "ratio": "0.100000",
  "sort_order": 0,
  "is_active": false
}


Rules: 1-depth 검증, 코드 유니크 검증, 루트/서브 유니크 스코프 적용.

201: 생성된 리소스(위 필드 + id/감사필드)

### 8.3 Retrieve

GET /api/admin/channels/{id}

200: 채널 상세

### 8.4 Update (부분 수정)

PATCH /api/admin/channels/{id}

Body: 변경 필드만 포함(예: name, ratio, sort_order, is_active, contract_date, parent_channel_id)

Rules: 1-depth 보장, 유니크/정합성 재검증

### 8.5 Activate / Deactivate

POST /api/admin/channels/{id}:activate

POST /api/admin/channels/{id}:deactivate

정책에 따른 서브 처리 전략 명시(기본 독립)

### 8.6 Soft Delete

DELETE /api/admin/channels/{id}

Rules: 자식 존재 시 거절. 성공 시 is_deleted = true, deleted_tz/by 기록.

