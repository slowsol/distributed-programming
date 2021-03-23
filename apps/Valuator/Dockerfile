FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["RepositoryLibrary/RepositoryLibrary.csproj", "RepositoryLibrary/"]
COPY ["Valuator/Valuator.csproj", "Valuator/"]
RUN dotnet restore "Valuator/Valuator.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "Valuator/Valuator.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 
WORKDIR /app/Valuator
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Valuator.dll"]
