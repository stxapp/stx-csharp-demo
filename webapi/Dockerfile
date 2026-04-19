# Use the official .NET 8 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory in the image to /app
WORKDIR /app

# Copy the .csproj file and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the API source code
COPY . ./

# Build the API for release
RUN dotnet publish -c Release -o out

# Use the official .NET 8 ASP.NET runtime image as the base for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory in the image to /app
WORKDIR /app

# Copy the built API from the build-env image
COPY --from=build-env /app/out .

# Expose port 8080 for the API (default in .NET 8)
EXPOSE 8080

# Start the API
ENTRYPOINT ["dotnet", "STX.Sdk.Api.dll"]
