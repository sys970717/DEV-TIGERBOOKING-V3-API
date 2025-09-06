#!/usr/bin/env bash
# send_slack.sh
# Default webhook URL is read from SLACK_WEBHOOK_URL environment variable.
# Usage:
#   SLACK_WEBHOOK_URL="https://hooks.slack.com/services/XXX/YYY/ZZZ" ./scripts/send_slack.sh "Work Start" "작업을 시작했습니다. 아래 내용을 확인하세요." "#3B82F6" "간단 요약"

set -euo pipefail
WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
if [ -z "$WEBHOOK_URL" ]; then
  echo "ERROR: SLACK_WEBHOOK_URL environment variable is not set. Exiting." >&2
  exit 2
fi

TITLE="$1"
HEADER="$2"
COLOR="$3"
CHANGE_SUMMARY="$4"

payload=$(cat <<-JSON
{
  "username": "TB-v3-api-copilot",
  "icon_emoji": ":tiger:",
  "channel": "C09DV5N5Z6E",
  "attachments": [
    {
      "color": "${COLOR}",
      "blocks": [
        { "type": "section", "text": { "type": "mrkdwn", "text": "*${TITLE}*\n${HEADER}" } },
        { "type": "divider" },
        { "type": "section", "fields": [
            { "type": "mrkdwn", "text": "*프로젝트*\nDEV-TIGERBOOKING-V3-API" },
            { "type": "mrkdwn", "text": "*작업자*\nTB-v3-api-copilot" },
            { "type": "mrkdwn", "text": "*변경사항*\n${CHANGE_SUMMARY}" }
        ] }
      ]
    }
  ]
}
JSON
)

curl -s -X POST -H 'Content-type: application/json' --data "$payload" "$WEBHOOK_URL" >/dev/null

# exit 0
