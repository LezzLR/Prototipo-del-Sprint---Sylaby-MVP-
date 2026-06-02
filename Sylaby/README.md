# Sylaby – Gestión de Sílabos y Acreditación Académica

## Descripción del proyecto

Sylaby es una aplicación **ASP.NET Core MVC** diseñada para la creación, revisión y aprobación de sílabos universitarios.  La aplicación incluye:

- **Autenticación** con registro y login, validación de dominio `@usmp.pe` y almacenamiento seguro de contraseñas.
- **Roles**: `Docente` y `Director` con accesos diferenciados.
- **Módulo de Cierre de Ciclo** – los docentes pueden registrar el nivel de cumplimiento de los sílabos al final del periodo académico.
- **Módulo de Gestión de Sílabos** – permite a los docentes proponer mejoras, editar el contenido del sílabo estructurado en secciones/pestañas y enviarlo a revisión. Los directores pueden filtrar propuestas, revisar contenidos, añadir observaciones específicas por sección, y aprobar o rechazar la propuesta. Se incluye además una bitácora de auditoría histórica.
- **Base de datos** SQLite configurada con `DbInitializer` y seed de usuarios, cursos y sílabos preconfigurados.
- **Interfaz premium** con colores institucionales (`#B22222`), tipografía Inter, micro‑animaciones y layout responsive.
- **Mejoras de UI**: ajustes de padding/margin para evitar solapamiento del contenido con el footer y eliminación del footer que interfería con la vista de la encuesta.

## Requisitos

- .NET 8 SDK (o superior)
- Windows (el proyecto está configurado para ejecutarse en Windows)

## Cómo ejecutar la aplicación

```bash
# Desde la carpeta del proyecto
cd "C:\Users\PC\OneDrive\Escritorio\PMV - Ing Software\Sylaby"
# Restaurar paquetes
dotnet restore
# Ejecutar aplicación en modo desarrollo
dotnet run
```

La aplicación se iniciará en `https://localhost:5001` (o el puerto indicado en la consola).

## Estructura de carpetas relevante

```
Sylaby/
├─ Controllers/
│   ├─ CierreCicloController.cs   # Lógica para encuestas de cierre de ciclo
│   └─ SilabosController.cs       # Lógica del flujo de edición y revisión de sílabos
├─ Models/
│   ├─ DbInitializer.cs           # Seed de usuarios, cursos y sílabos de prueba
│   ├─ Silabo.cs                  # Modelo de datos del Sílabo académico
│   ├─ PropuestaMejora.cs         # Propuestas registradas por el docente
│   ├─ ObservacionDirector.cs     # Observaciones del Director por sección
│   ├─ BitacoraAccion.cs          # Log de auditoría de acciones realizadas
│   └─ SylabyDbContext.cs         # Contexto de base de datos y relaciones EF Core
├─ Views/
│   ├─ CierreCiclo/               # Vistas del módulo de cierre de ciclo
│   ├─ Silabos/                   # Vistas del módulo de gestión de sílabos (Docente y Director)
│   └─ Shared/_Layout.cshtml      # Layout principal (enlaces añadidos en barra de navegación)
├─ wwwroot/css/site.css           # Estilos personalizados y ajustes UI (estilos del módulo de sílabos)
└─ README.md                      # **Este archivo** – documentación del proyecto
```

## Registro y roles

- **Registro**: el formulario solicita email y password. Sólo se aceptan correos terminados en `@usmp.pe`. La contraseña se guarda con hash (ASP.NET Identity).
- **Login**: verifica credenciales contra la base SQLite y redirige al **Dashboard**.
- **Roles**: al crearse un usuario se asigna el rol `Docente` por defecto; el administrador puede asignar `Director`.

## Últimos cambios (Resumen de tareas completadas)

1. Implementación completa de autenticación y roles.
2. Creación del módulo **Cierre de Ciclo** con vistas y lógica de asignación de cursos.
3. Corrección de superposición del footer y estilos de UI (padding, margin, eliminación del footer).
4. Actualización de **site.css** para espaciado y diseño premium.
5. Despliegue de la aplicación con SQLite y seed de datos.
6. **README creado y actualizado** – se mantiene al día con cada tarea concluida.
7. **Módulo de Gestión de Sílabos completo**:
   - Modelado de base de datos con relaciones y control de estados (En edición, En revisión, Por corregir, Aprobado).
   - Interfaz interactiva de edición por pestañas para docentes con guardado de borrador y validación estricta de envío.
   - Formulario de Propuestas de Mejora con justificación requerida para el docente.
   - Interfaz de revisión para el Director con acordeón de observaciones específicas por cada sección, aprobación/rechazo y control de reapertura de edición.
   - Bitácora de acciones que registra el log histórico de todas las interacciones realizadas en cada sílabo.
   - Siembra de datos ampliada con sílabos de prueba completos para los 4 cursos asignados a Profesor 1 y Profesor 2.

---

*Este documento se actualizará automáticamente al completar nuevas tareas o al realizar cambios significativos en la arquitectura del proyecto.*

