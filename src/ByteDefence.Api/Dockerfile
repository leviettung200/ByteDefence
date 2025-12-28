# Build and package Azure Functions isolated GraphQL API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ByteDefence.Api/ByteDefence.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS final
WORKDIR /home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    AzureWebJobsScriptRoot=/home/site/wwwroot
COPY --from=build /app/publish .

# Base image provides the entrypoint to start the Functions host
