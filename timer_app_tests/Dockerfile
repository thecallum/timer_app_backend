FROM mcr.microsoft.com/dotnet/sdk:6.0
RUN dotnet tool install --global dotnet-ef --version 6.0.6


ARG CONNECTION_STRING
ENV CONNECTION_STRING=$CONNECTION_STRING

# Copy csproj and restore as distinct layers
COPY ./timer_app_backend.sln ./
COPY ./timer_app/timer_app.csproj ./timer_app/
COPY ./timer_app_tests/timer_app_tests.csproj ./timer_app_tests/

RUN dotnet restore ./timer_app/timer_app.csproj
RUN dotnet restore ./timer_app_tests/timer_app_tests.csproj


COPY . ./
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

# # Run EF Migrations against local db
# WORKDIR "/timer_app"
# RUN dotnet ef database update

CMD ["dotnet", "test"]

# EXPOSE 3000
# CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/timer_app.dll
