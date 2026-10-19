# HardenACrud API

ASP.NET Core Web API with validated input, centralized exception handling,
and standardized responses. All errors are returned as Problem Details
(`application/problem+json`).

## Endpoints

| Method | Route              | Success          | Error codes                 |
|--------|--------------------|------------------|-----------------------------|
| GET    | /api/products      | 200 (paged list) | 400 (invalid page/pageSize) |
| GET    | /api/products/{id} | 200              | 404, 500                    |
| POST   | /api/products      | 201 + Location   | 400 (validation), 409, 500  |
| PUT    | /api/products/{id} | 200              | 400 (validation), 404, 409, 500 |
| DELETE | /api/products/{id} | 204 (no body)    | 404, 500                    |

## Paged list format

GET /api/products?page=1&pageSize=10 returns:

    { "data": [ ... ], "meta": { "page", "pageSize", "totalCount", "pageCount" } }

## Validation rules

- Name: required, 2 to 100 characters
- Description: max 500 characters
- Price: 0.01 to 1,000,000
- SalePrice (optional): 0.01 to 1,000,000 and must be less than Price
- page >= 1, pageSize between 1 and 100