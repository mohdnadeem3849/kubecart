Write-Host "====================================="
Write-Host " KubeCart - Prerequisite Check"
Write-Host "====================================="

$commands = @("docker", "kubectl", "minikube")

foreach ($cmd in $commands) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "❌ $cmd is NOT installed or not in PATH" -ForegroundColor Red
        exit 1
    }
    else {
        Write-Host "✅ $cmd found"
    }
}

Write-Host ""
Write-Host "Docker version:"
docker version --format '{{.Server.Version}}'

Write-Host ""
Write-Host "kubectl version:"
kubectl version --client --short

Write-Host ""
Write-Host "Minikube version:"
minikube version

Write-Host ""
Write-Host "✅ All prerequisites are installed"
