# set base image as the dotnet 2.2 SDK.
FROM microsoft/dotnet:2.2-sdk AS build-env

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# remember, our current working directory within the container is /app
# we now copy the .csproj file (from local machine) to /app (in the container).
COPY *.csproj ./

# on the container (we are in /app folder) we restore the project to get
# all the referenced nuget packages.
RUN dotnet restore Bejebeje.Identity.csproj

# again, remember, our current working directory within the container is /app
# we now copy all the files (from local machine) to /app (in the container).
COPY . ./

# and again, on the container (we are in /app folder)
# we now publish the project into a folder called 'out'.
RUN dotnet publish Bejebeje.Identity.csproj -c Release -o out

# set base image as the dotnet 2.2 runtime.
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime

# an environmental variable for the seed user website.
ENV InitialSeedConfiguration__Website=https://www.code-hound.com

# an environmental variable for the seed user username.
ENV InitialSeedConfiguration__Username=jwan

# an environmental variable for the seed user password.
ENV InitialSeedConfiguration__Password=Password123!

# an environmental variable for the seed user's last name.
ENV InitialSeedConfiguration__LastName=Doe

# an environmental variable for the seed user's first name.
ENV InitialSeedConfiguration__FirstName=Jwan

# an environmental variable for the seed user's email.
ENV InitialSeedConfiguration__Email=j.khalaf@example.com

# telling the application what port to run on.
ENV ASPNETCORE_URLS=http://*:5000

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# copy the contents of /app/out in the `build-env` and paste it in the
# `/app` directory of the new runtime container.
COPY --from=build-env /app/out .

# set the entry point into the application.
ENTRYPOINT ["dotnet", "Bejebeje.Identity.dll", "/seed"]