# Lab Log: Hardening the Products CRUD API

## Part 0: Baseline

Bad behaviors reproduced:
1. POST with an empty name and a negative price was accepted and stored (200 OK).
2. GET /api/products/9999 (missing id) caused an unhandled exception and a 500.

## Part 1: Data Annotations validation

Added CreateProductRequest and UpdateProductRequest with [Required],
[StringLength], and [Range]. [ApiController] triggers automatic validation.

400 response body for POST { "name": "", "price": -5 }:

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The Name field is required.",
      "The field Name must be a string with a minimum length of 2 and a maximum length of 100."
    ],
    "Price": [
      "The field Price must be between 0.01 and 1000000."
    ]
  },
  "traceId": "00-8f3a2c1d9e4b5a6f7c8d9e0a1b2c3d4e-5f6a7b8c9d0e1f2a-00"
}


## Part 2: Custom rule and domain exception

Custom rule: IValidatableObject on both request models requires
SalePrice < Price when SalePrice is provided.
Message returned (400): "SalePrice must be less than Price."

Added NotFoundException, thrown from GET/PUT/DELETE when the id does not
exist. Before Part 3, GET /api/products/9999 produced an unhandled
exception and a raw 500.

## Part 3: Global exception handling

Added GlobalExceptionHandler (IExceptionHandler) mapping
NotFoundException -> 404, ConflictException -> 409,
ValidationException -> 400, anything else -> 500. The full exception is
logged server-side; the client only receives a safe ProblemDetails.

404 body:

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "Product with id '9999' was not found.",
  "instance": "/api/products/9999",
  "traceId": "0HNEXAMPLE1234:00000001"
}


Matching server log line:

warn: HardenACrud.Exceptions.GlobalExceptionHandler[0]
      Handled NotFoundException (404) on GET /api/products/9999


## Part 4: Standardized responses

201 Created response (with Location header):

Status: 201 Created
Location: https://localhost:7123/api/products/4

{
  "id": 4,
  "name": "Headphones",
  "description": null,
  "price": 80,
  "salePrice": 60
}


404 body:

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "Product with id '9999' was not found.",
  "instance": "/api/products/9999",
  "traceId": "0HNEXAMPLE1234:00000002"
}

Paged list response (GET /api/products?page=1&pageSize=2):

{
  "data": [
    {
      "id": 1,
      "name": "Keyboard",
      "description": "Mechanical keyboard",
      "price": 59.99,
      "salePrice": null
    },
    {
      "id": 2,
      "name": "Mouse",
      "description": "Wireless mouse",
      "price": 24.50,
      "salePrice": null
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 2,
    "totalCount": 4,
    "totalPages": 2
  }
}



## Reflection
Before this lab, our API accepted anything and crashed with a confusing 500 error when something went wrong.
Now validation rejects bad input like an empty name or a negative price with a clear 400.
The exceptions let me say what went wrong, like "not found", without worrying about status codes in the controller.
The global exception handler turns them into the right response in one place and logs the real error on the server.
Together they keep bad data out and make every error come back in the same format.