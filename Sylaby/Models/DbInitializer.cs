using Microsoft.AspNetCore.Identity;
using Sylaby.Models;
using System.Linq;

namespace Sylaby.Models
{
    public static class DbInitializer
    {
        public static void Seed(SylabyDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Check if there are already users
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            var passwordHasher = new PasswordHasher<User>();

            // 1. Seed Director
            var director = new User
            {
                Email = "director@usmp.pe",
                Role = "Director"
            };
            director.PasswordHash = passwordHasher.HashPassword(director, "1234");
            context.Users.Add(director);

            // 2. Seed Docentes
            var profesor1 = new User
            {
                Email = "profesor1@usmp.pe",
                Role = "Docente"
            };
            profesor1.PasswordHash = passwordHasher.HashPassword(profesor1, "1234");
            context.Users.Add(profesor1);

            var profesor2 = new User
            {
                Email = "profesor2@usmp.pe",
                Role = "Docente"
            };
            profesor2.PasswordHash = passwordHasher.HashPassword(profesor2, "1234");
            context.Users.Add(profesor2);

            context.SaveChanges();
        }
    }
}
