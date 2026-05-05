FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ServerFitnessCorrector/ ./ServerFitnessCorrector/
COPY PoseDetection/ ./PoseDetection/

RUN dotnet restore ServerFitnessCorrector/FitnessCorrector.WebAPI/FitnessCorrector.WebAPI.csproj
RUN dotnet publish ServerFitnessCorrector/FitnessCorrector.WebAPI/FitnessCorrector.WebAPI.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app/out

RUN apt-get update \
    && apt-get install -y python3 python3-pip libgl1 libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out ./
COPY --from=build /src/PoseDetection ./PoseDetection

RUN python3 -m pip install --no-cache-dir -r PoseDetection/requirements.txt

ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

ENTRYPOINT ["dotnet", "FitnessCorrector.WebAPI.dll"]
