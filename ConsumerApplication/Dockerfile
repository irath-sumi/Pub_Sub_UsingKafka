#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ConsumerApplication/ConsumerApplication.csproj", "ConsumerApplication/"]
RUN dotnet restore "ConsumerApplication/ConsumerApplication.csproj"
COPY . .
WORKDIR "/src/ConsumerApplication"
RUN dotnet build "ConsumerApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsumerApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsumerApplication.dll"]