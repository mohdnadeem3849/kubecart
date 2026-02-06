Write-Host "====================================="
Write-Host " KubeCart - Build Docker Images"
Write-Host "====================================="

# Configure Docker to use Minikube daemon
Write-Host "Configuring Docker to use Minikube..."
& minikube -p minikube docker-env --shell powershell | Invoke-Expression

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to configure Docker for Minikube" -ForegroundColor Red
    exit 1
}

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host ""
Write-Host "Building Identity API image..."
docker build -t kubecart-identity:local ./services/identity/src/Identity.Api

Write-Host ""
Write-Host "Building Catalog API image..."
docker build -t kubecart-catalog:local ./services/catalog/src/Catalog.Api

Write-Host ""
Write-Host "Building Orders API image..."
docker build -t kubecart-orders:local ./services/orders/src/Orders.Api

Write-Host ""
Write-Host "Building UI image..."
docker build -t kubecart-ui:local ./ui

Write-Host ""
Write-Host "Docker images built:"
docker images | Select-String "kubecart-"

Write-Host ""
Write-Host "✅ Docker images built successfully"
