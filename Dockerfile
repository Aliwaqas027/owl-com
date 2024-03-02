#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt update -y && \
	apt install python3 python3-pip -y && \
	python3 -m pip install PyVirtualDisplay pdfkit
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OwlApi.csproj", "."]
RUN dotnet restore "./OwlApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "OwlApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OwlApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OwlApi.dll"]