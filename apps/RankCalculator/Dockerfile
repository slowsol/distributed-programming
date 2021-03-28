FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["RepositoryLibrary/RepositoryLibrary.csproj", "RepositoryLibrary/"]
COPY ["RankCalculator/RankCalculator.csproj", "RankCalculator/"]
RUN dotnet restore "RankCalculator/RankCalculator.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "RankCalculator/RankCalculator.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 
WORKDIR /app/RankCalculator
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "RankCalculator.dll"]
