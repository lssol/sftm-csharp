FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builder
ARG csproj="tree-matching-csharp.Visualization"
WORKDIR /src
COPY . ./
RUN dotnet restore $csproj --source "https://api.nuget.org/v3/index.json;http://baget.amarislab.com:5000/v3/index.json"
RUN dotnet build $csproj -c Release
RUN dotnet publish $csproj -c Release -o "/publish"

FROM mcr.microsoft.com/dotnet/aspnet:5.0
# Certificate
RUN wget https://assets.amarislab.com/datalake.ca.pem
RUN openssl x509 -in datalake.ca.pem -inform PEM -out datalake.ca.crt
RUN mv datalake.ca.crt /usr/local/share/ca-certificates
RUN update-ca-certificates
WORKDIR /app
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
ENTRYPOINT ["dotnet", "tree-matching-csharp.Visualization.dll"]
COPY --from=builder /publish .
