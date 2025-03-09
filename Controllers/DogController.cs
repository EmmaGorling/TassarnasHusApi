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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dogs.ToListAsync());
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Breed,City,Sex,Description,Age,Size,Adopted,ImageFile")] Dog dog)
        {
            if (ModelState.IsValid)
            {
                // Check for image
                if(dog.ImageFile != null) {
                    // Generate unique filename
                    string fileName = Path.GetFileNameWithoutExtension(dog.ImageFile.FileName);
                    string extension = Path.GetExtension(dog.ImageFile.FileName);
                    dog.ImageName = fileName = fileName.Replace(" ", string.Empty) + DateTime.Now.ToString("yymmssff") + extension;

                    string path = Path.Combine(wwwRootPath + "/dogImages", fileName);

                    // Stroe in filesystem
                    using(var fileStream = new FileStream(path, FileMode.Create)) {
                        await dog.ImageFile.CopyToAsync(fileStream);
                    };
                } else {
                    dog.ImageName = "default.png";
                }
                _context.Add(dog);
                // Add logged in user
                dog.CreatedBy = User.Identity?.Name ?? "Unknown";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dog);
        }

        // GET: Dog/Edit/5
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Breed,City,Sex,Description,Age,Size,Adopted,ImageName")] Dog dog)
        {
            if (id != dog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dog);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dog = await _context.Dogs.FindAsync(id);
            if (dog != null)
            {
                _context.Dogs.Remove(dog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DogExists(int id)
        {
            return _context.Dogs.Any(e => e.Id == id);
        }

        
    }
}
