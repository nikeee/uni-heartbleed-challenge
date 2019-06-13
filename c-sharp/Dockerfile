FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
	WORKDIR /app

	COPY *.csproj ./
	RUN dotnet restore

	COPY . ./
	RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine AS runtime
	WORKDIR /app
	ENTRYPOINT ["time", "dotnet", "Challenge.dll"]
	COPY --from=build /app/out .
