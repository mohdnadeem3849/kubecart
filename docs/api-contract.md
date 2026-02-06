\# KubeCart – API Contract



This document describes the public API endpoints exposed by the KubeCart

microservices and how the frontend UI communicates with them.



This file is documentation only.  

It does not affect application behavior.



---



\## Routing Overview (Local Development)



All API calls are made using relative paths from the UI.



| Path Prefix | Service | Local URL |

|------------|--------|-----------|

| `/api/auth` | Identity.Api | https://localhost:7046 |

| `/api/catalog` | Catalog.Api | https://localhost:7221 |

| `/api/orders` | Orders.Api | https://localhost:7034 |



The Vite development server proxies these paths to the correct backend service.



---



\## Identity Service (`/api/auth`)



\### POST `/api/auth/login`



Authenticates a user and returns a JWT token.



\*\*Request\*\*

```json

{

&nbsp; "email": "user@kubecart.com",

&nbsp; "password": "Password123!"

}

Response



json

Copy code

{

&nbsp; "token": "JWT\_TOKEN"

}

JWT Token Contents

The JWT token contains the following claims:



sub – User ID (GUID)



role – User role (Admin or Customer)



email – User email



iss – Token issuer (kubecart-identity)



aud – Token audience (kubecart-api)



This token is required to access protected endpoints.



GET /api/auth/me

Returns details of the currently logged-in user.



Headers



makefile

Copy code

Authorization: Bearer <JWT\_TOKEN>

Catalog Service (/api/catalog)

GET /api/catalog/products

Returns a list of available products.



Used by the Products listing page.



GET /api/catalog/products/{id}

Returns details for a single product.



Used by the Product Details page.



Admin-only Operations

Catalog service exposes admin-only endpoints for:



Creating products



Updating products



Managing product images



These endpoints require a JWT token with role=Admin.



Orders Service (/api/orders)

GET /api/orders

Returns all orders belonging to the logged-in user.



Users can only access their own orders.



GET /api/orders/{orderId}

Returns order details including order items.



Order items contain snapshot data:



Product name at time of purchase



Unit price at time of purchase



Image URL at time of purchase



This ensures historical accuracy even if catalog data changes later.



POST /api/orders/checkout

Creates an order from the user's cart.



Request



json

Copy code

{

&nbsp; "notes": "Deliver after 6 PM"

}

Behavior



Validates products by calling Catalog service



Calculates total amount



Stores order and order items transactionally



Clears the cart after successful checkout



Admin Endpoint

PUT /api/orders/admin/{orderId}/status



Allows an admin user to update the order status.



Security Rules

All Orders endpoints require authentication



Admin endpoints require role=Admin



Each service validates JWT tokens independently



Health Endpoints

All services expose the following endpoints:



/health/live – Confirms service is running



/health/ready – Confirms service is ready and database is reachable

