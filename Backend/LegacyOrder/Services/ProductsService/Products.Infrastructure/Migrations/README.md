Place EF Core migration files for `AppDbContext` in this folder.

Best practice in this solution:
- Keep migrations in `Products.Infrastructure` because `AppDbContext` and persistence configuration live here.
- Keep `Products.Domain` persistence-agnostic (no EF migration files there).

