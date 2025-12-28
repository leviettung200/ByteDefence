# Build and package the SignalR host
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ByteDefence.SignalR/ByteDefence.SignalR.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ByteDefence.SignalR.dll"]
