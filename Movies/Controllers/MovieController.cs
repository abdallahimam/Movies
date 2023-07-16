using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Abstractions;
using Movies.Models;
using Movies.ViewModels;
using NToastNotify;

namespace Movies.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        public MovieController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create ()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                return View(model);
            }

            var files = Request.Form.Files;

            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please, select a movie poster.");
                return View("MovieForm", model);
            }

            var poster = files.FirstOrDefault();
            var allowedExtensions = new List<string> { ".jpg", ".png" };
            if (!allowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .jpg, .png images are allowed");
                return View("MovieForm", model);
            }

            if (poster.Length > 1048576)
            {
                model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Image size cannot be greater than 1 Mb.");
                return View("MovieForm", model);
            }

            using var dataStream = new MemoryStream();

            await poster.CopyToAsync(dataStream);

            var movie = new Movie
            {
                GenreId = model.GenreId,
                Title = model.Title,
                Year = model.Year,
                Rate = model.Rate,
                Storeline = model.Storeline,
                Poster = dataStream.ToArray()
            };

            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Movie added successfully!");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit (int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Rate = movie.Rate,
                Storeline = movie.Storeline,
                Poster = movie.Poster,
                GenreId = movie.GenreId,
                Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync()
            };

            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                return View(model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);
            if (movie == null)
                return NotFound();

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();

                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                var allowedExtensions = new List<string> { ".jpg", ".png" };
                if (!allowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .jpg, .png images are allowed");
                    return View("MovieForm", model);
                }

                if (poster.Length > 1048576)
                {
                    model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Image size cannot be greater than 1 Mb.");
                    return View("MovieForm", model);
                }

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = model.Title;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.Storeline = model.Storeline;
            movie.GenreId = model.GenreId;

            await _context.SaveChangesAsync();

            _toastNotification.AddSuccessToastMessage("Movie updated successfully!");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }
    }
}
