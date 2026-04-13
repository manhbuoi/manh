using Microsoft.EntityFrameworkCore;
using cuahanggiay.Models;
using BCrypt.Net; // Đảm bảo bạn đã cài package BCrypt.Net-Next

namespace cuahanggiay.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // --------------------------------------------------------
            // 1. TẠO TÀI KHOẢN NGƯỜI DÙNG & PHÂN QUYỀN (Ưu tiên chạy trước)
            // --------------------------------------------------------
            var defaultPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");
            
            if (!context.Users.Any(u => u.Email == "admin@vuagiayhieu.com")) {
                context.Users.Add(new User { FullName = "Admin", Email = "admin@vuagiayhieu.com", Password = defaultPassword, Role = "Admin" });
            }

            if (!context.Users.Any(u => u.Email == "owner@vuagiayhieu.com")) {
                context.Users.Add(new User { FullName = "Store Owner", Email = "owner@vuagiayhieu.com", Password = defaultPassword, Role = "StoreOwner" });
            }

            if (!context.Users.Any(u => u.Email == "shipper@vuagiayhieu.com")) {
                context.Users.Add(new User { FullName = "Shipper Staff", Email = "shipper@vuagiayhieu.com", Password = defaultPassword, Role = "Shipper" });
            }

            if (!context.Users.Any(u => u.Email == "customer@vuagiayhieu.com")) {
                context.Users.Add(new User { FullName = "Khách Hàng Mẫu", Email = "customer@vuagiayhieu.com", Password = defaultPassword, Role = "Customer" });
            }

            // Lưu Users vào CSDL trước
            context.SaveChanges();


            // --------------------------------------------------------
            // 2. TẠO DỮ LIỆU SẢN PHẨM (Chỉ tạo nếu chưa có giày nào)
            // --------------------------------------------------------
            if (context.Shoes.Any())
            {
                return;   // DB đã có dữ liệu giày, bỏ qua phần dưới
            }

            // Tạo Brands
            var brandNike = new Brand { Name = "Nike" };
            var brandAdidas = new Brand { Name = "Adidas" };
            var brandPuma = new Brand { Name = "Puma" };
            
            if (!context.Brands.Any())
            {
                context.Brands.AddRange(brandNike, brandAdidas, brandPuma);
            }

            // Tạo Categories
            var catMen = new Category { Name = "Giày Nam" };
            var catWomen = new Category { Name = "Giày Nữ" };
            var catSandals = new Category { Name = "Dép" };
            
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(catMen, catWomen, catSandals);
            }

            context.Shoes.AddRange(
                // Giày Nam
                new Shoe { Name = "Nike Air Force 1 Nam", Description = "Giày thể thao huyền thoại với thiết kế cổ điển cho nam", Price = 2500000, OldPrice = 3000000, Category = catMen, Brand = brandNike, ImageUrl = "nike-af1-men.jpg", IsFeatured = true, Rating = 5, Tonkho = 50, Daban = 120 },
                new Shoe { Name = "Adidas Ultraboost 22 Nam", Description = "Giày chạy bộ cực êm với công nghệ Boost cho nam", Price = 3200000, Category = catMen, Brand = brandAdidas, ImageUrl = "adidas-ultraboost-men.jpg", IsFeatured = true, Rating = 4, Tonkho = 30, Daban = 85 },
                new Shoe { Name = "Nike Air Max 270 Nam", Description = "Giày sneaker với đệm khí Air Max cho nam", Price = 2800000, OldPrice = 3500000, Category = catMen, Brand = brandNike, ImageUrl = "nike-airmax-men.jpg", IsFeatured = false, Rating = 5, Tonkho = 40, Daban = 95 },
                new Shoe { Name = "Adidas Stan Smith Nam", Description = "Giày công sở cổ điển cho nam", Price = 2200000, Category = catMen, Brand = brandAdidas, ImageUrl = "adidas-stansmith-men.jpg", IsFeatured = false, Rating = 4, Tonkho = 25, Daban = 60 },
                new Shoe { Name = "Puma RS-X3 Nam", Description = "Giày sneaker retro cho nam", Price = 1900000, Category = catMen, Brand = brandPuma, ImageUrl = "puma-rsx3-men.jpg", IsFeatured = true, Rating = 4, Tonkho = 35, Daban = 75 },
                new Shoe { Name = "Nike Pegasus 39 Nam", Description = "Giày chạy bộ nhẹ nhàng cho nam", Price = 2600000, Category = catMen, Brand = brandNike, ImageUrl = "nike-pegasus-men.jpg", IsFeatured = false, Rating = 5, Tonkho = 45, Daban = 110 },
                new Shoe { Name = "Adidas Sambas Nam", Description = "Giày công sở da thật cho nam", Price = 3500000, OldPrice = 4000000, Category = catMen, Brand = brandAdidas, ImageUrl = "adidas-sambas-men.jpg", IsFeatured = true, Rating = 5, Tonkho = 20, Daban = 45 },
                
                // Giày Nữ
                new Shoe { Name = "Nike Air Force 1 Nữ", Description = "Giày thể thao huyền thoại với thiết kế cổ điển cho nữ", Price = 2400000, OldPrice = 2900000, Category = catWomen, Brand = brandNike, ImageUrl = "nike-af1-women.jpg", IsFeatured = true, Rating = 5, Tonkho = 45, Daban = 100 },
                new Shoe { Name = "Adidas Ultraboost 22 Nữ", Description = "Giày chạy bộ cực êm với công nghệ Boost cho nữ", Price = 3100000, Category = catWomen, Brand = brandAdidas, ImageUrl = "adidas-ultraboost-women.jpg", IsFeatured = true, Rating = 4, Tonkho = 35, Daban = 80 },
                new Shoe { Name = "Nike Air Max 270 Nữ", Description = "Giày sneaker với đệm khí Air Max cho nữ", Price = 2700000, OldPrice = 3400000, Category = catWomen, Brand = brandNike, ImageUrl = "nike-airmax-women.jpg", IsFeatured = false, Rating = 5, Tonkho = 38, Daban = 90 },
                new Shoe { Name = "Adidas Stan Smith Nữ", Description = "Giày công sở cổ điển cho nữ", Price = 2100000, Category = catWomen, Brand = brandAdidas, ImageUrl = "adidas-stansmith-women.jpg", IsFeatured = false, Rating = 4, Tonkho = 28, Daban = 55 },
                new Shoe { Name = "Puma RS-X3 Nữ", Description = "Giày sneaker retro cho nữ", Price = 1800000, Category = catWomen, Brand = brandPuma, ImageUrl = "puma-rsx3-women.jpg", IsFeatured = true, Rating = 4, Tonkho = 32, Daban = 70 },
                new Shoe { Name = "Nike Pegasus 39 Nữ", Description = "Giày chạy bộ nhẹ nhàng cho nữ", Price = 2500000, Category = catWomen, Brand = brandNike, ImageUrl = "nike-pegasus-women.jpg", IsFeatured = false, Rating = 5, Tonkho = 42, Daban = 105 },
                new Shoe { Name = "Adidas Sambas Nữ", Description = "Giày công sở da thật cho nữ", Price = 3400000, OldPrice = 3900000, Category = catWomen, Brand = brandAdidas, ImageUrl = "adidas-sambas-women.jpg", IsFeatured = true, Rating = 5, Tonkho = 22, Daban = 40 },
                
                // Dép
                new Shoe { Name = "Dép Nike Benassi", Description = "Dép đi biển thoải mái, thoáng khí", Price = 800000, Category = catSandals, Brand = brandNike, ImageUrl = "nike-benassi.jpg", IsFeatured = false, Rating = 4, Tonkho = 60, Daban = 150 },
                new Shoe { Name = "Dép Adidas Adilette", Description = "Dép thể thao cổ điển, thoải mái", Price = 900000, Category = catSandals, Brand = brandAdidas, ImageUrl = "adidas-adilette.jpg", IsFeatured = true, Rating = 4, Tonkho = 55, Daban = 130 },
                new Shoe { Name = "Dép Puma Smash", Description = "Dép sneaker phong cách retro", Price = 750000, Category = catSandals, Brand = brandPuma, ImageUrl = "puma-smash.jpg", IsFeatured = false, Rating = 4, Tonkho = 48, Daban = 95 }
            );

            context.SaveChanges();
        }
    }
}