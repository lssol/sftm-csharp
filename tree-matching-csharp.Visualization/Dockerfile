FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS builder
ARG csproj="tree-matching-csharp.Visualization"
WORKDIR /src
COPY . ./
RUN dotnet restore $csproj 
RUN dotnet build $csproj -c Release
RUN dotnet publish $csproj -c Release -o "/publish"

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal
# Certificate
RUN apt update
RUN apt install -y wget
RUN wget --timeout=90000 https://assets.amarislab.com/datalake.ca.pem
RUN openssl x509 -in datalake.ca.pem -inform PEM -out datalake.ca.crt
RUN mv datalake.ca.crt /usr/local/share/ca-certificates
RUN update-ca-certificates
WORKDIR /app
ENTRYPOINT ["dotnet", "tree-matching-csharp.Visualization.dll"]
COPY --from=builder /publish .
