FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend.csproj", "."]
RUN dotnet restore "backend.csproj"
COPY . .
RUN dotnet build "backend.csproj" -c release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]