dotnet ef migrations add InitialCreate --context WriteDbContext --output-dir Migrations/Write
dotnet ef migrations add InitialCreate --context ReadDbContext --output-dir Migrations/Read

dotnet ef database update --context ReadDbContext
dotnet ef database update --context WriteDbContext
