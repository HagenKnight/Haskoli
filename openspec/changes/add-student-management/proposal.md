## Why

El PRD de Háskóli (`docs/PRD.md`, Épica 1) define la gestión de estudiantes como la base sobre la que se apoyan el resto de las épicas: sin un estudiante persistido no hay programa de créditos, ni inscripción de materias, ni consulta de compañeros de clase. Hoy la solución solo contiene la entidad de ejemplo `Country`, cuyo CRUD de escritura está declarado pero sin implementar (`CreateCountryCommandHandler`, `UpdateCountryCommandHandler` y `DeleteCountryCommandHandler` lanzan `NotImplementedException`) y cuyo controlador solo expone las dos operaciones de lectura.

Esto significa que la Épica 1 no es solo "otra entidad más": es la primera implementación end-to-end de escritura del proyecto y, por lo tanto, la que fija el patrón de referencia (validación de unicidad, auditoría, borrado lógico, paginación y filtros) que las épicas siguientes copiarán.

## What Changes

**Nueva entidad y vertical slice completo de `Student`**

- Entidad `Student` en `Haskoli.Domain`, heredando de `EntityBase<int>` para reutilizar los campos de auditoría, con los campos del PRD: `Documento`, `Nombre`, `Apellido`, `Email`.
- DTOs `StudentDTO`, `CreateStudentDTO`, `UpdateStudentDTO`, `DeleteStudentDTO` siguiendo la convención documentada en `docs/README.md`.
- `IStudentRepository` / `StudentRepository` e `IStudentService` / `StudentService`, este último sobre el `CRUDService` genérico existente.
- Features CQRS en `Haskoli.Application/Features/Student`: comandos de crear, actualizar y eliminar con su validador FluentValidation, y queries de listado paginado y de detalle por `Id`.
- `StudentController` en `Haskoli.Api` exponiendo las cinco operaciones (crear, listar paginado con filtros, consultar por `Id`, actualizar, eliminar).
- Configuración EF Core `StudentConfiguration` con índices únicos sobre `Documento` y `Email`, `DbSet<Student>` en `HaskoliDbContext` y su migración.

**Unicidad de negocio (HU01-CA02/CA04, HU04-CA04/CA05)**

- Validación de documento y email únicos, tolerante a la actualización del propio registro (reenviar el mismo valor es válido), reportada con los mensajes de error exactos del PRD.

**Exclusión de eliminados mediante filtro global (HU02-CA01, HU03, HU05-CA02/CA06)**

- Se introduce un filtro de consulta global de EF Core sobre `IsDeleted` para `Student`, de modo que los registros eliminados desaparecen de todas las consultas sin depender de que cada query lo recuerde. Hoy `CRUDService.FindAsync` valida `IsDeleted` a mano y `GetAllAsync` no lo filtra en absoluto.

**Completar la auditoría de eliminación (HU05-CA04)**

- `CRUDService.DeleteAsync` ya asigna `IsDeleted` y `DeleteDate`, pero **no** asigna `DeletedBy`. Se completa el rastro de auditoría de borrado con el valor `system` que usa el resto de la infraestructura (`HaskoliDbContext.SaveChangesAsync`).

**Listado paginado con filtros (HU02-CA01/CA02)**

- Listado paginado con filtros opcionales por documento, apellido y email, devuelto en el envoltorio `ApiResponse<MetaData<StudentDTO>>` ya existente. Se corrige el comportamiento de `GetPagedAsync` frente a un conjunto de resultados vacío, que hoy lanza `PageRowIndexNotFound` en la primera página cuando no hay registros.

**Suite de pruebas de aceptación automatizadas**

- Se puebla el proyecto `tests/Haskoli.NTest`, que existe y está registrado en la solución pero no contiene ninguna prueba, con una suite que cubre cada escenario de la especificación: validadores, handlers de MediatR y comportamiento de persistencia (auditoría, borrado lógico, filtro global e índices únicos), incluyendo los casos límite de cada historia de usuario.
- Las pruebas de persistencia corren contra SQL Server, sobre la misma base que usa el desarrollo, vaciando la tabla `Student` entre pruebas. Así se verifican con fidelidad los mecanismos que solo el motor real reproduce: los índices únicos, el filtro global y, sobre todo, la collation `Modern_Spanish_CI_AS` de la que dependen los escenarios de búsqueda.
- Se añaden al proyecto de pruebas las referencias a los proyectos de la solución y `NSubstitute` para los dobles de prueba.

