FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConvertShell/ConvertShell.csproj", "ConvertShell/"]
RUN dotnet restore "ConvertShell/ConvertShell.csproj"
COPY . .
WORKDIR "/src/ConvertShell"
RUN dotnet build "ConvertShell.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConvertShell.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConvertShell.dll"]
