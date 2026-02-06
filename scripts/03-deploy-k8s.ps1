Write-Host "====================================="
Write-Host " KubeCart - Deploy to Kubernetes"
Write-Host "====================================="

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host ""
Write-Host "Applying namespace..."
kubectl apply -f .\k8s\00-namespace.yaml

Write-Host ""
Write-Host "Applying ConfigMaps..."
kubectl apply -f .\k8s\config\identity-configmap.yaml
kubectl apply -f .\k8s\config\catalog-configmap.yaml
kubectl apply -f .\k8s\config\orders-configmap.yaml
kubectl apply -f .\k8s\config\ui-configmap.yaml

Write-Host ""
Write-Host "Applying Secrets..."

# IMPORTANT:
# DevOps must copy *.example.yaml -> *.yaml before running this script:
#   k8s/secrets/identity-secrets.example.yaml -> k8s/secrets/identity-secrets.yaml
#   k8s/secrets/catalog-secrets.example.yaml  -> k8s/secrets/catalog-secrets.yaml
#   k8s/secrets/orders-secrets.example.yaml   -> k8s/secrets/orders-secrets.yaml

kubectl apply -f .\k8s\secrets\identity-secrets.yaml
kubectl apply -f .\k8s\secrets\catalog-secrets.yaml
kubectl apply -f .\k8s\secrets\orders-secrets.yaml

Write-Host ""
Write-Host "Applying Deployments..."
kubectl apply -f .\k8s\deployments\identity-deployment.yaml
kubectl apply -f .\k8s\deployments\catalog-deployment.yaml
kubectl apply -f .\k8s\deployments\orders-deployment.yaml
kubectl apply -f .\k8s\deployments\ui-deployment.yaml

Write-Host ""
Write-Host "Applying Services..."
kubectl apply -f .\k8s\services\identity-service.yaml
kubectl apply -f .\k8s\services\catalog-service.yaml
kubectl apply -f .\k8s\services\orders-service.yaml
kubectl apply -f .\k8s\services\ui-service.yaml

Write-Host ""
Write-Host "Applying Ingress..."
kubectl apply -f .\k8s\ingress\ingress.yaml

Write-Host ""
Write-Host "Current Pods (demo namespace):"
kubectl -n demo get pods

Write-Host ""
Write-Host "✅ Deploy finished"
Write-Host "TIP: watch pods with: kubectl -n demo get pods -w"
