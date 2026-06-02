# Sylaby – Gestión de Sílabos y Acreditación Académica

## Descripción del proyecto

Sylaby es una aplicación **ASP.NET Core MVC** diseñada para la creación, revisión y aprobación institucional de sílabos universitarios. La aplicación incluye:

- **Autenticación** con registro y login, validación de dominio `@usmp.pe` y almacenamiento seguro de contraseñas.
- **Roles**: `Docente`, `Director` y `RevisorAcademico` con accesos diferenciados.
- **Módulo de Cierre de Ciclo** – los docentes registran el nivel de cumplimiento de los sílabos al final del periodo académico.
- **Módulo de Gestión de Sílabos** – flujo de tres etapas: Docente → Director → Departamento Académico.
- **Base de datos** SQLite configurada con `DbInitializer` y seed de usuarios, cursos y sílabos preconfigurados.
- **Interfaz premium** con colores institucionales (`#B22222`), tipografía Inter, micro‑animaciones y layout responsive.

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

## Usuarios de prueba

| Correo | Contraseña | Rol |
|---|---|---|
| `director@usmp.pe` | `1234` | Director |
| `depaca@usmp.pe` | `1234` | RevisorAcademico |
| `profesor1@usmp.pe` | `1234` | Docente |
| `profesor2@usmp.pe` | `1234` | Docente |

## Flujo de estados del sílabo

```
Docente edita           → "En edición"
Docente envía           → "En revisión"
Director rechaza        → "Por corregir"              (Docente puede editar y reenviar)
Director aprueba        → "Aprobado por Director"
Revisor abre            → "En validación académica"
Revisor observa         → "Observado"                 (Director puede reabrir edición al Docente)
Revisor aprueba final   → "Aprobado Final"            (Lectura. El Revisor puede reabrir edición masiva o individualmente, volviendo a "En edición")
```

## Estructura de carpetas relevante

```
Sylaby/
├─ Controllers/
│   ├─ CierreCicloController.cs       # Lógica para encuestas de cierre de ciclo
│   └─ SilabosController.cs           # Flujo completo: Docente, Director, RevisorAcademico
├─ Models/
│   ├─ DbInitializer.cs               # Seed de 4 usuarios, cursos y sílabos de prueba
│   ├─ Silabo.cs                      # Modelo del Sílabo (estados + campos de validación)
│   ├─ ValidacionAcademica.cs         # Checklist por sección del Revisor Académico
│   ├─ PropuestaMejora.cs             # Propuestas registradas por el docente
│   ├─ ObservacionDirector.cs         # Observaciones del Director por sección
│   ├─ BitacoraAccion.cs              # Log de auditoría histórico
│   └─ SylabyDbContext.cs             # Contexto de BD y relaciones EF Core
├─ Views/
│   ├─ CierreCiclo/                   # Vistas del módulo de cierre de ciclo
│   ├─ Silabos/
│   │   ├─ DocenteIndex.cshtml        # Tarjetas de cursos del docente con estado
│   │   ├─ Editor.cshtml              # Editor de sílabo por pestañas (docente)
│   │   ├─ DirectorIndex.cshtml       # Lista de sílabos con filtros (director)
│   │   ├─ Detalle.cshtml             # Revisión/aprobación del sílabo (director)
│   │   ├─ ValidacionIndex.cshtml     # Lista de sílabos para revisar (Dpto. Académico)
│   │   └─ DetalleValidacion.cshtml   # Checklist de validación institucional
│   └─ Shared/_Layout.cshtml          # Layout con menús diferenciados por rol
├─ wwwroot/css/site.css               # Estilos personalizados y tokens de diseño
└─ README.md                          # Este archivo – documentación del proyecto
```

## Últimos cambios (Resumen de tareas completadas)

1. Implementación completa de autenticación y roles.
2. Creación del módulo **Cierre de Ciclo** con vistas y lógica de asignación de cursos.
3. Corrección de superposición del footer y estilos de UI.
4. Actualización de **site.css** para espaciado y diseño premium.
5. Despliegue de la aplicación con SQLite y seed de datos.
6. **README creado y actualizado** – se mantiene al día con cada tarea concluida.
7. **Módulo de Gestión de Sílabos (Docente → Director)**:
   - Modelado de base de datos con relaciones y control de estados.
   - Interfaz interactiva de edición por pestañas para docentes.
   - Formulario de Propuestas de Mejora con justificación requerida.
   - Interfaz de revisión para el Director con acordeón de observaciones.
   - Bitácora de acciones con log histórico completo.
   - Auto-limpieza de observaciones del Director al modificar secciones.
   - Reapertura de edición desde el Director (vuelve a "En edición").
8. **Módulo de Validación Académica (Departamento Académico)**:
   - Nuevo rol `RevisorAcademico` con usuario `depaca@usmp.pe / 1234`.
   - Nuevo modelo `ValidacionAcademica` con checklist Conforme/No Conforme por sección.
   - Flujo extendido: "Aprobado por Director" → "En validación académica" → "Observado" / "Aprobado Final".
   - Vista `ValidacionIndex` con filtros por curso, docente y estado.
   - Vista `DetalleValidacion` con checklist interactivo; comentarios obligatorios para "No conforme".
   - Historial acumulativo de validaciones por sílabo.
   - "Aprobado Final" bloquea el sílabo en modo solo lectura, pero otorga al Revisor la opción de **reabrir la edición** (de forma individual o masiva), reiniciando el flujo al estado "En edición".
   - Director puede reabrir edición desde "Aprobado por Director" u "Observado".

---

*Este documento se actualiza automáticamente al completar nuevas tareas o al realizar cambios significativos en la arquitectura del proyecto.*
