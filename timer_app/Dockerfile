FROM mcr.microsoft.com/dotnet/sdk:6.0
RUN dotnet tool install --global dotnet-ef --version 6.0.6

# Copy csproj and restore as distinct layers
COPY ./timer_app_backend.sln ./
COPY ./timer_app/timer_app.csproj ./timer_app/

RUN dotnet restore ./timer_app/timer_app.csproj


COPY . ./
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

# # Run EF Migrations against local db
# WORKDIR "/timer_app"
# RUN dotnet ef database update

EXPOSE 3000
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/timer_app.dll
