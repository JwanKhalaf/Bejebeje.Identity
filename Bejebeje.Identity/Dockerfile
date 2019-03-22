# set base image as the dotnet 2.2 SDK.
FROM microsoft/dotnet:2.2-sdk AS build-env

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# copy the .csproj file to /app.
COPY *.csproj ./

# restore the project to get all nuget packages ...etc.
RUN dotnet restore Bejebeje.Identity.csproj

COPY . ./

# publish the project into a folder called 'out'.
RUN dotnet publish Bejebeje.Identity.csproj -c Release -o out

# set base image as the dotnet 2.2 runtime.
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# copy the contents of /app/out in the `build-env` and paste it in the
# `/app` directory.
COPY - from=build-env /app/out .

# set the entry point into the application.
ENTRYPOINT ["dotnet", "Bejebeje.Identity.dll"]