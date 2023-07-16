using Movies.Models;
using System.ComponentModel.DataAnnotations;

namespace Movies.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string? Title { get; set; }

        public int Year { get; set; }

        [Range(1, 10)]
        public double Rate { get; set; }

        [Required, MaxLength(2500)]
        public string? Storeline { get; set; }

        [Display(Name = "Select Poster")]
        public byte[]? Poster { get; set; }

        [Display(Name = "Genre")]
        public byte GenreId { get; set; }

        public IEnumerable<Genre>? Genres { get; set; }
    }
}
