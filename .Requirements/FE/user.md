# FE / user.md — 스펙 템플릿 (JWT + Redis)

## 1) 개요 (Overview)

- **목적**: 최종 사용자(고객)의 회원가입/로그인/프로필 관리.
- **범위**: LOCAL(이메일/비밀번호) 가입·로그인, SNS(OAuth) 가입·로그인, 프로필 조회/수정, 소프트 삭제, **JWT 인증/로그아웃(Redis)**.
- **전제**:
  - **단일 채널 소속** 모델. **가입 시 **`channel_id`**는 임시로 **`1`** 고정**(환경설정으로 변경 예정).
  - DB에는 **FK 미생성**, 무결성은 애플리케이션(EF Core)에서 보장.
  - 이메일/이름/성별/닉네임/전화/국적은 **API 서비스 로직 내에서 암호화(Enc/Dec)** 후 `VARCHAR(500)` 컬럼에 저장.

## 2) 용어 (Glossary)

- **LOCAL**: 이메일+비밀번호 기반 계정.
- **SNS**: Google/Kakao/Naver/Apple 등 OAuth 기반 계정(`social_auth` 레코드로 표현).
- **소프트 삭제**: `is_deleted = true`로 표기하되 데이터는 물리 삭제하지 않음.
- **JWT**: JSON Web Token. 서버 서명(HS/RS)으로 위변조 방지. 본 문서에선 **Access JWT 1종**만 사용(간단 모드).
- **JTI**: JWT ID(고유 식별자). Redis 키로도 사용.

## 3) 라우트 (FE 화면)

- 회원가입: `/auth/register`
- 로그인: `/auth/login`
- 로그아웃: `/auth/logout`
- 내 정보: `/me`
- 프로필 수정: `/profile`
- SNS 로그인/콜백: `/auth/{provider}` → `/auth/{provider}/callback`

## 4) 데이터 모델 (요약)

> 실제 테이블은 `user`, `social_auth` 사용. 컬럼명은 `snake_case`. 모든 테이블에 감사/삭제 공통부 포함.

### 4.1 user

| 필드                                                                               | 타입            | 필수 | 기본값             | 설명                                    |
| -------------------------------------------------------------------------------- | ------------- | -- | --------------- | ------------------------------------- |
| id                                                                               | BIGINT        | ✔︎ | identity        | PK                                    |
| channel\_id                                                                      | BIGINT        | ✔︎ | (가입 시 **1** 고정) | 단일 채널 소속                              |
| email                                                                            | VARCHAR(500)  | ✔︎ |                 | **암호화 저장**(결정적 암호화 권장)                |
| password                                                                         | VARCHAR(100)  |    |                 | 비밀번호 **해시**(Argon2id/bcrypt) — LOCAL만 |
| social\_auth\_idx                                                                | BIGINT        |    |                 | SNS 자격 연결용(논리, FK 없음)                 |
| family\_name / given\_name                                                       | VARCHAR(500)  | ✔︎ |                 | 여권식 성/이름, **암호화 저장**                  |
| gender / nickname                                                                | VARCHAR(500)  |    |                 | **암호화 저장**                            |
| phone\_country\_code / phone\_number                                             | VARCHAR(500)  |    |                 | **암호화 저장**                            |
| nationality\_code                                                                | VARCHAR(500)  |    |                 | **암호화 저장**                            |
| point                                                                            | DECIMAL(15,6) | ✔︎ | 0               | 포인트 잔액                                |
| is\_active                                                                       | BOOLEAN       | ✔︎ | **TRUE**        | 활성 여부                                 |
| email\_verified\_tz / last\_login\_tz / failed\_login\_count / locked\_until\_tz |               |    |                 | 보안/상태                                 |

**유일성 규칙**

- 같은 채널에서 **이메일 중복 금지(소프트삭제 제외)**  → `(channel_id, email) UNIQUE WHERE is_deleted = FALSE`

### 4.2 social\_auth

