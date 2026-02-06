\# KubeCart – System Architecture



This document explains the overall architecture of the KubeCart application.

It is written for understanding, grading, and viva discussion.



---



\## Architectural Style



KubeCart follows a \*\*microservice-based architecture\*\*.



Each major business capability is implemented as an independent service with:

\- Its own codebase

\- Its own database

\- Its own responsibility



Services communicate with each other using \*\*HTTP APIs\*\*, not shared databases.



---



\## High-Level Components



The system consists of the following components:



1\. Frontend UI (React + Vite)

2\. Identity Service (Authentication)

3\. Catalog Service (Products)

4\. Orders Service (Cart \& Orders)

5\. External SQL Server (multiple databases)



---



\## Frontend UI



The frontend is a \*\*React application\*\* built using Vite.



\### Responsibilities

\- User login and registration

\- Product browsing

\- Cart management (client-side)

\- Checkout

\- Viewing order history



\### API Communication

The UI always calls \*\*relative paths\*\*:

\- `/api/auth`

\- `/api/catalog`

\- `/api/orders`



In local development, Vite proxies these paths to backend services.

In production, Kubernetes Ingress routes these paths.



---



\## Identity Service



\### Responsibility

\- User authentication

\- JWT token issuance

\- User identity verification



\### Database

\- `KubeCart\_Identity`



The Identity service is the \*\*only service\*\* allowed to manage user credentials.



\### JWT Tokens

After successful login, Identity service issues a JWT containing:

\- User ID (`sub`)

\- Role (`Admin` or `Customer`)

\- Email

\- Issuer and audience claims



Other services validate the JWT but do not generate it.



---



\## Catalog Service



\### Responsibility

\- Product management

\- Product listing and details

\- Product images



\### Database

\- `KubeCart\_Catalog`



\### Admin Control

Only users with `Admin` role can:

\- Add products

\- Update products

\- Manage product images



Catalog service does not store user or order data.



---



\## Orders Service



\### Responsibility

\- Cart persistence during checkout

\- Order creation

\- Order history

\- Order status management



\### Database

\- `KubeCart\_Orders`



\### Checkout Flow

1\. User initiates checkout

2\. Orders service fetches cart items

3\. Orders service calls Catalog service via HTTP to validate products

4\. Order and order items are created in a database transaction

5\. Snapshot data is stored (product name, price, image)

6\. Cart is cleared



\### Snapshot Strategy

Orders service stores snapshot values to preserve historical accuracy even if

catalog data changes later.



---



\## Service-to-Service Communication



\- Services communicate using \*\*HTTP APIs\*\*

\- No service directly accesses another service’s database

\- Example:

&nbsp; - Orders service calls Catalog service to validate products



This enforces proper microservice boundaries.



---



\## Database Strategy



Each service owns its own database:



\- Identity → `KubeCart\_Identity`

\- Catalog → `KubeCart\_Catalog`

\- Orders → `KubeCart\_Orders`



All databases run on the same SQL Server instance but are logically isolated.



This prevents tight coupling between services.



---



\## Security Model



\- JWT-based authentication

\- Role-based authorization

\- Each service validates JWT independently

\- Users can only access their own data



Security is enforced at the \*\*API level\*\*, not the UI.



---



\## Health Checks



Each service exposes:

\- `/health/live` – confirms service is running

\- `/health/ready` – confirms database connectivity



These endpoints are used by Kubernetes for service health monitoring.



---



\## Deployment Readiness



The application is designed to be deployed in Kubernetes:



\- Each service has its own Dockerfile

\- Configuration is provided via environment variables

\- Ingress routes traffic based on URL paths

\- Services can be scaled independently



---



\## Why This Architecture



This architecture was chosen to:

\- Match real-world production systems

\- Support independent scaling

\- Improve fault isolation

\- Enforce clear service boundaries



It demonstrates industry-standard microservice practices.



