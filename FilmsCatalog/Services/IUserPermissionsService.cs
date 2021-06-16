using System;
using FilmsCatalog.Models;

namespace FilmsCatalog.Services
{
    public interface IUserPermissionsService
    {
        Boolean CanEditFilm(Film film);

    }
}