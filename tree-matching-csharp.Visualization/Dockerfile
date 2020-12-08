FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
COPY . ./

# Certificate
RUN wget https://assets.amarislab.com/datalake.ca.pem
RUN openssl x509 -in datalake.ca.pem -inform PEM -out datalake.ca.crt
RUN mv datalake.ca.crt /usr/local/share/ca-certificates
RUN update-ca-certificates

RUN dotnet publish tree-matching-csharp.Visualization -c Release -o out
ENTRYPOINT ["dotnet", "out/tree-matching-csharp.Visualization.dll"]