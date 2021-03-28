FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["RepositoryLibrary/RepositoryLibrary.csproj", "RepositoryLibrary/"]
COPY ["EventsLogger/EventsLogger.csproj", "EventsLogger/"]
RUN dotnet restore "EventsLogger/EventsLogger.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish "EventsLogger/EventsLogger.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 
WORKDIR /app/EventsLogger
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "EventsLogger.dll"]
