#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["lnd_invoice.Blazor/lnd_invoice.Blazor.csproj", "lnd_invoice.Blazor/"]
RUN dotnet restore "lnd_invoice.Blazor/lnd_invoice.Blazor.csproj"
COPY . .
WORKDIR "/src/lnd_invoice.Blazor"
RUN dotnet build "lnd_invoice.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "lnd_invoice.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "lnd_invoice.Blazor.dll"]