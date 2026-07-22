FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY StudentManagementSystem.csproj ./
RUN dotnet restore StudentManagementSystem.csproj

COPY . .
RUN dotnet publish StudentManagementSystem.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "StudentManagementSystem.dll"]
