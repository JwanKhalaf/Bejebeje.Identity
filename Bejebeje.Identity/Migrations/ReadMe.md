# Set Environment

In Bash, you run: export ASPNETCORE_ENVIRONMENT=Development

# Create New Migration

dotnet-ef migrations --project Bejebeje.Identity --startup-project Bejebeje.Identity add InitialCreate

# Update Database

dotnet-ef database update --context ApplicationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

dotnet-ef database update --context ConfigurationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity