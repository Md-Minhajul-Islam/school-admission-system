#!/bin/sh
set -eu

# The built frontend is static, so the backend URL is injected into a tiny runtime
# config file each time the container starts.
API_BASE_URL="${API_BASE_URL:-http://localhost:8080}"
ESCAPED_URL=$(printf '%s' "$API_BASE_URL" | sed 's/\\/\\\\/g; s/"/\\"/g')

cat > /usr/share/nginx/html/config.js <<EOF
window.__APP_CONFIG__ = { API_BASE_URL: "${ESCAPED_URL}" };
EOF
