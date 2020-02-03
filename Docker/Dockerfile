# Place this file in the AuthenticationChallenge directory and run:
# docker build -t authenticationchallenge .
# to build the image

# Run the container via:
# docker run -d -p 5000:5000 authenticationchallenge
FROM mcr.microsoft.com/dotnet/core/sdk:2.1


RUN apt update
RUN apt install -y libgdiplus
WORKDIR /app
COPY . .


EXPOSE 5000

ENTRYPOINT dotnet run