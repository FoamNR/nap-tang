FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy shared library csproj and restore dependencies
COPY src/Shared/EasyTrack.Shared.csproj src/Shared/
# Copy MediaService csproj and restore dependencies
COPY src/Services/MediaService/EasyTrack.MediaService.csproj src/Services/MediaService/
RUN dotnet restore src/Services/MediaService/EasyTrack.MediaService.csproj

# Copy source code and build the project
COPY src/Shared/ src/Shared/
COPY src/Services/MediaService/ src/Services/MediaService/
WORKDIR /app/src/Services/MediaService
RUN dotnet publish -c Release -o /app/publish

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8003
EXPOSE 8003
ENTRYPOINT ["dotnet", "EasyTrack.MediaService.dll"]
