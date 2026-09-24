# ---------- Etapa de build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Firmeza.sln .
COPY src/Firmeza.Web/Firmeza.Web.csproj src/Firmeza.Web/
COPY src/Firmeza.Tests/Firmeza.Tests.csproj src/Firmeza.Tests/
RUN dotnet restore src/Firmeza.Web/Firmeza.Web.csproj

COPY src/ src/
RUN dotnet publish src/Firmeza.Web/Firmeza.Web.csproj -c Release -o /app/publish

# ---------- Etapa final ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Web.dll"]
