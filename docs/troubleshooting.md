&nbsp;KubeCart – Troubleshooting Guide



This document lists common problems that can occur in a microservice-based

application and explains how to debug them.



This is documentation only and does not affect application behavior.



---



\## 1️⃣ Checkout fails when Catalog service is down



\### Symptom

\- Checkout API fails

\- Order is not created



\### Cause

Orders service depends on Catalog service to:

\- Validate product existence

\- Fetch product price and details



If Catalog service is unavailable, checkout cannot proceed.



\### How to Debug

1\. Check if Catalog API is running

2\. Call `/health/live` on Catalog service

3\. Verify `CATALOG\_SERVICE\_URL` configuration



\### Explanation (for viva)

Microservices communicate through HTTP.

If a dependent service is down, the workflow must fail gracefully.



---



\## 2️⃣ 401 Unauthorized error from API



\### Symptom

\- API returns `401 Unauthorized`

\- Swagger or UI cannot access protected endpoints



\### Possible Causes

\- Missing JWT token

\- Expired JWT token

\- Invalid issuer or audience

\- Incorrect Authorization header format



\### How to Debug

1\. Ensure header format is:

Authorization: Bearer <JWT\_TOKEN>



yaml

Copy code

2\. Decode JWT using jwt.io

3\. Verify claims:

&nbsp;  - `iss = kubecart-identity`

&nbsp;  - `aud = kubecart-api`

&nbsp;  - `role` exists



---



\## 3️⃣ User sees another user’s data



\### Expected Behavior

This should NEVER happen.



\### Prevention

\- Orders API filters data using `UserId` from JWT (`sub` claim)

\- Each query includes `WHERE UserId = @UserId`



\### Explanation (for viva)

Authorization is enforced at API level, not UI level.



---



\## 4️⃣ UI works locally but fails after deployment



\### Cause

\- Local dev uses Vite proxy

\- Production uses Ingress routing



\### Correct Approach

\- UI should always call relative paths:

&nbsp; - `/api/auth`

&nbsp; - `/api/catalog`

&nbsp; - `/api/orders`



Hardcoding `localhost` in UI breaks production deployments.



---



\## 5️⃣ Readiness probe fails in Kubernetes



\### Symptom

\- Pod starts but is removed from service routing



\### Cause

\- Database unavailable

\- Wrong DB credentials

\- Network connectivity issue



\### How to Debug

1\. Call `/health/ready`

2\. Check DB connection variables:

&nbsp;  - DB\_HOST

&nbsp;  - DB\_NAME

&nbsp;  - DB\_USER

&nbsp;  - DB\_PASSWORD



\### Explanation

A service that cannot connect to its database is not considered ready.



---



\## 6️⃣ Orders show old prices after product update



\### Explanation

This is expected behavior.



Orders service stores snapshot data:

\- Product name

\- Unit price

\- Image URL



This ensures historical accuracy even if catalog data changes later.



---



\## 7️⃣ Docker container runs on different port than local dev



\### Explanation

\- Local development uses Visual Studio ports (e.g. 7046, 7221, 7034)

\- Docker containers run on port 8080 internally



Ports are mapped by Kubernetes services and ingress.



This separation is intentional and correct.



---



\## Key Microservices Principle



Each service:

\- Owns its own database

\- Validates JWT independently

\- Communicates via HTTP

\- Can fail independently



This design mirrors real production systems.



