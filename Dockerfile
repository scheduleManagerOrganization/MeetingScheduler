FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj 파일 복사 및 복원
COPY MeetingScheduler.csproj .
RUN dotnet restore

# 전체 소스 복사 및 빌드
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 런타임 이미지
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# 포트 설정
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "MeetingScheduler.dll"]
