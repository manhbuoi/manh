using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Models;
using cuahanggiay.Data;

namespace cuahanggiay.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {

        ViewData["Categories"] = await _context.Categories.ToListAsync();

        var shoes = await _context.Shoes
                            .Include(s => s.Brand)
                            .Include(s => s.Category)
                            .Where(s => s.IsFeatured == true)
                            .OrderByDescending(s => s.Daban)
                            .Take(6)
                            .ToListAsync();

        return View(shoes);
    }

    public async Task<IActionResult> Cuahang(int? categoryId, int? brandId, string searchString, string sortOrder)
    {
        ViewData["Categories"] = await _context.Categories.ToListAsync();
        ViewData["Brands"] = await _context.Brands.ToListAsync();
        
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentBrand"] = brandId;
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;

        var shoesQuery = _context.Shoes
                            .Include(s => s.Brand)
                            .Include(s => s.Category)
                            .AsQueryable();

        if (categoryId.HasValue)
        {
            shoesQuery = shoesQuery.Where(s => s.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            shoesQuery = shoesQuery.Where(s => s.BrandId == brandId.Value);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            shoesQuery = shoesQuery.Where(s => s.Name.Contains(searchString) || (s.Description != null && s.Description.Contains(searchString)));
        }

        switch (sortOrder)
        {
            case "price_asc":
                shoesQuery = shoesQuery.OrderBy(s => s.Price);
                break;
            case "price_desc":
                shoesQuery = shoesQuery.OrderByDescending(s => s.Price);
                break;
            case "name_asc":
                shoesQuery = shoesQuery.OrderBy(s => s.Name);
                break;
            case "name_desc":
                shoesQuery = shoesQuery.OrderByDescending(s => s.Name);
                break;
            default:
                shoesQuery = shoesQuery.OrderByDescending(s => s.IsFeatured).ThenByDescending(s => s.Daban);
                break;
        }

        var shoes = await shoesQuery.ToListAsync();

        return View("Cuahang", shoes);
    }
  
    public IActionResult Gioithieu()
    {
        return View();
    }

    public IActionResult tintuc()
    {
        return View();
    }

    public IActionResult LienHe()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LienHe(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            var contact = new Contact
            {
                FullName = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Subject = model.Subject,
                Message = model.Message,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi trong thời gian sớm nhất!";
            return RedirectToAction(nameof(LienHe));
        }

        return View(model);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}