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
                // Try querying Silabos. If table does not exist, an exception is thrown
                _ = context.Silabos.Any();
                _ = context.PropuestasMejora.Any();
                _ = context.ObservacionesDirector.Any();
                _ = context.BitacoraAcciones.Any();
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

            // 3. Seed Silabos (if none exist)
            if (!context.Silabos.Any())
            {
                var p1 = context.Users.FirstOrDefault(u => u.Email == "profesor1@usmp.pe");
                var p2 = context.Users.FirstOrDefault(u => u.Email == "profesor2@usmp.pe");

                var cursoGA = context.Cursos.FirstOrDefault(c => c.Nombre == "Geometría Analítica");
                var cursoCalc = context.Cursos.FirstOrDefault(c => c.Nombre == "Cálculo I");
                var cursoCiud = context.Cursos.FirstOrDefault(c => c.Nombre == "Ciudadanía Intercultural");
                var cursoMet = context.Cursos.FirstOrDefault(c => c.Nombre == "Métodos de Estudio");

                if (cursoGA != null && p1 != null)
                {
                    context.Silabos.Add(new Silabo
                    {
                        CursoId = cursoGA.Id,
                        EscuelaProfesional = "Ingeniería de Computación y Sistemas",
                        CicloAcademico = "I",
                        Creditos = 4,
                        HorasSemanales = 5,
                        DocenteResponsable = p1.Email,
                        Sumilla = "Asignatura orientada al desarrollo de habilidades matemáticas aplicadas a la ingeniería. Comprende el estudio de números reales, sistema de coordenadas rectangulares, funciones y secciones cónicas.",
                        Competencias = "• Aplica el análisis y la síntesis para resolver problemas matemáticos.\n• Utiliza la inducción y la deducción como estrategias de adquisición del conocimiento.",
                        Capacidades = "• Resolver problemas relacionados con números reales.\n• Aplicar conceptos de geometría analítica.\n• Analizar y representar funciones matemáticas.\n• Resolver problemas relacionados con secciones cónicas.",
                        ProgramacionContenidos = "UNIDAD I: Números Reales\n• Operaciones con números reales.\n• Intervalos.\n• Inecuaciones.\n• Valor absoluto.\n\nUNIDAD II: Sistema de Coordenadas Rectangulares\n• Plano cartesiano.\n• Distancia entre puntos.\n• Pendiente.\n• Rectas.\n• Lugares geométricos.\n\nUNIDAD III: Funciones\n• Dominio y rango.\n• Funciones polinómicas.\n• Funciones racionales.\n• Función inversa.\n• Composición de funciones.\n\nUNIDAD IV: Secciones Cónicas\n• Circunferencia.\n• Parábola.\n• Elipse.\n• Hipérbola.\n• Coordenadas polares.",
                        EstrategiasDiddacticas = "• Exposición interactiva.\n• Discusión guiada.\n• Resolución de ejercicios.\n• Desarrollo de casos prácticos.",
                        SistemaEvaluacion = "• Prácticas calificadas.\n• Evaluaciones parciales.\n• Examen final.",
                        Bibliografia = "• Figueroa, Ricardo. Geometría Analítica.\n• Lehmann, Charles. Geometría Analítica.\n• Espinoza Ramos, Eduardo. Geometría Analítica Plana.",
                        Estado = "En edición",
                        CreadoPor = p1.Email,
                        ModificadoPor = p1.Email,
                        FechaCreacion = DateTime.Now.AddDays(-10),
                        FechaModificacion = DateTime.Now.AddDays(-3)
                    });
                }

                if (cursoCalc != null && p1 != null)
                {
                    context.Silabos.Add(new Silabo
                    {
                        CursoId = cursoCalc.Id,
                        EscuelaProfesional = "Ingeniería de Computación y Sistemas",
                        CicloAcademico = "II",
                        Creditos = 4,
                        HorasSemanales = 5,
                        DocenteResponsable = p1.Email,
                        Sumilla = "Asignatura que desarrolla el pensamiento matemático formal a través del estudio de límites, continuidad, derivadas e integrales de funciones reales de una variable, con aplicaciones en ingeniería.",
                        Competencias = "• Aplica el razonamiento matemático para modelar y resolver problemas de ingeniería.\n• Interpreta fenómenos físicos y técnicos mediante el uso del cálculo diferencial e integral.",
                        Capacidades = "• Calcular límites y analizar continuidad de funciones.\n• Aplicar las reglas de derivación.\n• Resolver problemas de optimización.\n• Calcular integrales definidas e indefinidas.\n• Aplicar integrales en cálculo de áreas y volúmenes.",
                        ProgramacionContenidos = "UNIDAD I: Límites y Continuidad\n• Concepto de límite.\n• Propiedades de los límites.\n• Límites laterales.\n• Continuidad de funciones.\n\nUNIDAD II: Derivadas\n• Definición de derivada.\n• Reglas de derivación.\n• Derivada de funciones implícitas.\n• Derivadas de orden superior.\n\nUNIDAD III: Aplicaciones de la Derivada\n• Criterios de monotonía y concavidad.\n• Máximos y mínimos.\n• Problemas de optimización.\n• Regla de L'Hôpital.\n\nUNIDAD IV: Integrales\n• Integral indefinida.\n• Métodos de integración.\n• Integral definida y Teorema Fundamental del Cálculo.\n• Aplicaciones: áreas y volúmenes.",
                        EstrategiasDiddacticas = "• Clase magistral con ejemplos aplicados.\n• Talleres de resolución de problemas.\n• Trabajo colaborativo en grupos.\n• Uso de software matemático (GeoGebra).",
                        SistemaEvaluacion = "• Prácticas calificadas (30%).\n• Exámenes parciales (40%).\n• Examen final (30%).",
                        Bibliografia = "• Stewart, James. Cálculo de una Variable.\n• Larson, Ron y Edwards, Bruce. Cálculo.\n• Thomas, George. Cálculo con Geometría Analítica.",
                        Estado = "En edición",
                        CreadoPor = p1.Email,
                        ModificadoPor = p1.Email,
                        FechaCreacion = DateTime.Now.AddDays(-8),
                        FechaModificacion = DateTime.Now.AddDays(-2)
                    });
                }

                if (cursoCiud != null && p2 != null)
                {
                    context.Silabos.Add(new Silabo
                    {
                        CursoId = cursoCiud.Id,
                        EscuelaProfesional = "Ingeniería de Computación y Sistemas",
                        CicloAcademico = "I",
                        Creditos = 3,
                        HorasSemanales = 4,
                        DocenteResponsable = p2.Email,
                        Sumilla = "Asignatura que promueve el reconocimiento de la diversidad cultural del Perú y el mundo, fomentando el respeto, la convivencia y la responsabilidad ciudadana en contextos interculturales.",
                        Competencias = "• Valora la diversidad cultural como fuente de riqueza e identidad nacional.\n• Actúa con responsabilidad ciudadana en un contexto globalizado e intercultural.",
                        Capacidades = "• Identificar las principales culturas del Perú y sus aportes.\n• Analizar situaciones de interculturalidad en el contexto global.\n• Proponer soluciones a conflictos culturales basadas en el respeto.\n• Ejercer ciudadanía activa con responsabilidad ética.",
                        ProgramacionContenidos = "UNIDAD I: Identidad y Diversidad Cultural\n• Concepto de cultura e interculturalidad.\n• Identidad nacional y diversidad étnica del Perú.\n• Patrimonio cultural tangible e intangible.\n\nUNIDAD II: Ciudadanía y Democracia\n• Derechos y deberes ciudadanos.\n• Democracia participativa.\n• Estado de derecho e instituciones.\n\nUNIDAD III: Globalización e Interculturalidad\n• Impacto de la globalización en las culturas.\n• Migraciones y nuevas identidades.\n• Comunicación intercultural.\n\nUNIDAD IV: Ética y Responsabilidad Social\n• Valores éticos en la vida pública.\n• Responsabilidad social y ambiental.\n• Proyectos de ciudadanía activa.",
                        EstrategiasDiddacticas = "• Análisis de casos reales.\n• Debates y mesas redondas.\n• Proyectos de investigación cultural.\n• Visitas virtuales a museos y sitios patrimoniales.",
                        SistemaEvaluacion = "• Participación en debates (20%).\n• Trabajos de investigación (40%).\n• Examen integrador (40%).",
                        Bibliografia = "• Tubino, Fidel. La interculturalidad crítica como proyecto ético-político.\n• Kymlicka, Will. Ciudadanía Multicultural.\n• ONU. Declaración Universal de Derechos Humanos.",
                        Estado = "En edición",
                        CreadoPor = p2.Email,
                        ModificadoPor = p2.Email,
                        FechaCreacion = DateTime.Now.AddDays(-12),
                        FechaModificacion = DateTime.Now.AddDays(-4)
                    });
                }

                if (cursoMet != null && p2 != null)
                {
                    context.Silabos.Add(new Silabo
                    {
                        CursoId = cursoMet.Id,
                        EscuelaProfesional = "Ingeniería de Computación y Sistemas",
                        CicloAcademico = "I",
                        Creditos = 2,
                        HorasSemanales = 3,
                        DocenteResponsable = p2.Email,
                        Sumilla = "Asignatura orientada al desarrollo de competencias académicas para el aprendizaje universitario. Incluye técnicas de estudio, gestión del tiempo, comprensión lectora, redacción académica e investigación bibliográfica.",
                        Competencias = "• Gestiona de manera autónoma su proceso de aprendizaje universitario.\n• Aplica técnicas y estrategias de estudio eficientes y sostenibles.",
                        Capacidades = "• Aplicar técnicas de organización del tiempo y planificación del estudio.\n• Desarrollar habilidades de comprensión lectora en textos académicos.\n• Redactar textos académicos con coherencia y cohesión.\n• Utilizar fuentes bibliográficas confiables y citar correctamente.",
                        ProgramacionContenidos = "UNIDAD I: Gestión del Aprendizaje\n• El estudiante universitario y su entorno.\n• Planificación del tiempo y metas académicas.\n• Técnicas de concentración y memoria.\n\nUNIDAD II: Comprensión Lectora\n• Tipos de textos académicos.\n• Estrategias de lectura: skimming, scanning.\n• Análisis crítico de textos.\n\nUNIDAD III: Producción Escrita Académica\n• Estructura del texto académico.\n• Coherencia, cohesión y corrección.\n• El resumen, el ensayo y el informe.\n\nUNIDAD IV: Investigación y Fuentes\n• Búsqueda en bases de datos académicas.\n• Citas y referencias (norma APA).\n• Introducción al trabajo de investigación.",
                        EstrategiasDiddacticas = "• Talleres prácticos de escritura.\n• Lecturas comentadas en clase.\n• Portafolio de evidencias.\n• Tutorías individuales.",
                        SistemaEvaluacion = "• Portafolio de trabajos (40%).\n• Exposición y debate (20%).\n• Examen de comprensión lectora (40%).",
                        Bibliografia = "• Ander-Egg, Ezequiel. Técnicas de Investigación Social.\n• Carlino, Paula. Escribir, leer y aprender en la universidad.\n• APA. Manual de Publicaciones (7ª edición).",
                        Estado = "En edición",
                        CreadoPor = p2.Email,
                        ModificadoPor = p2.Email,
                        FechaCreacion = DateTime.Now.AddDays(-9),
                        FechaModificacion = DateTime.Now.AddDays(-1)
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
