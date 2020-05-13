# set environment

In Bash, you run: export ASPNETCORE_ENVIRONMENT=Development

# identityserver4 migrations

dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context ConfigurationDbContext --output-dir Migrations/ConfigurationDb Initial
dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context PersistedGrantDbContext --output-dir Migrations/PersistedGrantDb Initial

# create new migration

dotnet-ef migrations add --project Bejebeje.Identity --startup-project Bejebeje.Identity --context ApplicationDbContext InitialCreate

# update database

dotnet-ef database update --context ApplicationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

dotnet-ef database update --context ConfigurationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

dotnet-ef database update --context PersistedGrantDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity

# generate migration sql scripts

dotnet ef migrations script 0 20190928133250_InitialCreate --context ApplicationDbContext --project Bejebeje.Identity --startup-project Bejebeje.Identity
