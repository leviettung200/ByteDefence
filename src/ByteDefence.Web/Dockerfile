# Build the Blazor WASM client and serve via nginx
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ByteDefence.Web/ByteDefence.Web.csproj -c Release -o /app/publish

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html
COPY src/ByteDefence.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot .
COPY src/ByteDefence.Web/wwwroot/appsettings.Docker.json ./appsettings.json

EXPOSE 80
