Write-Host "====================================="
Write-Host " KubeCart - Cleanup"
Write-Host "====================================="

Write-Host ""
Write-Host "Deleting namespace demo (this removes all KubeCart resources)..."
kubectl delete namespace demo --ignore-not-found=true

Write-Host ""
Write-Host "Do you want to stop Minikube? (Y/N)"
$ans = Read-Host

if ($ans -eq "Y" -or $ans -eq "y") {
    Write-Host "Stopping Minikube..."
    minikube stop
    Write-Host "✅ Minikube stopped"
}
else {
    Write-Host "Skipping Minikube stop"
}

Write-Host ""
Write-Host "✅ Cleanup complete"
