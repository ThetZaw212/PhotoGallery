# PhotoGallery
Change it before running. For development prefer __User Secrets__ or environment variables.
- The upload endpoint stores image bytes in the database (Photo.ImageData). Ensure your DB has sufficient storage and consider switching to blob storage for large scale.
- The API uses role-based checks for deletions (admin role). Seed an admin user/role if required.

## What this project is
A Razor Pages photo gallery with accompanying API endpoints for listing, uploading and deleting photos. Uses ASP.NET Core (.NET 9), EF Core, and ASP.NET Core Identity.

## How to run

Prerequisites
- .NET 9 SDK installed
- SQL Server (or compatible) instance
- Visual Studio 2022 (recommended) or any editor/IDE that supports .NET 9

Configuration
1. Update the database connection in `appsettings.json` (`ConnectionStrings:DefaultConnection`) to point to your SQL Server. Do NOT commit secrets or credentials. Prefer using __User Secrets__ or environment variables for production credentials.

Database & Migrations (CLI)
- From the project root (project containing the DbContext):
  - Restore packages:
    ```
    dotnet restore
    ```
  - If migrations already exist:
    ```
    dotnet ef database update
    ```
  - If no migrations exist, create and apply the initial migration:
    ```
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

## What architecture and patterns are used

High-level architecture
- Razor Pages for the UI (Pages folder) — server-rendered pages for list, detail and upload.
- API Controllers for AJAX/clients (Controllers folder) — e.g., `Controllers/Gallery/PhotosController.cs`.
- EF Core (DbContext) for data access.
- ASP.NET Core Identity for authentication/authorization.

Patterns and practices observed
- Dependency Injection — services (DbContext, UserManager, etc.) are injected via constructor injection.
- Transaction / Unit of Work — explicit EF Core database transactions are used inside operations that need atomicity (e.g., photo upload + tag creation). DbContext provides a unit-of-work style boundary.
- DTO / View Model pattern — API returns shaped objects (anonymous DTOs) and uses request models (e.g., `UploadPhotoModel`) to decouple UI/API from entity models.
- Standardized API response wrapper — project uses a `ResponseHelper` / `DefaultResponseMessageModel` to provide consistent API responses.
- Authorization attributes — `[Authorize]` on controllers/actions and explicit role checks (e.g., `userManager.IsInRoleAsync(user, "admin")`).
- No explicit Repository pattern — the code interacts with `PhotoGalleryDbContext` directly instead of using a repository abstraction. If you prefer testability or separation, consider adding repository interfaces and implementations later.
- Simple CQRS separation — read operations and write operations are separated by different controller methods (not a formal CQRS library).

Other practical notes
- Pagination, filtering and sorting are implemented at the query level in the API (server-side).
- Images are returned as data URIs (base64) in API responses — convenient for small apps, but consider returning URLs to files stored in blob storage for production.

---