send_slack.sh 사용법

환경 변수 설정:

```bash
export SLACK_WEBHOOK_URL="https://hooks.slack.com/services/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"
```

작업 시작 메시지 전송 예:

```bash
./scripts/send_slack.sh "Work Start" "작업을 시작했습니다. 아래 내용을 확인하세요." "#3B82F6" "토큰/레디스 설정 작업 시작"
```

작업 종료 메시지 전송 예:

```bash
./scripts/send_slack.sh "Work End" "작업이 종료되었습니다. 아래 내용을 확인하세요." "#22C55E" "토큰/레디스 설정 반영 및 빌드 통과"
```

참고: 에이전트(이 자동화된 도구)는 현재 외부 네트워크로 직접 POST 요청을 보낼 수 없습니다. 따라서 제가 직접 슬랙으로 메시지를 전송할 수는 없고, 대신 이 저장소에 포함된 스크립트를 사용해 로컬 또는 CI 환경에서 Start/End 알림을 보낼 수 있도록 했습니다.

- 로컬에서 에이전트 작업 전/후 자동 알림 실행 예:

```bash
export SLACK_WEBHOOK_URL="<your webhook url>"
./scripts/auto_notify_for_agent.sh dotnet build TigerBooking.sln -c Debug
```

위 명령은 Work Start 메시지를 보내고 지정한 명령을 실행한 뒤 Work End 메시지를 전송합니다.

주의: 웹훅 URL은 비밀 정보이므로 외부에 노출하지 마세요.
