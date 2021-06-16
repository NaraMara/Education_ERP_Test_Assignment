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
namespace FilmsCatalog.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<FilmsController> _logger;


        public FilmsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<FilmsController> logger)
        {
            this._context = context;
            this._userManager = userManager;
            this._logger = logger;

        }

        // GET: Films
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? page)
        {
            /*
             var pageNumber = page ?? 1;
            var data =  await  _context.Films.Include(p => p.Creator).ToListAsync();
            var onePage = data.ToPagedList(pageNumber, 5);
            return View(onePage) ;
            */
            var pageNumber = page ?? 1;
            var pageSize = 5;
            var itemsToSkip = (pageNumber - 1) * pageSize;

            var data =  await  _context.Films.Skip(itemsToSkip).Take(pageSize).Include(p => p.Creator).ToListAsync();
            var filmsCount = _context.Films.Count();
            var onePage = new StaticPagedList<Film>(data, pageNumber, pageSize, filmsCount);
                //data.ToPagedList(pageNumber, pageSize);

            

            return View(onePage) ;
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
                return this.View();
            }
            var user = await this._userManager.GetUserAsync(this.HttpContext.User);
            if (user==null)
            {
                this.ModelState.AddModelError("UserError", "smth went wrong, please try to reauthenticate");
                return this.View();

            }
            var film = new Film
            {
                Name=model.Name,
                Description=model.Description,
                RealeaseDate=model.RealeaseDate,
                DirectorName=model.DirectorName,
                CreatorId=user.Id
            };

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

            film.Name = model.Name;
            film.Description = model.Description;
            film.DirectorName = model.DirectorName;
            film.RealeaseDate = model.RealeaseDate;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Unable to update film data ");

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

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var film = await _context.Films.FindAsync(id);
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(Guid id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}
