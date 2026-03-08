# Api-Pipeline-Concepts

A minimal **ASP.NET Core 10** Web API demonstrating three essential pipeline concepts: **Global Exception Handling**, **FluentValidation**, and **API Versioning**. Business logic is intentionally simple (in-memory list) so the focus remains entirely on the pipeline setup.

---

## Tech Stack

| | |
|---|---|
| **Framework** | ASP.NET Core 10 (.NET 10) |
| **Exception Handling** | `IExceptionHandler` (built-in, .NET 8+) |
| **Validation** | `FluentValidation.AspNetCore` v11 |
| **Versioning** | `Asp.Versioning.Mvc` v8 |

---

## Project Structure

```
Api-Pipeline-Concepts/
├── Controllers/
│   ├── V1/
│   │   └── ProductsController.cs   # v1 endpoints
│   └── V2/
│       └── ProductsController.cs   # v2 endpoints
├── DTOs/
│   └── CreateProductDto.cs         # Request DTO
├── Exceptions/
│   ├── GlobalExceptionHandler.cs   # IExceptionHandler implementation
│   └── NotFoundException.cs        # Custom 404 exception
├── Validators/
│   └── CreateProductDtoValidator.cs # FluentValidation rules
└── Program.cs                      # Full pipeline wired here
```

---

## Concept 1 — Global Exception Handling

**Interface:** `IExceptionHandler` (introduced in .NET 8)

`GlobalExceptionHandler` implements `IExceptionHandler.TryHandleAsync`. The middleware `UseExceptionHandler()` intercepts any unhandled exception and delegates to it. A `switch` expression maps exception types to standardized `ProblemDetails` responses (RFC 7807):

| Exception | Status | Title |
|---|---|---|
| `NotFoundException` | `404 Not Found` | Resource Not Found |
| Any other | `500 Internal Server Error` | An unexpected error occurred |

### Registration (`Program.cs`)

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// ...
app.UseExceptionHandler();
```

### Example Response (404)

```json
{
  "status": 404,
  "title": "Resource Not Found",
  "detail": "Product with key '999' was not found."
}
```

---

## Concept 2 — FluentValidation

**Package:** `FluentValidation.AspNetCore`

`CreateProductDtoValidator` extends `AbstractValidator<CreateProductDto>` and defines field-level rules:

| Field | Rules |
|---|---|
| `Name` | Not empty · Max 100 characters |
| `Price` | Greater than 0 |
| `Category` | Not empty |

`AddFluentValidationAutoValidation()` hooks into the ASP.NET Core model binding pipeline. The validator runs **before the controller action is invoked**. On failure the framework short-circuits with `400 Bad Request` and an `errors` dictionary — the controller is never reached.

### Registration (`Program.cs`)

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

### Example Failure Response (400)

```json
{
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than '0'."],
    "Category": ["'Category' must not be empty."]
  },
  "status": 400,
  "title": "One or more validation errors occurred."
}
```

---

## Concept 3 — API Versioning

**Package:** `Asp.Versioning.Mvc`

Version is read from the **URL path segment** using the route token `v{version:apiVersion}`. Each controller class is decorated with `[ApiVersion]` to declare which version it handles.

### Registration (`Program.cs`)

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();
```

`ReportApiVersions = true` adds `api-supported-versions` to every response header so clients can discover available versions.

### Version Comparison

| | `GET /api/v1/products` | `GET /api/v2/products` |
|---|---|---|
| **Response shape** | Plain array of names | Rich object with count + full details |
| **Example** | `["Laptop", "Notebook"]` | `{ "apiVersion": "2.0", "count": 2, "products": [...] }` |

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/products` | List product names (v1) |
| `GET` | `/api/v2/products` | List full products with metadata (v2) |
| `GET` | `/api/v1/products/{id}` | Get product by ID (throws `NotFoundException` if missing) |
| `POST` | `/api/v1/products` | Create product (validated by FluentValidation) |
| `GET` | `/api/v1/products/error` | Triggers a deliberate 500 for demo purposes |

---

## Getting Started

```bash
# Clone the repo and navigate to the project
cd Api-Pipeline-Concepts

# Restore packages and run
dotnet run --project Api-Pipeline-Concepts
```

The API will be available at `https://localhost:7001` (or the port shown in the console).

### Sample Requests

```http
# v1 — plain list
GET https://localhost:7001/api/v1/products

# v2 — rich list
GET https://localhost:7001/api/v2/products

# 404 — NotFoundException
GET https://localhost:7001/api/v1/products/999

# 500 — unhandled exception
GET https://localhost:7001/api/v1/products/error

# 201 — valid POST
POST https://localhost:7001/api/v1/products
Content-Type: application/json

{ "name": "Mechanical Keyboard", "price": 89.99, "category": "Electronics" }

# 400 — FluentValidation failure
POST https://localhost:7001/api/v1/products
Content-Type: application/json

{ "name": "", "price": -5, "category": "" }
```
