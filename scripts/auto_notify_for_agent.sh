#!/usr/bin/env bash
# auto_notify_for_agent.sh
# Usage:
#   Set SLACK_WEBHOOK_URL, then run this script with a command to execute.
#   The script will send Work Start, run the command, then send Work End (with success/failure summary).

set -euo pipefail
WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
if [ -z "$WEBHOOK_URL" ]; then
  echo "ERROR: SLACK_WEBHOOK_URL environment variable is not set. Exiting." >&2
  exit 2
fi

if [ $# -lt 1 ]; then
  echo "Usage: $0 <command...>" >&2
  exit 2
fi

TITLE_START="Work Start"
HEADER_START="작업을 시작했습니다. 아래 내용을 확인하세요."
COLOR_START="#3B82F6"
SUMMARY_START="$*"

# send start
./scripts/send_slack.sh "$TITLE_START" "$HEADER_START" "$COLOR_START" "$SUMMARY_START"

# run command
set +e
"$@"
RC=$?
set -e

if [ $RC -eq 0 ]; then
  TITLE_END="Work End"
  HEADER_END="작업이 종료되었습니다. 아래 내용을 확인하세요."
  COLOR_END="#22C55E"
  SUMMARY_END="명령이 성공적으로 완료되었습니다: $*"
else
  TITLE_END="Work End"
  HEADER_END="작업이 종료되었으나 오류가 발생했습니다. 아래 내용을 확인하세요."
  COLOR_END="#DC2626"
  SUMMARY_END="명령이 실패했습니다 (exit code $RC): $*"
fi

./scripts/send_slack.sh "$TITLE_END" "$HEADER_END" "$COLOR_END" "$SUMMARY_END"

exit $RC
