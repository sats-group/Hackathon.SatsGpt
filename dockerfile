# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish SATS.AI.Api -c Release -o /app/out

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 5166
ENTRYPOINT ["dotnet", "SATS.AI.Api.dll"]