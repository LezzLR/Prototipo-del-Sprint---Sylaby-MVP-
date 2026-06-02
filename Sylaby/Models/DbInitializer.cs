using Microsoft.AspNetCore.Identity;
using Sylaby.Models;
using System;
using System.Linq;

namespace Sylaby.Models
{
    public static class DbInitializer
    {
        public static void Seed(SylabyDbContext context)
        {
            // Smart Schema Upgrade: Detect if new tables exist, otherwise recreate the database
            try
            {
                // Try querying Cursos. If table does not exist, an exception is thrown
                _ = context.Cursos.Any();
            }
            catch (Exception)
            {
                // Schema has changed! Let's drop and recreate the DB in hot-swap mode
                context.Database.EnsureDeleted();
            }

            // Ensure the database is created (or recreated with new schema)
            context.Database.EnsureCreated();

            // 1. Seed Users (if none exist)
            if (!context.Users.Any())
            {
                var passwordHasher = new PasswordHasher<User>();

                // Seed Director
                var director = new User
                {
                    Email = "director@usmp.pe",
                    Role = "Director"
                };
                director.PasswordHash = passwordHasher.HashPassword(director, "1234");
                context.Users.Add(director);

                // Seed Docentes
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

            // 2. Seed Cursos (if none exist)
            if (!context.Cursos.Any())
            {
                var p1 = context.Users.FirstOrDefault(u => u.Email == "profesor1@usmp.pe");
                var p2 = context.Users.FirstOrDefault(u => u.Email == "profesor2@usmp.pe");

                if (p1 != null)
                {
                    context.Cursos.Add(new Curso { Nombre = "Geometría Analítica", DocenteId = p1.Id });
                    context.Cursos.Add(new Curso { Nombre = "Cálculo I", DocenteId = p1.Id });
                }

                if (p2 != null)
                {
                    context.Cursos.Add(new Curso { Nombre = "Ciudadanía Intercultural", DocenteId = p2.Id });
                    context.Cursos.Add(new Curso { Nombre = "Métodos de Estudio", DocenteId = p2.Id });
                }

                context.SaveChanges();
            }
        }
    }
}
