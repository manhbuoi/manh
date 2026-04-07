using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using cuahanggiay.ViewModels;
using cuahanggiay.Data;
using cuahanggiay.Models;

namespace cuahanggiay.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private const string CartSessionKey = "CartItems";

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson)) return new List<CartItemViewModel>();
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        // ========================================================
        // 1. TRANG GIỎ HÀNG (Ép đăng nhập)
        // ========================================================
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem giỏ hàng và thanh toán!";
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartFromSession();
            var model = new CheckoutViewModel
            {
                CartItems = cart,
                CartTotal = cart.Sum(item => item.Total)
            };
            return View(model);
        }

        // ========================================================
        // 2. THÊM VÀO GIỎ / MUA NGAY (Ép đăng nhập)
        // ========================================================
        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, decimal price, string? imageUrl, int quantity = 1, string? size = null, string action = "add_cart")
        {
            // KIỂM TRA: Nếu chưa đăng nhập -> Chuyển hướng về Login
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mua hàng!";
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId && item.Size == size);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    ImageUrl = imageUrl,
                    Quantity = quantity,
                    Size = size
                });
            }

            SaveCartToSession(cart);
            TempData["SuccessMessage"] = $"Đã thêm {productName} (Size: {size}) vào giỏ hàng!";

            if (action == "buy_now")
            {
                return RedirectToAction("Index"); // Chuyển sang trang Thanh toán
            }

            // Nếu bấm "Thêm vào giỏ" thì load lại trang chi tiết sản phẩm
            return RedirectToAction("Details", "Shoes", new { id = productId });
        }

        // ========================================================
        // 3. THÊM VÀO GIỎ BẰNG AJAX (Ép đăng nhập)
        // ========================================================
        [HttpPost]
        public IActionResult AddToCartAjax(int shoeId, int quantity = 1)
        {
            // KIỂM TRA: Dành cho nút bấm dạng ngầm (AJAX)
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectUrl = "/Account/Login", message = "Vui lòng đăng nhập để mua hàng!" });
            }

            var shoe = _context.Shoes.FirstOrDefault(s => s.Id == shoeId);
            if (shoe == null) return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == shoeId);

            if (existingItem != null) existingItem.Quantity += quantity;
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = shoeId,
                    ProductName = shoe.Name,
                    Price = shoe.Price,
                    ImageUrl = shoe.ImageUrl,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            return Json(new { success = true, message = $"Đã thêm {shoe.Name} vào giỏ hàng!" });
        }

        // ========================================================
        // 4. XÓA SẢN PHẨM KHỎI GIỎ (Ép đăng nhập)
        // ========================================================
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, string? size = null)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCartFromSession();
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartToSession(cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

        // ========================================================
        // 5. XỬ LÝ ĐẶT HÀNG & LƯU DATABASE (Ép đăng nhập)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var cart = GetCartFromSession();
                if (cart == null || !cart.Any())
                {
                    ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
                    model.CartItems = cart ?? new List<CartItemViewModel>();
                    model.CartTotal = 0;
                    return View("Index", model);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");
                int userId = int.Parse(userIdClaim);

                string orderCode = "VGH" + new Random().Next(10000, 99999).ToString();

                var order = new Order
                {
                    UserId = userId,
                    OrderCode = orderCode,
                    OrderDate = DateTime.Now,
                    ReceiverName = model.FullName,
                    ReceiverPhone = model.PhoneNumber,
                    ShippingAddress = model.Address,
                    Notes = model.Note,
                    Status = "Chờ xử lý"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ShoeId = item.ProductId,
                        Quantity = item.Quantity,
                    };
                    _context.OrderItems.Add(orderItem);
                }
                await _context.SaveChangesAsync();

                decimal totalAmount = cart.Sum(i => i.Total);
                HttpContext.Session.Remove(CartSessionKey);

                return RedirectToAction("Success", new { orderId = orderCode, amount = totalAmount });
            }

            model.CartItems = GetCartFromSession();
            model.CartTotal = model.CartItems.Sum(i => i.Total);
            return View("Index", model);
        }

        // GET: Lấy số lượng giỏ hàng cho Navbar
        [HttpGet]
        public IActionResult GetCartCount()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { count = 0 }); // Chưa đăng nhập thì giỏ hàng báo 0
            }

            var cart = GetCartFromSession();
            var count = cart.Sum(item => item.Quantity);
            return Json(new { count = count });
        }

        [HttpGet]
        public IActionResult Success(string orderId, decimal amount)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(orderId)) return RedirectToAction("Index", "Home");

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            return View();
        }
    }
}