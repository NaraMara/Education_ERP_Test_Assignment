using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class FilmViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RealeaseDate { get; set; }
        public string DirectorName { get; set; }

    }
}
