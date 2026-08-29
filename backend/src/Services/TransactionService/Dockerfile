FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy shared library csproj and restore dependencies
COPY src/Shared/EasyTrack.Shared.csproj src/Shared/
# Copy TransactionService csproj and restore dependencies
COPY src/Services/TransactionService/EasyTrack.TransactionService.csproj src/Services/TransactionService/
RUN dotnet restore src/Services/TransactionService/EasyTrack.TransactionService.csproj

# Copy source code and build the project
COPY src/Shared/ src/Shared/
COPY src/Services/TransactionService/ src/Services/TransactionService/
WORKDIR /app/src/Services/TransactionService
RUN dotnet publish -c Release -o /app/publish

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8002
EXPOSE 8002
ENTRYPOINT ["dotnet", "EasyTrack.TransactionService.dll"]
