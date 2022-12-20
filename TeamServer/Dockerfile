FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TeamServer/TeamServer.csproj", "TeamServer/"]
RUN dotnet restore "TeamServer/TeamServer.csproj"
COPY . .
WORKDIR "/src/TeamServer"
RUN dotnet build "TeamServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeamServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeamServer.dll"]
