# PricingService - Bài thực hành 5

## Thành phần

- `src/PricingService.Api`: Pricing Service REST API, tính giá các sản phẩm TRI/HSI/FAI/CAR.
- `tests/PricingService.Test`: unit tests cho domain, handler và controller.
- `tests/PricingService.IntegrationTest`: integration tests cho endpoint `/api/Pricing`.
- `eureka-server`: Eureka Server Spring Boot được cung cấp trong file nén.

## Chạy Eureka Server

Yêu cầu Java 17+ và Gradle wrapper:

```powershell
cd eureka-server
.\gradlew.bat bootRun
```

Mở `http://localhost:8761` để kiểm tra Eureka.

## Chạy PricingService

Mở terminal khác từ thư mục gốc:

```powershell
dotnet run --project .\src\PricingService.Api\PricingService.Api.csproj
```

Service chạy tại `http://localhost:5040` và đăng ký với Eureka qua cấu hình trong `src/PricingService.Api/appsettings.json`.

## Kiểm thử

```powershell
dotnet test .\PricingService.sln
```

Các test integration không phụ thuộc PostgreSQL vì PricingService hiện dùng `InMemoryDataStore` cho dữ liệu tariff mẫu.
