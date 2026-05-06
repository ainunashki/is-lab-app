#sborka
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY  IsLabApp.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

#zapusk
FROM mcr.microsoft.com/dotnet/aspnet:10.0 
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "IsLabApp.dll"]
