# RESEARCH NOTE: Intentionally using .NET 6.0 base (CVE-2022-34716) for experiment 
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base 
WORKDIR /app 
EXPOSE 80 
  
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 
WORKDIR /src 
COPY ["OrderService.csproj", "."] 
RUN dotnet restore 
COPY . . 
RUN dotnet build -c Release -o /app/build 
   
FROM build AS publish 
RUN dotnet publish -c Release -o /app/publish 
  
FROM base AS final 
WORKDIR /app 
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "OrderService.dll"]