using cuahanggiay.Data;
using cuahanggiay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cuahanggiay.Controllers
{
    public class ShoesController : Controller
    {
        // Sử dụng biến _db như bạn đã định nghĩa
        private readonly ApplicationDbContext _db;

        // Constructor nhận context từ hệ thống
        public ShoesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Shoes
        // Kết hợp tham số cũ (category, search, sort) và tham số mới (maxPrice)
        public async Task<IActionResult> Index(string? category, string? search, string? sort, decimal? maxPrice)
        {
            // 1. Khởi tạo query và dùng Include để lấy dữ liệu bảng liên quan (giống code cũ của bạn)
            var query = _db.Shoes
                .AsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Brand)
                .AsQueryable();

            // 2. Lọc theo Danh mục (Sử dụng Name.ToLower() để so sánh không phân biệt hoa thường)
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category != null && s.Category.Name.ToLower() == category.ToLower());
                ViewData["ActiveCategory"] = category;
            }

            // 3. Lọc theo Giá (Tính năng mới cho thanh trượt)
            if (maxPrice.HasValue && maxPrice > 0)
            {
                query = query.Where(s => s.Price <= maxPrice.Value);
                ViewData["CurrentMaxPrice"] = maxPrice;
            }
            else
            {
                ViewData["CurrentMaxPrice"] = 10000000; // Mặc định 10 triệu
            }

            // 4. Tìm kiếm (Giữ nguyên logic kiểm tra null Description của bạn)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search) || (s.Description != null && s.Description.Contains(search)));
                ViewData["CurrentSearch"] = search;
            }

            // 5. Sắp xếp (Dùng switch case chuyên nghiệp như bản cũ của bạn)
            query = sort?.ToLower() switch
            {
                "price_desc" => query.OrderByDescending(s => s.Price),
                "price_asc" => query.OrderBy(s => s.Price),
                "newest" => query.OrderByDescending(s => s.Id),
                _ => query.OrderBy(s => s.Name), // Mặc định sắp xếp theo tên như code cũ
            };

            ViewData["Sort"] = sort;

            // 6. TRUYỀN DỮ LIỆU CHO SIDEBAR (Categories & Brands)
            // Phần này giúp View render được danh sách các link danh mục và thương hiệu
            ViewData["Categories"] = await _db.Categories.ToListAsync();
            ViewData["Brands"] = await _db.Brands.ToListAsync();

            // Trả về list sản phẩm đã lọc
            var list = await query.ToListAsync();
            return View(list);
        }

        // GET: /Shoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            // Giữ nguyên phần Include để trang chi tiết không bị lỗi thiếu thông tin
            var shoe = await _db.Shoes
                .AsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Brand)
                .FirstOrDefaultAsync(s => s.Id == id.Value);

            if (shoe == null) return NotFound();

            return View(shoe);
        }
    }
}