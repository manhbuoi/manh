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

    public async Task<IActionResult> Cuahang()
    {
        var shoes = await _context.Shoes
                            .Include(s => s.Brand)
                            .Include(s => s.Category)
                            .OrderByDescending(s => s.IsFeatured)
                            .ThenByDescending(s => s.Daban)
                            .ToListAsync();

        return View("cuahang", shoes);
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