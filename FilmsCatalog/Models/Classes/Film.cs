using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class Film
    {
        public Guid Id { get; set; } = new Guid();
        public string  CreatorId { get; set; }
        public User Creator { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RealeaseDate { get; set; }
        public string DirectorName { get; set; }
        /*
        public string FileName { get; set; }
        public string FilePath { get; set; }
        */
    }
}
