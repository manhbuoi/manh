using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;
using cuahanggiay.Models;
using cuahanggiay.ViewModels;

namespace cuahanggiay.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // --- 1. ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            string normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            // Kiểm tra mật khẩu mã hóa BCrypt
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                SetSession(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    // Lấy quyền từ Database (mặc định là Customer nếu null)
                    new Claim(ClaimTypes.Role, user.Role ?? "Customer") 
                };

                // Phải chỉ định rõ ClaimTypes.Name và ClaimTypes.Role để User.IsInRole() hoạt động
                var identity = new ClaimsIdentity(claims, "CustomerSecurityScheme", ClaimTypes.Name, ClaimTypes.Role);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CustomerSecurityScheme", principal, new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                    AllowRefresh = true
                });

                TempData["SuccessMessage"] = "Đăng nhập thành công! Chúc bạn có trải nghiệm mua sắm tuyệt vời.";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect dựa trên Role
                if (user.Role == "Admin" || user.Role == "StoreOwner")
                {
                    return RedirectToAction("Index", "AdminOrder"); // Bạn nhớ tạo AdminOrderController nhé
                }
                else if (user.Role == "Shipper")
                {
                    return RedirectToAction("Index", "Shipper"); // Bạn nhớ tạo ShipperController nhé
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // --- 2. ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                return View(model);
            }

            var newUser = new User
            {
                FullName = model.FullName.Trim(),
                Email = normalizedEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Customer" // Mặc định đăng ký mới là khách hàng thường
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            SetSession(newUser);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                new Claim(ClaimTypes.Name, newUser.FullName ?? newUser.Email),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role ?? "Customer")
            };

            var identity = new ClaimsIdentity(claims, "CustomerSecurityScheme", ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CustomerSecurityScheme", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            });

            TempData["SuccessMessage"] = "Đăng ký thành công! Bạn đã được đăng nhập tự động.";
            return RedirectToAction("Index", "Home");
        }

        // --- 3. HỒ SƠ CÁ NHÂN ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.User.Identity?.IsAuthenticated == true
                ? HttpContext.User.FindFirstValue(ClaimTypes.Email)
                : HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            var model = new UserProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = HttpContext.User.Identity?.IsAuthenticated == true
                ? HttpContext.User.FindFirstValue(ClaimTypes.Email)
                : HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            if (!string.Equals(user.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var isEmailTaken = await _db.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLowerInvariant() && u.Id != user.Id);
                if (isEmailTaken)
                {
                    ModelState.AddModelError("Email", "Email này đang được sử dụng bởi tài khoản khác.");
                    return View(model);
                }
                user.Email = model.Email.Trim().ToLowerInvariant();
            }

            user.FullName = model.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _db.SaveChangesAsync();
            SetSession(user);
            TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công.";
            return RedirectToAction("Profile");
        }

        // --- 4. ĐĂNG XUẤT ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CustomerSecurityScheme");
            ClearSession();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất an toàn.";
            return RedirectToAction("Index", "Home");
        }

        // --- 5. HÀM HỖ TRỢ ---
        private void SetSession(User user)
        {
            HttpContext.Session.SetString("UserName", user.FullName ?? user.Email);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role ?? "Customer");
        }

        private void ClearSession()
        {
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserRole");
        }
    }
}