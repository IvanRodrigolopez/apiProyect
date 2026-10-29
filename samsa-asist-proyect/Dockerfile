# Consulte https://aka.ms/customizecontainer para obtener más información.

# ==========================================
# FASE 1: Base (Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ==========================================
# FASE 2: Compilación (SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 👇 IMPORTANTE: Copiar las 4 capas de tu arquitectura
COPY ["samsa-asist-proyect/samsa-asist-proyect.csproj", "samsa-asist-proyect/"]
COPY ["samsa_asist_proyect.En/samsa_asist_proyect.En.csproj", "samsa_asist_proyect.En/"]
COPY ["samsa_asist_proyect.Dt/samsa_asist_proyect.Dt.csproj", "samsa_asist_proyect.Dt/"]
COPY ["samsa_asist_proyect.Ng/samsa_asist_proyect.Ng.csproj", "samsa_asist_proyect.Ng/"]

# Restaurar dependencias de la API (esto restaurará las demás automáticamente)
RUN dotnet restore "./samsa-asist-proyect/samsa-asist-proyect.csproj"

# Copiar el resto del código fuente
COPY . .

# Compilar el proyecto
WORKDIR "/src/samsa-asist-proyect"
RUN dotnet build "./samsa-asist-proyect.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# FASE 3: Publicación
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./samsa-asist-proyect.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# FASE 4: Final (Producción)
# ==========================================
FROM base AS final
WORKDIR /app

# Copiar los archivos publicados
COPY --from=publish /app/publish .

# 👇 Crear la carpeta de imágenes para evitar errores al guardar fotos
RUN mkdir -p /app/wwwroot/images

ENTRYPOINT ["dotnet", "samsa-asist-proyect.dll"]