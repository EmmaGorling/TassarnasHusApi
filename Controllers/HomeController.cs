using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TassarnasHusApi.Data;
using TassarnasHusApi.Models;

namespace TassarnasHusApi.Controllers;

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
        var latestNews = await _context.News
            .OrderByDescending(n => n.CreatedAt)
            .Take(4) // Hämta de 5 senaste nyheterna
            .ToListAsync();
        return View(latestNews);
    }


    public async Task<IActionResult> Dogs()
    {
        var dogs = await _context.Dogs
            .Where(d => d.Adopted == false)
            .ToListAsync();

        return View(dogs);
    }


    public async Task<IActionResult> News()
    {
        var news = await _context.News
            .ToListAsync();
        
        return View(news);
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
