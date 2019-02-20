FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY . .
RUN dotnet restore

# copy everything else and build app
COPY MOTORSPdfHelper/. ./MOTORSPdfHelper/
WORKDIR /app/MOTORSPdfHelper
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app

COPY --from=build /app/MOTORSPdfHelper/out ./
ENTRYPOINT ["dotnet", "MOTORSPdfHelper.dll"]