# Cửa hàng giày (demo)

Ứng dụng ASP.NET Core MVC mẫu được tùy biến lại để phục vụ báo cáo thực tập.
Nội dung chính:

- **Model & Database**: Sử dụng EF Core InMemory để mô phỏng dữ liệu giày (`Shoe` model).
- **Trang cửa hàng**: `ShoesController` và views hiển thị danh sách sản phẩm, chi tiết.
- **Trang chủ**: bao gồm banner, bộ lọc tĩnh và thêm phần "Sản phẩm nổi bật" động lấy từ database.
- **Trang liên hệ**: biểu mẫu contact với `ContactViewModel`, xác thực dữ liệu trên client/server. Sau khi gửi sẽ hiển thị trang cảm ơn.
- **Layout**: _Layout.cshtml đã được chăm chút, navigation, header, footer được thiết kế responsive bằng Bootstrap.
- **Styles**: tập trung vào `wwwroot/css/site.css` với biến CSS, chữ ký riêng và animation nhỏ.
- **Seed Data**: tự động thêm một vài sản phẩm mẫu khi ứng dụng khởi động.

## Chạy ứng dụng

```powershell
cd d:\WEBNANGCAO\cuahanggiay
dotnet restore
dotnet run
```

Mở trình duyệt tới `https://localhost:5001` để xem trang chủ. Chức năng:

- `Home/Index` hiển thị banner và sản phẩm.
- `Shoes/Index` liệt kê tất cả giày.
- `Shoes/Details/{id}` xem chi tiết một đôi giày.
- `Home/LienHe` gửi thông tin liên hệ.

Bạn có thể mở `Program.cs` và `Data/SeedData.cs` để tùy chỉnh dữ liệu.
## 🛠 Công Nghệ & Cơ Sở Dữ Liệu
- [cite_start]**Framework:** ASP.NET Core 8.0 MVC.
- **ORM:** Entity Framework Core (Code First approach).
- **Database:** Microsoft SQL Server.
- **Tính năng nổi bật:** - Cơ chế **Auto-Migration**: Tự động đồng bộ cấu trúc bảng khi khởi chạy.
    - **Seed Data**: Tự động nạp dữ liệu mẫu cho hệ thống.
    - [cite_start]**Session Management**: Quản lý trạng thái người dùng và giỏ hàng.
---
# Cửa hàng giày (Vua Giày Hiệu)

Ứng dụng ASP.NET Core MVC được xây dựng phục vụ báo cáo thực tập.

## 🛠 Công Nghệ & Cơ Sở Dữ Liệu
- **Framework:** ASP.NET Core 8.0 MVC.
- **ORM:** Entity Framework Core 8.0 (Code First).
- **Database:** Microsoft SQL Server.
- **Tính năng nổi bật:**
  - **Auto-Migration**: Tự động đồng bộ cấu trúc bảng vào CSDL khi khởi chạy.
  - **Seed Data**: Tự động nạp dữ liệu mẫu ban đầu cho hệ thống.
  - **Session Management**: Quản lý trạng thái người dùng và giỏ hàng an toàn.

## Nội dung chính:
- **Trang cửa hàng**: `ShoesController` và views hiển thị danh sách sản phẩm, chi tiết.
- **Trang chủ**: Banner quảng cáo, bộ lọc tĩnh và "Sản phẩm nổi bật" động lấy từ database.
- **Trang liên hệ**: Biểu mẫu contact với `ContactViewModel`, xác thực dữ liệu chặt chẽ trên client/server.
- **Layout**: `_Layout.cshtml` chuyên nghiệp, navigation, header, footer responsive bằng Bootstrap.
- **Styles**: Quản lý tại `wwwroot/css/` với biến CSS, chia tách module rõ ràng.

## 🚀 Hướng dẫn khởi chạy

```powershell
cd d:\WEBNANGCAO\cuahanggiay
dotnet restore
dotnet ef database update
dotnet run
Mọi thay đổi, đóng góp hoặc mở rộng đều chào đón!