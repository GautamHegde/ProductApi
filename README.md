# ProductApi

This is a RESTful API built with ASP.NET Core and Entity Framework Core using the Code First approach.  
It provides CRUD operations and stock management features for Products.

Features

-  Add, update, delete, and fetch products
-  Auto-generate unique 6-digit Product IDs
-  SQLite database
-  RESTful endpoints using ASP.NET Core Web API
-  Endpoints to manage stock (add/decrement)
-  EF Core migrations enabled
-  Unit tests 
	
##  Technologies

- ASP.NET Core 7.0+
- Entity Framework Core
- SQLite
- C#
- Visual Studio Professional 2022
- Git
- Xunit


| Method | Endpoint												| Description		    |
|--------|------------------------------------------------------------------------------|
| POST   | `/api/products`										| Create a new product	|
| GET    | `/api/products`										| Get all products	    |
| GET    | `/api/products/{id}`									| Get product by ID     |
| PUT    | `/api/products/{id}`									| Update product        |
| DELETE | `/api/products/{id}`									| Delete product        |
| PUT    | `/api/products/decrement-stock/{id}/{quantity}`		| Decrease stock        |
| PUT    | `/api/products/add-to-stock/{id}/{quantity}`			| Increase stock        |



## Examples

### Create a New Product
**Request:**
{ "name": "New Product", "description": "This is a sample product.", "price": 25.99, "stockAvailable": 100 }

**Response:**
{ "id": 123456, "name": "New Product", "description": "This is a sample product.", "price": 25.99, "stockAvailable": 100 }

### Get All Products
**Response:**
{ "id": 123456, "name": "New Product", "description": "This is a sample product.", "price": 25.99, "stockAvailable": 100 }


### Get Product by ID
**Request:**  
`GET /api/products/123456`
**Response:**
{ "id": 123456, "name": "New Product", "description": "This is a sample product.", "price": 25.99, "stockAvailable": 100 }


