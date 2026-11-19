# etapa 1 „build”: imaginea mare cu SDK-ul .NET 8, folosită doar pentru compilare
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
# folderul de lucru din această etapă; comenzile următoare rulează aici
WORKDIR /src
# copiază doar fișierul proiectului (strat separat: pachetele NuGet rămân în cache cât timp el nu se schimbă)
COPY src/Notite.Api/Notite.Api.csproj src/Notite.Api/
# descarcă pachetele NuGet de care are nevoie aplicația
RUN dotnet restore src/Notite.Api/Notite.Api.csproj
# abia acum copiază codul sursă (se schimbă des, deci vine după restore)
COPY src/Notite.Api/ src/Notite.Api/
# compilează în modul Release și pune rezultatul în /out; --no-restore = pachetele sunt deja descărcate
RUN dotnet publish src/Notite.Api/Notite.Api.csproj -c Release -o /out --no-restore

# etapa 2 „finală”: imaginea mică, doar cu runtime-ul ASP.NET 8 (fără SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
# folderul de lucru din imaginea finală
WORKDIR /app
# aduce doar rezultatul compilării din etapa „build”
COPY --from=build /out .
# aplicația ascultă pe toate interfețele, portul 8000 (fără asta ar asculta pe 8080)
ENV ASPNETCORE_URLS=http://0.0.0.0:8000
# documentează portul pe care ascultă aplicația
EXPOSE 8000
# comanda de pornire a aplicației
ENTRYPOINT ["dotnet", "Notite.Api.dll"]
