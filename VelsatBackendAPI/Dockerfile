FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["VelsatBackendAPI/APIDatero.csproj", "VelsatBackendAPI/"]
COPY ["VelsatBackendAPI.Data/APIDatero.Data.csproj", "VelsatBackendAPI.Data/"]
COPY ["VelsatBackendAPI.Model/APIDatero.Model.csproj", "VelsatBackendAPI.Model/"]

RUN dotnet restore "VelsatBackendAPI/APIDatero.csproj"

COPY . .

WORKDIR "/src/VelsatBackendAPI"
RUN dotnet build "APIDatero.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "APIDatero.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APIDatero.dll"]