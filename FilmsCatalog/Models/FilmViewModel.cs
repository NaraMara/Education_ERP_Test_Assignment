using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class FilmViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime RealeaseDate { get; set; }
        public string DirectorName { get; set; }
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }


    }
}
