$databases = @(
  "auth_service",
  "chat_service",
  "pricing_service",
  "policy_service",
  "policy_search_service",
  "payment_service",
  "product_service",
  "document_service",
  "agent_portal_api_gateway"
)

$backupDir = Join-Path $PSScriptRoot "..\backups"
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

foreach ($database in $databases) {
  pg_dump -U postgres -F c -f (Join-Path $backupDir "$database.backup") $database
}
