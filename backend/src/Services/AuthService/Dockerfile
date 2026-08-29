FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy shared library csproj and restore dependencies
COPY src/Shared/EasyTrack.Shared.csproj src/Shared/
# Copy AuthService csproj and restore dependencies
COPY src/Services/AuthService/EasyTrack.AuthService.csproj src/Services/AuthService/
RUN dotnet restore src/Services/AuthService/EasyTrack.AuthService.csproj

# Copy source code and build the project
COPY src/Shared/ src/Shared/
COPY src/Services/AuthService/ src/Services/AuthService/
WORKDIR /app/src/Services/AuthService
RUN dotnet publish -c Release -o /app/publish

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8001
EXPOSE 8001
ENTRYPOINT ["dotnet", "EasyTrack.AuthService.dll"]
