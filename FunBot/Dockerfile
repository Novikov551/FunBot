#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FunBot/FunBot.csproj", "FunBot/"]
COPY ["FunBot.Infrastructure/FunBot.Infrastructure.csproj", "FunBot.Infrastructure/"]
COPY ["FunBot.Logic/FunBot.Logic.csproj", "FunBot.Logic/"]
RUN dotnet restore "FunBot/FunBot.csproj"
COPY . .
WORKDIR "/src/FunBot"
RUN dotnet build "FunBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FunBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FunBot.dll"]