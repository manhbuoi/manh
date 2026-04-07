using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;
using cuahanggiay.Models;

namespace cuahanggiay.Controllers
{
    [Authorize(Roles = "Admin", AuthenticationSchemes = "CustomerSecurityScheme")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminOrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // 1. QUẢN LÝ SẢN PHẨM (THÊM, SỬA, XÓA, XEM)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Shoes()
        {
            var shoes = await _db.Shoes.Include(s => s.Category).ToListAsync();
            return View(shoes);
        }

        [HttpGet]
        public IActionResult CreateShoe()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShoe(Shoe shoe)
        {
            // Bỏ qua lỗi xác thực ảo khi thêm mới
            ModelState.Remove("Category");
            ModelState.Remove("Brand");

            if (ModelState.IsValid)
            {
                _db.Shoes.Add(shoe);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Shoes");
            }
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpGet]
        public async Task<IActionResult> EditShoe(int id)
        {
            var shoe = await _db.Shoes.FindAsync(id);
            if (shoe == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShoe(Shoe shoe, IFormFile? ImageFile)
        {
            // ========================================================
            // ĐÂY LÀ CHÌA KHÓA DẬP TẮT LỖI TỪ CHỐI UPLOAD ẢNH:
            ModelState.Remove("Category");
            ModelState.Remove("Brand");
            ModelState.Remove("ImageUrl");
            // ========================================================

            if (ModelState.IsValid)
            {
                try
                {
                    // XỬ LÝ UPLOAD ẢNH NẾU ADMIN CÓ CHỌN ẢNH MỚI
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // 1. Tạo tên file duy nhất 
                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);

                        // 2. Đường dẫn lưu file
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        string filePath = Path.Combine(uploadDir, fileName);

                        // 3. Tạo thư mục nếu chưa có
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        // 4. Lưu file vật lý
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        // 5. Cập nhật đường dẫn vào Database
                        shoe.ImageUrl = "/images/" + fileName;
                    }

                    _db.Update(shoe);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm và ảnh thành công!";
                    return RedirectToAction("Shoes"); // Thành công sẽ nhảy về trang Danh sách
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu ảnh: " + ex.Message);
                }
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShoe(int id)
        {
            var shoe = await _db.Shoes.FindAsync(id);
            if (shoe != null)
            {
                _db.Shoes.Remove(shoe);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm!";
            }
            return RedirectToAction("Shoes");
        }

        // ==========================================
        // 2. KIỂM TRA & QUẢN LÝ ĐƠN HÀNG
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _db.Orders
                                  .Include(o => o.User)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật đơn hàng #{order.OrderCode} thành {status}";
            }
            return RedirectToAction("Orders");
        }

        // ==========================================
        // 3. QUẢN LÝ TÀI KHOẢN & PHÂN QUYỀN
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.Role = role;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cấp quyền [{role}] thành công cho tài khoản: {user.Id}!";
            }
            return RedirectToAction("Users");
        }
    }
}