# Sử dụng image SDK để build mã nguồn
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy và restore thư viện
COPY ["WebQLministop.csproj", "./"]
RUN dotnet restore "./WebQLministop.csproj"

# Copy toàn bộ mã nguồn và biên dịch (Publish)
COPY . .
RUN dotnet publish "WebQLministop.csproj" -c Release -o /app/publish

# Môi trường chạy thực tế (chỉ chứa Runtime cho nhẹ server)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render thường yêu cầu chạy trên cổng 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WebQLministop.dll"]
