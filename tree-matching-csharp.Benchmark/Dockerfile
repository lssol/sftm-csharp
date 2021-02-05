FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS builder
WORKDIR /src
COPY . ./
RUN dotnet restore 
RUN dotnet build -c Release
RUN dotnet publish -c Release -o "/publish"

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
RUN apk add --no-cache wget
RUN apk add --no-cache openssl
RUN wget --timeout=90000 https://assets.amarislab.com/datalake.ca.pem
RUN openssl x509 -in datalake.ca.pem -inform PEM -out datalake.ca.crt
RUN mv datalake.ca.crt /usr/local/share/ca-certificates
RUN update-ca-certificates

WORKDIR /app
ENTRYPOINT ["dotnet", "tree-matching-csharp.Benchmark.dll"]
COPY --from=builder /publish .
