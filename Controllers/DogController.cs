using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using TassarnasHusApi.Data;
using TassarnasHusApi.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.AspNetCore.Authorization;

namespace TassarnasHusApi.Controllers
{
    public class DogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string wwwRootPath;

        public DogController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            wwwRootPath = hostEnvironment.WebRootPath;
        }

        // GET: Dog
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dogs.OrderByDescending(d=> d.CreatedAt).ToListAsync());
        }

        // GET: Dog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dog = await _context.Dogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dog == null)
            {
                return NotFound();
            }

            return View(dog);
        }

        // GET: Dog/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,Breed,City,Sex,Description,Age,Size,Adopted,ImageFile")] Dog dog)
        {
            if (ModelState.IsValid)
            {
                // Check for image
                if(dog.ImageFile != null) {
                    // Generate unique filename
                    string fileName = Path.GetFileNameWithoutExtension(dog.ImageFile.FileName);
                    string extension = Path.GetExtension(dog.ImageFile.FileName);
                    dog.ImageName = fileName = fileName.Replace(" ", string.Empty) + DateTime.Now.ToString("yymmssfff") + extension;

                    string path = Path.Combine(wwwRootPath + "/dogImages", fileName);

                    // Resize and store in filestystem
                    await ResizeAndSaveImage(dog.ImageFile,path);
                } else {
                    dog.ImageName = "default.png";
                }
                _context.Add(dog);
                // Add logged in user
                dog.CreatedBy = User.Identity?.Name ?? "Okänd";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dog);
        }

        // GET: Dog/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dog = await _context.Dogs.FindAsync(id);
            if (dog == null)
            {
                return NotFound();
            }
            return View(dog);
        }

        // POST: Dog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Breed,City,Sex,Description,Age,Size,Adopted,ImageFile")] Dog dog)
        {
            if (id != dog.Id)
            {
                return NotFound();
            }

            var existingDog = await _context.Dogs.FindAsync(id);
            if (existingDog == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                dog.CreatedBy = existingDog.CreatedBy;
                // If new image is added
                if (dog.ImageFile != null) {
                    // Delete old image, if not "default.png"
                    if (!string.IsNullOrEmpty(existingDog.ImageName) && existingDog.ImageName != "default.png") {
                        string oldPath = Path.Combine(wwwRootPath + "/dogImages" + existingDog.ImageName);
                        if (System.IO.File.Exists(oldPath)) {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Generate unique filename
                    string fileName = Path.GetFileNameWithoutExtension(dog.ImageFile.FileName);
                    string extension = Path.GetExtension(dog.ImageFile.FileName);
                    dog.ImageName = fileName = fileName.Replace(" ", string.Empty) + DateTime.Now.ToString("yymmssfff") + extension;

                    string path = Path.Combine(wwwRootPath + "/dogImages", fileName);

                    // Resize and store in filestystem
                    await ResizeAndSaveImage(dog.ImageFile,path);
                } else {
                    // Keep old image if not any new
                    dog.ImageName = existingDog.ImageName;
                }
                try
                {
                    _context.Entry(existingDog).CurrentValues.SetValues(dog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DogExists(dog.Id))
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
            return View(dog);
        }

        // GET: Dog/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dog = await _context.Dogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dog == null)
            {
                return NotFound();
            }

            return View(dog);
        }

        // POST: Dog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog != null)
            {
                // Check if image is not null och default.png
                if (!string.IsNullOrEmpty(dog.ImageName) && dog.ImageName != "default.png") {
                    string path = Path.Combine(wwwRootPath + "/dogImages" + dog.ImageName);
                    // Delete if exists
                    if(System.IO.File.Exists(path)) {
                        System.IO.File.Delete(path);
                    }
                }
                _context.Dogs.Remove(dog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DogExists(int id)
        {
            return _context.Dogs.Any(e => e.Id == id);
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