| 필드                 | 타입            | 필수 | 설명                        |
| ------------------ | ------------- | -- | ------------------------- |
| id                 | BIGINT        | ✔︎ | PK                        |
| provider           | VARCHAR(50)   | ✔︎ | GOOGLE/KAKAO/NAVER/APPLE… |
| provider\_user\_id | VARCHAR(200)  | ✔︎ | 공급자 내 사용자 고유 ID           |
| provider\_email    | VARCHAR(500)  |    | 필요 시 암호화 저장               |
| provider\_token    | VARCHAR(2000) |    | 토큰/자격(민감, 암호화 권장)         |

**유일성 규칙**

- `(provider, provider_user_id)`는 소프트삭제 제외 전역 유일.

## 5) 인증 구조 (JWT + Redis)

### 5.1 토큰 발급

- **로그인 성공 시** 서버는 Access **JWT**를 발급하고 응답한다.
- JWT는 다음 **클레임**을 포함한다(PII 금지):
  - `sub`: 사용자 id (BIGINT)
  - `ch`: 채널 id (여기서는 `1`)
  - `jti`: 랜덤 고유 ID(로그아웃/블랙리스트 관리용)
  - `iat`, `exp`: 발급/만료 시각(예: 15\~60분)
  - (선택) `scp`/`roles`: 권한 범위
- **보안 권장사항**: FE에서는 **HttpOnly, Secure, SameSite** 쿠키로 전달/보관을 권장. (대안: 메모리/스토리지, CSRF 대비 필요)

### 5.2 Redis 관리 방식 (세션성 제어)

- 로그인 시 **Redis에 허용 리스트(allow-list)** 키를 기록한다.
  - 키: `auth:jwt:<jti>`
  - 값: `user_id` 또는 직렬화된 최소 정보
  - TTL: JWT 만료(`exp`)와 동일
- **요청 처리 시** 서버 미들웨어는
  1. JWT 서명·만료 검증 →
  2. `auth:jwt:<jti>` 키 존재 여부 확인(없으면 **로그아웃/철회된 토큰**으로 간주)
- **로그아웃 시**: Redis에서 `DEL auth:jwt:<jti>` 수행 → 토큰 즉시 무효화.

> 참고: 블랙리스트(deny-list)도 가능하지만, 허용 리스트가 **로그아웃·강제 만료**를 간단히 처리하고 메모리 사용량도 예측 가능.

### 5.3 토큰 재발급(선택)

- 간단 모드에선 **미지원**. 필요 시 `/auth/refresh`와 \*\*Refresh 토큰(쿠키 보관)\*\*을 별도 문서로 정의.

## 6) 플로우 (Flow)

### 6.1 회원가입(LOCAL)

1. 입력: `email`, `password`, `family_name`, `given_name` (선택: `gender`, `nickname`, `phone_*`, `nationality_code`)
2. 정규화/검증: 이메일 정규화 후 **(channel\_id=1, email)** 중복 검사; 비밀번호 해시 생성; PII 암호화
3. 저장: `channel_id=1`, `is_active=TRUE`, 감사 필드
4. 응답: 가입 성공 (자동 로그인은 정책에 따라)

### 6.2 로그인(LOCAL)

1. 입력: `email`, `password`
2. 조회: `(channel_id=1, email)` → 비밀번호 해시 검증
3. 발급: Access JWT 생성(`jti` 포함) → **Redis **`auth:jwt:<jti>`** SET EX TTL**
4. 응답: JWT 반환(쿠키 또는 JSON)

### 6.3 SNS 로그인/가입

1. `/auth/{provider}` 리다이렉트 → 콜백에서 `provider_user_id` 확인
2. 기존 `social_auth` 없으면 생성, 사용자 없으면 가입(이메일 수집 필요 시 UI로 확보)
3. **(channel\_id=1, email)** 충돌 검사 후 사용자 확정
4. JWT 발급 + Redis 등록(동일하게 `auth:jwt:<jti>`)

### 6.4 로그아웃

1. 클라이언트는 **Authorization 헤더**(또는 쿠키)를 전달한 상태로 `/auth/logout` 호출
2. 서버는 JWT에서 `jti` 추출 후 **Redis **`DEL auth:jwt:<jti>`** 수행
3. 쿠키를 사용 중이면 **쿠키 무효화(Set-Cookie 만료)** 반환

