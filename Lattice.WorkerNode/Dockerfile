FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish Lattice.WorkerNode -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "WorkerNode.dll"]
