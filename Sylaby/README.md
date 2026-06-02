# Sylaby – Gestión de Sílabos y Acreditación Académica

## Descripción del proyecto

Sylaby es una aplicación **ASP.NET Core MVC** diseñada para la creación, revisión y aprobación de sílabos universitarios.  La aplicación incluye:

- **Autenticación** con registro y login, validación de dominio `@usmp.pe` y almacenamiento seguro de contraseñas.
- **Roles**: `Docente` y `Director` con accesos diferenciados.
- **Módulo de Cierre de Ciclo** – los docentes pueden registrar el nivel de cumplimiento de los sílabos al final del periodo académico.
- **Base de datos** SQLite configurada con `DbInitializer` y seed de usuarios y cursos.
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
│   └─ CierreCicloController.cs   # Lógica para docentes y directores
├─ Models/
│   ├─ DbInitializer.cs           # Seed de usuarios y cursos
│   └─ ...                        # Entidades del dominio
├─ Views/
│   ├─ CierreCiclo/               # Vistas del módulo de cierre de ciclo
│   └─ Shared/_Layout.cshtml      # Layout principal (footer eliminado)
├─ wwwroot/css/site.css           # Estilos personalizados y ajustes UI
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
6. **README creado** – se mantendrá actualizado cada vez que se concluya una tarea.

---

*Este documento se actualizará automáticamente al completar nuevas tareas o al realizar cambios significativos en la arquitectura del proyecto.*