## 7) API 계약 (샘플)

### 7.1 POST `/api/auth/register`

**Body**

```json
{
  "email": "test@test.io",
  "password": "P@ssw0rd!",
  "family_name": "KIM",
  "given_name": "YUNA",
  "gender": "F",
  "nickname": "yuna_k",
  "phone_country_code": "+82",
  "phone_number": "01012345678",
  "nationality_code": "KR"
}
```

**201**

```json
{ "id": 1001, "channel_id": 1 }
```

### 7.2 POST `/api/auth/login`

**Body**

```json
{ "email": "test@test.io", "password": "P@ssw0rd!" }
```

**200**

```json
{
  "access_token": "<jwt>",
  "token_type": "Bearer",
  "expires_in": 3600,
  "jti": "c5f0..."
}
```

### 7.3 POST `/api/auth/logout`

- **Auth**: `Authorization: Bearer <jwt>` 또는 HttpOnly 쿠키
  **200**

```json
{ "ok": true }
```

> 서버는 JWT 검증 후 Redis `DEL auth:jwt:<jti>` 실행. 쿠키 방식이면 쿠키 만료도 함께 반환.

### 7.4 GET `/api/me`

**200**

```json
{
  "id": 1001,
  "channel_id": 1,
  "email": "test@test.io",
  "family_name": "KIM",
  "given_name": "YUNA",
  "gender": "F",
  "nickname": "yuna_k",
  "phone_country_code": "+82",
  "phone_number": "10-1234-5678",
  "nationality_code": "KR",
  "point": "0.000000",
  "is_active": true
}
```

> 응답의 PII는 **API 서비스 로직에서 복호화 후** 전달.

## 8) 검증/규칙 (Validation & Rules)

- **채널 고정**: 가입 시 `channel_id = 1`
- **이메일 중복 금지**: `(channel_id, email)` (소프트삭제 제외)
- **암호화 전략**: 이메일 등 PII는 **API 서비스 로직 내 암호화 저장**; 유니크 인덱스와의 호환을 위해 **결정적 암호화** 권장(아니면 서비스 로직에서 중복 검사 필수)
- **비밀번호**: Argon2id/bcrypt로 해시
- **JWT 서명키/알고리즘**: HS256/RS256 중 택1(운영 표준에 맞춤)
- **Redis 키 정책**: `auth:jwt:<jti>` 허용 리스트, TTL은 `exp`까지; 로그아웃 시 `DEL`
- **미들웨어**: 서명/만료 검증 + Redis 존재성 확인(없으면 401/419)

## 9) 오류 코드 (Error Codes)

- `USR001` 이메일 중복 (채널=1)
- `USR002` 비밀번호 불일치
- `USR003` 계정 잠금/비활성
- `USR004` SNS 식별자 중복(social\_auth)
- `USR005` 입력 형식 오류(길이/형식)
- `AUTH001` 토큰 누락 또는 형식 오류
- `AUTH002` 토큰 만료
- `AUTH003` 서명 불일치
- `AUTH004` 토큰 철회(로그아웃됨 — Redis 미존재)

## 10) 보안/비기능 (Non-Functional)

- **로그**에 PII/비번/토큰 출력 금지
- PII는 서버 메모리 상에서도 최소 보관, 응답 시 필요한 정보만 복호화
- Redis 장애 시의 **폴백 정책**(권고: 장애 시 신규 로그인 제한 또는 임시 블랙리스트 모드)
- 트랜잭션: 가입/소셜링크 생성은 단일 트랜잭션

## 11) 수락 기준 (Acceptance Criteria)

-

## 12) 오픈 이슈 (Open Questions)

- 이메일 **결정적 암호화** 채택 여부(유니크 인덱스 유지 목적)
- Access 토큰 만료(예: 15분/60분)와 쿠키 보관 여부(쿠키 vs 메모리)
- Refresh 토큰 도입 여부와 로테이션 정책(별도 문서로 승격 예정)
- Redis 장애 모드(허용 리스트 무시/차단 중 택1)