**Fuera de alcance de este cambio**

- Interfaz de usuario. Los criterios HU02-CA01 (presentación paginada), HU04-CA07 y HU05-CA07 (confirmación al usuario) se especifican como contrato de la API que la interfaz consumirá; su implementación visual queda para un cambio posterior.
- Borrado lógico en cascada de las materias del estudiante (HU05-CA05). La entidad de inscripción pertenece a una épica posterior y todavía no existe; el requisito queda registrado como pendiente y se resolverá cuando esa entidad se cree.

## Capabilities

### New Capabilities

- `student-management`: gestión del ciclo de vida de los estudiantes del programa académico — registro, consulta individual, listado paginado con filtros, actualización y eliminación lógica, incluyendo las reglas de unicidad de documento y email y el rastro de auditoría asociado.

### Modified Capabilities

Ninguna. Los requisitos de `platform-runtime` (framework objetivo, EF Core como único ORM, alineación de versiones de paquetes e higiene de dependencias) se respetan sin cambios: no se agregan dependencias nuevas y todo el trabajo ocurre en proyectos que ya apuntan a `net10.0`.

## Impact

**Código nuevo**

- `src/Haskoli.Domain`: entidad `Student`, DTOs, `IStudentRepository`, `IStudentService`.
- `src/Haskoli.Application`: carpeta `Features/Student` con comandos, queries, handlers y validadores; nuevos mapeos en `Mappings/AutoMapperProfile.cs`.
- `src/Haskoli.Infrastructure.Common`: `StudentRepository`, `StudentService` y su registro en `ServiceCollection/ServiceCollection.cs`.
- `src/Haskoli.Infrastructure.Persistence`: `StudentConfiguration`, `DbSet<Student>` en `HaskoliDbContext` y una migración nueva.
- `src/Haskoli.Api`: `StudentController`.

**Infraestructura compartida que se modifica**

- `HaskoliDbContext`: registro de la nueva configuración, `DbSet` y filtro global de `IsDeleted`.
- `CRUDService.DeleteAsync`: asignación de `DeletedBy`, con efecto sobre cualquier entidad futura que use el servicio genérico (`Country` incluida, aunque hoy no ejerza esa ruta).
- `CRUDService.GetPagedAsync`: manejo del caso sin resultados. Afecta al comportamiento de paginación de todas las entidades.
- `CountryConfiguration`: la fecha del seed pasa de `DateTime.Now` a una constante. El valor dinámico hacía que el modelo cambiara en cada compilación, y EF Core rechazaba aplicar cualquier migración por considerarlo no determinista.
- `HaskoliDbContextFactory`: carga también `appsettings.{entorno}.json`. Como `appsettings.json` viaja con las cadenas en blanco por ser plantilla, las herramientas de EF se quedaban sin cadena de conexión.
- `AddApplicationLayer`: registro de `ConverterPaging<,>`. AutoMapper resuelve los conversores del contenedor y no registra por sí solo los genéricos abiertos, así que el mapeo de `PagedList` a `MetaData` fallaba con un `500`. El defecto estaba latente porque la paginación de `Country` está comentada y nunca lo había ejercido.

**Proyecto de pruebas**

- `tests/Haskoli.NTest`: pasa de estar vacío a contener la suite de aceptación, con referencias a `Haskoli.Domain`, `Haskoli.Application`, `Haskoli.Infrastructure.Common` y `Haskoli.Infrastructure.Persistence`.

**Base de datos**

- Nueva tabla `Student` con índices únicos en `Documento` y `Email`. Requiere aplicar la migración con `dotnet ef database update --context HaskoliDbContext`.

**Dependencias**

- En los proyectos de producción, ninguna nueva: se reutilizan MediatR, AutoMapper, FluentValidation y EF Core ya presentes.
- En el proyecto de pruebas, solo `NSubstitute`: el proveedor SQL Server llega de forma transitiva por `Haskoli.Infrastructure.Persistence`. El proyecto ya cuenta con NUnit, `Microsoft.NET.Test.Sdk` y `coverlet.collector`. Las aserciones usan el modelo de restricciones nativo de NUnit, sin sumar una librería adicional.

**Pruebas**

- Los escenarios de la especificación se verifican de forma automatizada en `tests/Haskoli.NTest`. La suite requiere una instancia de SQL Server accesible: es el precio de verificar de verdad la collation y los índices únicos, en lugar de simularlos sobre un motor que se comporta distinto.
