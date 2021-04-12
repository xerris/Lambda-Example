FROM amazon/aws-lambda-dotnet:core3.1 AS base
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

WORKDIR /app
COPY ./LambdaProject/LambdaProject/src ./

WORKDIR /app

RUN dotnet restore LambdaProject/LambdaProject.csproj

RUN dotnet build LambdaProject/LambdaProject.csproj --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish LambdaProject/LambdaProject.csproj \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  
RUN ls /app/publish
FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .