using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TassarnasHusApi.Data;
using TassarnasHusApi.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.AspNetCore.Authorization;

namespace TassarnasHusApi.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string wwwRootPath;

        public NewsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            wwwRootPath = hostEnvironment.WebRootPath;
        }

        // GET: News
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.News.OrderByDescending(n=> n.CreatedAt).ToListAsync());
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CreatedAt,ImageFile,CreatedBy")] News news)
        {
            if (ModelState.IsValid)
            {
                // Check for image
                if(news.ImageFile != null) {
                    // Generate unique filename
                    string fileName = Path.GetFileNameWithoutExtension(news.ImageFile.FileName);
                    string extension = Path.GetExtension(news.ImageFile.FileName);
                    news. ImageName = fileName = fileName.Replace(" ", string.Empty) + DateTime.Now.ToString("yyMMDDss") + extension;

                    string path = Path.Combine(wwwRootPath + "/newsImages", fileName);

                    // Resize and store
                    await ResizeAndSaveImage(news.ImageFile, path);
                } else {
                    news.ImageName = "default.png";
                }
                _context.Add(news);
                // Add logged in user
                news.CreatedBy = User.Identity?.Name ?? "Okänd";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreatedAt,ImageFile,CreatedBy")] News news)
        {
            if (id != news.Id)
            {
                return NotFound();
            }
            var existingNews = await _context.News.FindAsync(id);
            if (existingNews == null ) {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                news.CreatedBy = existingNews.CreatedBy;
                // If new image is added
                if (news.ImageFile != null) {
                    // Delete old image, if not "default.png"
                    if(!string.IsNullOrEmpty(existingNews.ImageName) && existingNews.ImageName != "default.png") {
                        string oldPath = Path.Combine(wwwRootPath + "/newsImages" + existingNews.ImageName);
                        if (System.IO.File.Exists(oldPath)) {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Generate unique filename
                    string fileName = Path.GetFileNameWithoutExtension(news.ImageFile.FileName);
                    string extension = Path.GetExtension(news.ImageFile.FileName);
                    news. ImageName = fileName = fileName.Replace(" ", string.Empty) + DateTime.Now.ToString("yyMMDDss") + extension;

                    string path = Path.Combine(wwwRootPath + "/newsImages", fileName);

                    // Resize and store
                    await ResizeAndSaveImage(news.ImageFile, path);
                } else {
                    news.ImageName = existingNews.ImageName;
                }
                try
                {
                    _context.Entry(existingNews).CurrentValues.SetValues(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.Id == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                // Check if image is not null or "default.png"
                if (!string.IsNullOrEmpty(news.ImageName) && news.ImageName != "default.png") {
                    string path = Path.Combine(wwwRootPath + "/newsImages" + news.ImageName);
                    // Delete if exists
                    if (System.IO.File.Exists(path)) {
                        System.IO.File.Delete(path);
                    }
                }
                _context.News.Remove(news);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }

        public async Task ResizeAndSaveImage(IFormFile imageFile, string path)
        {
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset position in stream

            using var image = Image.Load(memoryStream);
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(600, 480),
                Mode = ResizeMode.Crop // Crop eccess
            }));

            // Save image as jpg with quality 90
            await image.SaveAsync(path, new JpegEncoder { Quality = 90 });
        }
    }
}
