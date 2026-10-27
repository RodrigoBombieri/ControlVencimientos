# syntax=docker/dockerfile:1
# Ubicación: raíz del repositorio (junto a README.md)

# ---------- Etapa 1: compilar y publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Ruta al .csproj del proyecto web (el que tiene Program.cs)
ARG PROJECT=ControlVencimientosS\ControlVencimientosP\ControlVencimientosP.csproj

COPY ControlVencimientosS/ ControlVencimientosS/
RUN dotnet restore "$PROJECT"
RUN dotnet publish "$PROJECT" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: imagen final (solo runtime, mucho más liviana) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Las imágenes de .NET 8+ escuchan en 8080 y corren sin root por defecto
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Carpeta escribible para adjuntos / claves de Data Protection (se monta como volumen)
RUN mkdir -p /app/data && chown -R $APP_UID /app/data

COPY --from=build /app/publish .
USER $APP_UID

# El nombre del .dll es el AssemblyName del proyecto web
ENTRYPOINT ["dotnet", "ControlVencimientosP.dll"]
