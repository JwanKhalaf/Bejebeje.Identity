# Set Environment

In Bash, you run: export ASPNETCORE_ENVIRONMENT=Development

# IdentityServer4 Migrations

dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context ConfigurationDbContext --output-dir Migrations/ConfigurationDb Initial
dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context PersistedGrantDbContext --output-dir Migrations/PersistedGrantDb Initial

# Create New Migration

dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context ApplicationDbContext InitialCreate

# Update Database

dotnet-ef database update --context ApplicationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

dotnet-ef database update --context ConfigurationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

dotnet-ef database update --context PersistedGrantDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity