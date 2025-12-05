# Dockerfile para MultiAB - Aplicación .NET 8.0 Multi-tenant

# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Instalar dotnet-ef para migraciones (especificar versión para evitar errores)
RUN dotnet tool install --global dotnet-ef --version 8.0.0

# Copiar archivos del proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código fuente
COPY . ./

# Build de la aplicación
RUN dotnet build -c Release -o /app/build

# Publicar la aplicación
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Exponer el puerto 8080 (puedes cambiarlo si lo necesitas)
EXPOSE 8080

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "MultiAB.dll"]

# Etapa 3: SDK para migraciones (opcional, para ejecutar migraciones)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migrations
WORKDIR /app

# Instalar dotnet-ef (especificar versión para evitar errores)
RUN dotnet tool install --global dotnet-ef --version 8.0.0

# Copiar archivos del proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código fuente (necesario para migraciones)
COPY . ./

# Build del proyecto (necesario para que dotnet ef funcione)
RUN dotnet build -c Release

# Comando por defecto (se puede sobrescribir)
CMD ["sh", "-c", "export PATH=\"$PATH:/root/.dotnet/tools\" && dotnet ef database update"]

