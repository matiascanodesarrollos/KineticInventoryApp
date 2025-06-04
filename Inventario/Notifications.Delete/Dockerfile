#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Notifications.Delete/Notifications.Delete.csproj", "Notifications.Delete/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Notifications.Delete/Notifications.Delete.csproj"
COPY . .
WORKDIR "/src/Notifications.Delete"
RUN dotnet build "Notifications.Delete.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notifications.Delete.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Delete.dll"]