# Runs DB init scripts for KubeCart
# Requires: sqlcmd available (comes with SSMS tools) OR SQL Server command line tools installed

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$identity = Join-Path $PSScriptRoot "identity-init.sql"
$catalog  = Join-Path $PSScriptRoot "catalog-init.sql"
$orders   = Join-Path $PSScriptRoot "orders-init.sql"

# Change if needed
$server = $env:SQL_SERVER
if ([string]::IsNullOrWhiteSpace($server)) { $server = "localhost" }

# SQL Auth (recommended for K8S-style dev)
$user = $env:SQL_USER
$pass = $env:SQL_PASSWORD

Write-Host "SQL Server: $server"

function Run-SqlFile($filePath) {
  Write-Host "Running $filePath ..."
  if (-not (Test-Path $filePath)) { throw "File not found: $filePath" }

  if (-not [string]::IsNullOrWhiteSpace($user)) {
    if ([string]::IsNullOrWhiteSpace($pass)) { throw "SQL_PASSWORD env var is missing." }
    sqlcmd -S $server -U $user -P $pass -i $filePath
  } else {
    # Windows Auth fallback (local dev convenience)
    sqlcmd -S $server -E -i $filePath
  }
}

Run-SqlFile $identity
Run-SqlFile $catalog
Run-SqlFile $orders

Write-Host "✅ All databases initialized."
