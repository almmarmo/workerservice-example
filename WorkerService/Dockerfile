FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["WorkerService/WorkerService.csproj", "WorkerService/"]
COPY ["Rabbit.Workservice/Rabbit.Workservice.csproj", "Rabbit.Workservice/"]
RUN dotnet restore "WorkerService/WorkerService.csproj"
COPY . .
WORKDIR "/src/WorkerService"
RUN dotnet build "WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerService.dll"]