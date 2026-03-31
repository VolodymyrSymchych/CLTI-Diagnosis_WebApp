FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CLTI.Diagnosis.sln", "./"]
COPY ["CLTI.Diagnosis/CLTI.Diagnosis.csproj", "CLTI.Diagnosis/"]
COPY ["CLTI.Diagnosis.Client/CLTI.Diagnosis.Client.csproj", "CLTI.Diagnosis.Client/"]
RUN dotnet restore "CLTI.Diagnosis/CLTI.Diagnosis.csproj"

COPY . .
WORKDIR /src/CLTI.Diagnosis
RUN rm -f /src/global.json \
    && dotnet publish "CLTI.Diagnosis.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet CLTI.Diagnosis.dll --urls http://0.0.0.0:${PORT:-10000}"]
