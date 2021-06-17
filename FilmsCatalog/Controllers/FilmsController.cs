using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using X.PagedList;
using X.PagedList.Mvc;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using FilmsCatalog.Services;

namespace FilmsCatalog.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<FilmsController> _logger;
        private readonly IWebHostEnvironment _webHostingEnvironment;
        private readonly IUserPermissionsService _userPermissionsService;



        public FilmsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<FilmsController> logger, IWebHostEnvironment webHostingEnvironment, IUserPermissionsService userPermissionsService)
        {
            this._context = context;
            this._userManager = userManager;
            this._logger = logger;
            this._webHostingEnvironment = webHostingEnvironment;
            this._userPermissionsService = userPermissionsService;

        }

        // GET: Films
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? page)
        {
           
            var pageNumber = page ?? 1;
            var pageSize = 5;
            var itemsToSkip = (pageNumber - 1) * pageSize;

            var data =  await  _context.Films.Skip(itemsToSkip).Take(pageSize).Include(p => p.Creator).ToListAsync();
            var filmsCount = _context.Films.Count();
            return View(new StaticPagedList<Film>(data, pageNumber, pageSize, filmsCount)) ;
        }

        // GET: Films/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // GET: Films/Create
        public IActionResult Create()
        {
            return View( new FilmViewModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( FilmViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(model);
            }
            var user = await this._userManager.GetUserAsync(this.HttpContext.User);
            if (user==null)
            {
                this.ModelState.AddModelError("UserError", "smth went wrong, please try to reauthenticate");
                return this.View(model);

            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            if (!AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
                return this.View(model);
            }

            var film = new Film
            {
                Name=model.Name,
                Description=model.Description,
                RealeaseDate=model.RealeaseDate,
                DirectorName=model.DirectorName,
                CreatorId=user.Id,
                FileName=fileName
            };
            var filePath = Path.Combine(this._webHostingEnvironment.WebRootPath, "filmPics", film.Id.ToString("N") + fileExt);
            film.FilePath = $"/filmPics/{film.Id:N}{fileExt}";
            using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
            {
                await model.File.CopyToAsync(fileStream);
            }

            _context.Films.Add(film);
            await this._context.SaveChangesAsync();
            _logger.LogInformation("new film has been successfully created");


            return RedirectToAction("Index");

        }

        // GET: Films/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var film = await _context.Films.SingleOrDefaultAsync(f => f.Id == id);
            if (film == null)
            {
                return NotFound();
            }
            if (!_userPermissionsService.CanEditFilm(film))
            {
                this.ModelState.AddModelError("User", "You are prohibited from editing this file ");
                return RedirectToAction("Index");

            }
            var model = new FilmViewModel
            {
                Name=film.Name,
                Description=film.Description,
                DirectorName=film.DirectorName,
                RealeaseDate=film.RealeaseDate               
            };
            ViewData["CreatorId"] =  film.CreatorId; 
            return View(model);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, FilmViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            if (id == null)
            {
                return NotFound();
            }
            var film = _context.Films
                .SingleOrDefault(f => f.Id == id);

            if (film == null)
            {
                return NotFound();
            }
            if (!_userPermissionsService.CanEditFilm(film))
            {
                this.ModelState.AddModelError("User", "You are prohibited from editing this file ");
                return this.View(model);

            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            if (!AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
                return this.View(model);
            }

            film.Name = model.Name;
            film.Description = model.Description;
            film.DirectorName = model.DirectorName;
            film.RealeaseDate = model.RealeaseDate;

            if (model.File != null)
            {
                var filePath = Path.Combine(this._webHostingEnvironment.WebRootPath, "filmPics", film.Id.ToString("N") + Path.GetExtension(film.FilePath));
                System.IO.File.Delete(filePath);


                var newFilePath = Path.Combine(this._webHostingEnvironment.WebRootPath, "filmPics", film.Id.ToString("N") + fileExt);
                film.FilePath = $"/filmPics/{film.Id:N}{fileExt}";
                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Unable to update film data ");
                return this.View(model);
            }
            _logger.LogInformation("film data has been successfuly updated");
            return RedirectToAction("Details",new { id });
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }
            if (!_userPermissionsService.CanEditFilm(film))
            {
                this.ModelState.AddModelError("User", "You are prohibited from editing this file ");
                return RedirectToAction("Index");
            }

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var film = _context.Films
                .SingleOrDefault(f => f.Id == id);

            if (film == null)
            {
                return NotFound();
            }
            if (!_userPermissionsService.CanEditFilm(film))
            {
                this.ModelState.AddModelError("User", "You are prohibited from editing this file ");
                return RedirectToAction("Index");
            }

            var filePath = Path.Combine(this._webHostingEnvironment.WebRootPath, "filmPics", film.Id.ToString("N") + Path.GetExtension(film.FilePath));
            System.IO.File.Delete(filePath);
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            _logger.LogInformation("film  has been successfuly deleted");

            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(Guid id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}
