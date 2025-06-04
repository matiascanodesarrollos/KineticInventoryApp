#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Notifications.Update/Notifications.Update.csproj", "Notifications.Update/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Notifications.Update/Notifications.Update.csproj"
COPY . .
WORKDIR "/src/Notifications.Update"
RUN dotnet build "Notifications.Update.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notifications.Update.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Update.dll"]