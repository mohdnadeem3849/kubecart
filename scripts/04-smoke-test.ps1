Write-Host "====================================="
Write-Host " KubeCart - Smoke Test"
Write-Host "====================================="

$ip = minikube ip
Write-Host ""
Write-Host "Minikube IP: $ip"
Write-Host "Make sure your hosts file contains:"
Write-Host "  $ip  kubecart.local"
Write-Host ""

Write-Host "Checking Ingress..."
kubectl -n demo get ingress

Write-Host ""
Write-Host "Checking service health endpoints..."

Write-Host ""
Write-Host "Identity health:"
curl.exe -s http://kubecart.local/api/auth/health/live
Write-Host ""

Write-Host ""
Write-Host "Catalog health:"
curl.exe -s http://kubecart.local/api/catalog/health/live
Write-Host ""

Write-Host ""
Write-Host "Orders health:"
curl.exe -s http://kubecart.local/api/orders/health/live
Write-Host ""

Write-Host ""
Write-Host "Catalog products (public endpoint):"
curl.exe -s http://kubecart.local/api/catalog/products
Write-Host ""

Write-Host ""
Write-Host "✅ Smoke test completed"
Write-Host "Open UI in browser: http://kubecart.local"
