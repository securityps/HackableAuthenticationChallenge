# Place this file in the AuthenticationChallenge directory and run:
# docker build -t authenticationchallenge .
# to build the image

# Run the container via:
# docker run -d -p 5000:5000 ntucker000/authenticationchallenge
FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/sdk:2.1
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "AuthenticationChallenge.dll"]