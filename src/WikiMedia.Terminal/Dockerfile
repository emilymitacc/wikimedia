#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/WikiMedia.Terminal/WikiMedia.Terminal.csproj", "src/WikiMedia.Terminal/"]
COPY ["src/WikiMedia.Infrastructure/WikiMedia.Infrastructure.csproj", "src/WikiMedia.Infrastructure/"]
COPY ["src/WikiMedia.Domain/WikiMedia.Domain.csproj", "src/WikiMedia.Domain/"]
COPY ["src/WikiMedia.Core/WikiMedia.Core.csproj", "src/WikiMedia.Core/"]
RUN dotnet restore "src/WikiMedia.Terminal/WikiMedia.Terminal.csproj"
COPY . .
WORKDIR "/src/src/WikiMedia.Terminal"
RUN dotnet build "WikiMedia.Terminal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WikiMedia.Terminal.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WikiMedia.Terminal.dll"]