Write-Host "====================================="
Write-Host " KubeCart - Start Minikube"
Write-Host "====================================="

Write-Host "Starting Minikube..."
minikube start

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Minikube failed to start" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Enabling Ingress addon..."
minikube addons enable ingress

Write-Host ""
Write-Host "Minikube status:"
minikube status

Write-Host ""
Write-Host "Minikube IP:"
minikube ip

Write-Host ""
Write-Host "ℹ️  Add this entry to your hosts file:"
Write-Host "   <MINIKUBE_IP>  kubecart.local"
Write-Host ""
Write-Host "✅ Minikube is ready"
