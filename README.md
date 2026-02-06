\# KubeCart — Capstone Microservices Application (Minikube)



KubeCart is a \*\*production-style microservice e-commerce application\*\* built as a capstone project using:



\- .NET 8 Web APIs (Identity, Catalog, Orders)

\- React + Vite UI

\- SQL Server (external to Kubernetes)

\- Docker + Kubernetes (Minikube)

\- Ingress-based routing under one domain



This project follows \*\*real-world microservice boundaries\*\*:

\- Each service owns its database

\- No shared databases between services

\- Services communicate via HTTP

\- Configuration via environment variables



---



\## Architecture Overview



Services:

\- \*\*Identity Service\*\* → Authentication \& JWT

\- \*\*Catalog Service\*\* → Products \& Categories

\- \*\*Orders Service\*\* → Cart, Checkout, Orders, Admin Status

\- \*\*UI\*\* → React app served via Nginx



Databases (outside Kubernetes):

\- `KubeCart\_Identity`

\- `KubeCart\_Catalog`

\- `KubeCart\_Orders`



Ingress routing:

\- `/` → UI

\- `/api/auth` → Identity API

\- `/api/catalog` → Catalog API

\- `/api/orders` → Orders API



---



\## Repository Structure



kubecart/

docs/

scripts/

sql/

k8s/

services/

identity/

catalog/

orders/

ui/



yaml

Copy code



---



\## Prerequisites



Install on your machine:

\- Docker Desktop

\- kubectl

\- Minikube

\- SQL Server (local) + SSMS

\- sqlcmd (comes with SQL Server tools)



---



\## Step 1 — Initialize Databases (External SQL Server)



This project uses SQL Server \*\*outside Kubernetes\*\*.



Run PowerShell in repo root:



```powershell

cd C:\\Projects\\kubecart\\KubeCart

Option A — Windows Authentication (local dev)

powershell

Copy code

.\\scripts\\sql\\00-create-dbs.ps1

Option B — SQL Authentication (recommended for Kubernetes)

powershell

Copy code

$env:SQL\_SERVER="localhost"

$env:SQL\_USER="sa"

$env:SQL\_PASSWORD="YOUR\_PASSWORD"

.\\scripts\\sql\\00-create-dbs.ps1

This creates:



Databases



Tables



Minimal seed data for Catalog



Step 2 — Prepare Kubernetes Secrets

Example secret files are provided:



bash

Copy code

k8s/secrets/

&nbsp; identity-secrets.example.yaml

&nbsp; catalog-secrets.example.yaml

&nbsp; orders-secrets.example.yaml

Create real secrets locally (DO NOT COMMIT)

Copy each file:



identity-secrets.example.yaml → identity-secrets.yaml



catalog-secrets.example.yaml → catalog-secrets.yaml



orders-secrets.example.yaml → orders-secrets.yaml



Edit the real files and replace:



DB\_PASSWORD



JWT\_SIGNING\_KEY



APP\_ENCRYPTION\_KEY (Identity only)



Real secrets are ignored by git via .gitignore.



Step 3 — Start Minikube

powershell

Copy code

.\\scripts\\00-check-prereqs.ps1

.\\scripts\\01-start-minikube.ps1

Note the Minikube IP shown.



Update hosts file (required)

Edit as Administrator:



makefile

Copy code

C:\\Windows\\System32\\drivers\\etc\\hosts

Add:



lua

Copy code

<MINIKUBE\_IP> kubecart.local

Step 4 — Build Docker Images (inside Minikube)

powershell

Copy code

.\\scripts\\02-build-images.ps1

Images built:



kubecart-identity:local



kubecart-catalog:local



kubecart-orders:local



kubecart-ui:local



Step 5 — Deploy to Kubernetes

powershell

Copy code

.\\scripts\\03-deploy-k8s.ps1

Watch pods:



powershell

Copy code

kubectl -n demo get pods -w

Step 6 — Smoke Test

powershell

Copy code

.\\scripts\\04-smoke-test.ps1

Open UI in browser:



arduino

Copy code

http://kubecart.local

Health Endpoints

Each service exposes:



/health/live



/health/ready



Used by Kubernetes liveness/readiness probes.



Cleanup

To remove all resources:



powershell

Copy code

.\\scripts\\99-cleanup.ps1

Notes

Kubernetes pods access local SQL Server via:



csharp

Copy code

host.minikube.internal,1433

SQL Server must allow TCP connections



SQL Authentication is required for Kubernetes



UI calls APIs using relative paths (/api/...)



Documentation

See /docs folder:



architecture.md



api-contract.md



troubleshooting.md



yaml

Copy code



