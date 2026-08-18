## 1. Dominio

- [x] 1.1 Crear `src/Haskoli.Domain/Entities/Student.cs` con `Student : EntityBase<int>` y las propiedades `Document`, `FirstName`, `LastName`, `Email`, con `[StringLength]` en cada una (documento 20, nombre 100, apellido 100, email 150)
- [x] 1.2 Crear `src/Haskoli.Domain/DTO/Student/StudentDTO.cs` con `Id`, `Document`, `FirstName`, `LastName`, `Email` y sin ningún campo de auditoría
- [x] 1.3 Crear `src/Haskoli.Domain/DTO/Student/CreateStudentDTO.cs` heredando de `CommandDTO` e implementando `IRequest<ApiResponse<StudentDTO>>`, con los cuatro campos editables
- [x] 1.4 Crear `src/Haskoli.Domain/DTO/Student/UpdateStudentDTO.cs` heredando de `CommandDTO` e implementando `IRequest<ApiResponse<StudentDTO>>`, con los cuatro campos editables
- [x] 1.5 Crear `src/Haskoli.Domain/DTO/Student/DeleteStudentDTO.cs` heredando de `CommandDTO` e implementando `IRequest<ApiResponse<StudentDTO>>`
- [x] 1.6 Crear `src/Haskoli.Domain/Interfaces/Repository/IStudentRepository.cs` extendiendo `IBaseRepository<Student, TContext>` y declarando `ExistsByDocumentAsync(string document, int? excludeId, CancellationToken)` y `ExistsByEmailAsync(string email, int? excludeId, CancellationToken)`
- [x] 1.7 Crear `src/Haskoli.Domain/Interfaces/Services/IStudentService.cs` con `RowCount`, el listado paginado con predicado, `FindStudent(int id, ...)`, y las operaciones de creación, actualización y eliminación
- [x] 1.8 Crear `src/Haskoli.Domain/Parameters/GetAllStudentParameter.cs` heredando de `RequestParameter`, con `Document`, `LastName` y `Email` opcionales, sin recortar `PageSize`: el rango 10–50 lo hace cumplir `CRUDService.GetPagedAsync` rechazando los valores fuera de rango, tal como exige la especificación, en lugar de heredar el truncamiento a 10 del constructor con parámetros de `RequestParameter`
- [x] 1.9 Compilar `Haskoli.Domain` y confirmar que no aparecen advertencias nuevas atribuibles a los archivos creados

## 2. Persistencia y migración

- [x] 2.1 Crear `src/Haskoli.Infrastructure.Persistence/Data/Configurations/StudentConfiguration.cs` con `ToTable("Student")`, índice único sobre `Document`, índice único sobre `Email` y `HasQueryFilter(s => !s.IsDeleted)`
- [x] 2.2 Agregar `public DbSet<Student> Students { get; set; }` a `HaskoliDbContext` y registrar `modelBuilder.ApplyConfiguration(new StudentConfiguration())` en `OnModelCreating`
- [x] 2.3 Generar la migración. No existía ninguna migración previa para `HaskoliDbContext`, así que la generada es la inicial y crea `Country` y `Student`: se nombra `InitialCreate` en lugar de `AddStudent` para no mentir sobre su alcance
- [x] 2.4 Revisar el archivo de migración generado: crea `Student` con `IX_Student_Document` e `IX_Student_Email` como `CREATE UNIQUE INDEX`, y crea `Country` con su seed por ser la migración inicial. **La collation no viaja en el SQL**: EF Core no emite `ALTER DATABASE ... COLLATE` en una migración inicial, aunque el modelo sí la conserva (el snapshot y el Designer llevan `UseCollation("Modern_Spanish_CI_AS")`, lo que confirma que el condicional de la tarea 3.4 funciona bajo SQL Server). Se resuelve creando la base explícitamente con esa collation, no parcheando la migración
- [x] 2.5 Preparar la base y aplicar la migración:
  - [x] 2.5.1 Hacer que `HaskoliDbContextFactory` cargue también `appsettings.{entorno}.json`: `appsettings.json` viaja con las cadenas en blanco por ser plantilla, de modo que las herramientas de EF se quedaban sin cadena de conexión
  - [x] 2.5.2 Sustituir `CreatedDate = DateTime.Now` por una constante estática en las 244 filas del seed de `CountryConfiguration`: el valor dinámico hacía que el modelo cambiara en cada compilación y EF rechazaba aplicar migraciones con `PendingModelChangesWarning`
  - [x] 2.5.3 Crear la base con `CREATE DATABASE [Haskoli] COLLATE Modern_Spanish_CI_AS`, para que las columnas de texto hereden la collation que exigen los escenarios de filtro
  - [x] 2.5.4 Aplicar la migración con `dotnet ef database update` y verificar en la base: `Student` con sus dos índices únicos, columnas de texto con collation `Modern_Spanish_CI_AS` y las 244 filas de `Country` con fecha fija

## 3. Correcciones en la infraestructura compartida

- [x] 3.1 En `CRUDService.DeleteAsync` (`src/Haskoli.Infrastructure.Common/Services/Base/CRUDService.cs`), asignar `DeletedBy = "system"` junto con `IsDeleted` y `DeleteDate`
- [x] 3.2 En ambas sobrecargas de `CRUDService.GetPagedAsync`, corregir la guarda de `pageNumber` para que un conjunto de resultados vacío devuelva una colección vacía en la página 1 en lugar de lanzar `PageRowIndexNotFound`, y validar el tamaño de página antes que el índice para no dividir por un tamaño inválido
- [x] 3.3 ~~Abrir el constructor de `HaskoliDbContext` a proveedores distintos de SQL Server~~ **Descartada y revertida.** Existía solo para permitir SQLite en pruebas; al verificar la persistencia contra SQL Server real deja de tener motivo, y el contexto queda tal como estaba
- [x] 3.4 ~~Condicionar `modelBuilder.UseCollation(Collation)` al proveedor SQL Server~~ **Descartada y revertida** por el mismo motivo que 3.3: sin SQLite no hay proveedor que desconozca la collation
- [x] 3.5 Compilar la solución completa con `dotnet build Haskoli.sln` y confirmar que los cambios en el servicio genérico y en el contexto no rompen `Country`
- [x] 3.6 Confirmar por inspección que el rechazo de `"Database": "mysql"` sigue intacto en los tres puntos que lo declaran (`AddPersistenceLayer`, `HaskoliDbContextFactory` e `IdentityServiceRegistration`)
- [x] 3.7 Confirmado al levantar la API: con `"Database": "mysql"` el arranque falla con `NotSupportedException` y el mensaje que remite a `mssql`, lanzado desde `IdentityServiceRegistration`

## 4. Repositorio y servicio

- [x] 4.1 Crear `src/Haskoli.Infrastructure.Common/Repositories/StudentRepository.cs` heredando de `BaseRepository<Student, int, HaskoliDbContext>`
- [x] 4.2 Implementar `ExistsByDocumentAsync` y `ExistsByEmailAsync` en `StudentRepository` usando `DbContext.Set<Student>().IgnoreQueryFilters()`, excluyendo el propio `Id` cuando se recibe `excludeId`, para que la unicidad alcance también a los estudiantes eliminados
- [x] 4.3 Crear `src/Haskoli.Infrastructure.Common/Services/StudentService.cs` heredando de `CRUDService<Student, StudentDTO, CommandDTO, int, Student, IStudentRepository<HaskoliDbContext>, HaskoliDbContext>` e implementando `IStudentService`
- [x] 4.4 Exponer en `StudentService` los métodos de consulta del dominio delegando en el servicio genérico, y los de verificación de unicidad delegando en el repositorio. `GetPagedStudents` elige la sobrecarga sin predicado cuando no hay filtros, y pasa cadena vacía en lugar de `null` a `fields` y `orderBy` porque el servicio genérico no los declara nulables y ambos se comprueban con `IsNullOrEmpty`
- [x] 4.5 Registrar `IStudentRepository<HaskoliDbContext>` → `StudentRepository` e `IStudentService` → `StudentService` en `AddCommonLayer` (`src/Haskoli.Infrastructure.Common/ServiceCollection/ServiceCollection.cs`)

## 5. Mapeos y mensajes

- [x] 5.1 Agregar en `src/Haskoli.Application/Mappings/AutoMapperProfile.cs` los mapeos entre `Student` y `StudentDTO`, `CreateStudentDTO`, `UpdateStudentDTO` y `DeleteStudentDTO`, siguiendo el patrón de `Country`
- [x] 5.2 Configurar el mapeo de creación para que ignore `Id`, de modo que un `id` enviado por el cliente no llegue a la columna identidad. El mapa de creación se declara sin `ReverseMap` para poder ignorar `Id` solo en el sentido DTO → entidad
- [x] 5.3 Definir en `src/Haskoli.Application/Constants/StudentMessages.cs` los mensajes literales del PRD: documento duplicado, email duplicado e inexistencia por `Id`, más los tres de confirmación. El texto de email duplicado que la especificación fija para la actualización se reutiliza al registrar, donde solo se exige rechazar la operación

## 6. Features de MediatR

Tres restricciones descubiertas al preparar los mensajes, que condicionan cómo deben escribirse los handlers:

- `CRUDService.FindAsync` lanza `EntityNotFoundException` con un mensaje en inglés que incluye el nombre del tipo, no el que fija el PRD. Los handlers SHALL verificar la existencia por su cuenta y lanzar la excepción con `StudentMessages.NotFound`.
- `CRUDService.UpdateAsync` envuelve su cuerpo en un `try/catch` que relanza **toda** excepción como `MappingNotFoundException`, incluida la de inexistencia que él mismo produce. Como el middleware no contempla ese tipo, acabaría en `500` donde la especificación exige `404`. Por eso la verificación de existencia debe ocurrir en el handler, antes de delegar.
- El middleware mapea `BusinessException` a `406`. Para los duplicados, que la especificación fija en `400`, corresponde `EntityDuplicatedException` o `EntityAlreadyExistException`.

- [x] 6.1 Crear `src/Haskoli.Application/Features/Student/Commands/CreateStudent/CreateStudentCommandValidator.cs` exigiendo documento, nombre, apellido y email no vacíos y validando el formato del email con un patrón que rechace `usuario@` y `usuario`. Las reglas se extraen a `StudentRules` como métodos de extensión, porque la especificación exige exactamente las mismas en creación y actualización y así no pueden divergir. El formato del email se impone con un patrón propio: el validador de FluentValidation acepta `usuario@dominio`, que la especificación rechaza por no tener punto ni sufijo
- [x] 6.2 Crear `CreateStudentCommandHandler` que verifique unicidad de documento y de email, lance `EntityAlreadyExistException` con el mensaje correspondiente si hay colisión, y en caso contrario inserte vía `IStudentService` y devuelva `ApiResponse<StudentDTO>` con mensaje de confirmación
- [x] 6.3 Crear `src/Haskoli.Application/Features/Student/Commands/UpdateStudent/UpdateStudentCommandValidator.cs` con las mismas reglas de obligatoriedad y formato, más `Id` mayor que cero
- [x] 6.4 Crear `UpdateStudentCommandHandler` que verifique la existencia del estudiante, valide unicidad de documento y email excluyendo el propio `Id` (para que reenviar los mismos valores sea válido) y devuelva el estudiante actualizado con mensaje de confirmación
- [x] 6.5 Crear `src/Haskoli.Application/Features/Student/Commands/DeleteStudent/DeleteStudentCommandValidator.cs` exigiendo `Id` mayor que cero
- [x] 6.6 Crear `DeleteStudentCommandHandler` que ejecute el borrado lógico vía `IStudentService` y devuelva confirmación, apoyándose en el filtro global para que un estudiante ya eliminado produzca `EntityNotFoundException`
- [x] 6.7 Crear `src/Haskoli.Application/Features/Student/Queries/StudentQuery.cs` con `GetAllStudentQuery` (página, tamaño, orden, ruta y los tres filtros) y `GetStudentQuery(int id)`
- [x] 6.8 Crear `GetStudentHandler` que devuelva el detalle por `Id` delegando en `FindStudent`
- [x] 6.9 Crear `GetAllStudentHandler` que componga el predicado con los filtros presentes, obtenga la página vía `GetPagedAsync` con predicado y devuelva `ApiResponse<MetaData<StudentDTO>>` construyendo `PagedList<StudentDTO>` con `IUriService` y la ruta recibida. Cada filtro ausente se neutraliza comparándolo contra `null` dentro de la propia expresión, en lugar de componer árboles de expresión con un visitante: el resultado es el mismo y la consulta queda legible. Sin ningún filtro devuelve `null`, para que el servicio use la sobrecarga sin predicado
- [x] 6.10 Agregar el mapeo de `GetAllStudentQuery` a `GetAllStudentParameter` en el perfil de AutoMapper
- [x] 6.11 Crear `StudentLookup.FindOrThrowAsync`, usado por los handlers de detalle, actualización y eliminación, que traduce la excepción de inexistencia del servicio genérico al mensaje literal del PRD

## 7. API

- [x] 7.1 Crear `src/Haskoli.Api/Controllers/StudentController.cs` con `[Route("api/[controller]")]` e inyección de `IMediator`, siguiendo la forma de `CountryController`
- [x] 7.2 Implementar `POST api/Student` que envíe `CreateStudentDTO` y devuelva `ApiResponse<StudentDTO>`
- [x] 7.3 Implementar `GET api/Student` que reciba `GetAllStudentParameter` con `[FromQuery]`, propague `Request.Path.Value` como ruta y devuelva `ApiResponse<MetaData<StudentDTO>>`
- [x] 7.4 Implementar `GET api/Student/{id}` que devuelva el detalle del estudiante
- [x] 7.5 Implementar `PUT api/Student` que envíe `UpdateStudentDTO` y devuelva el estudiante actualizado
- [x] 7.6 Implementar `DELETE api/Student/{id}` que envíe `DeleteStudentDTO` y devuelva la confirmación
- [x] 7.7 Compilar la solución y levantar la API, confirmando que los cinco endpoints aparecen en Swagger
- [x] 7.8 Registrar `ConverterPaging<,>` en `AddApplicationLayer`. AutoMapper resuelve los conversores del contenedor y no registra por sí solo los genéricos abiertos, de modo que el mapeo de `PagedList` a `MetaData` fallaba en tiempo de ejecución con un `500`. El defecto estaba latente: la paginación de `Country` está comentada y nunca había ejercido esa ruta

## 8. Preparación del proyecto de pruebas

- [ ] 8.1 Agregar en `tests/Haskoli.NTest/Haskoli.NTest.csproj` las referencias a `Haskoli.Domain`, `Haskoli.Application`, `Haskoli.Infrastructure.Common` y `Haskoli.Infrastructure.Persistence`
- [ ] 8.2 Agregar el paquete `NSubstitute` para los dobles de prueba. El proveedor SQL Server ya llega de forma transitiva por `Haskoli.Infrastructure.Persistence`, así que no se suma ningún proveedor adicional
- [ ] 8.3 Crear `tests/Haskoli.NTest/Students/Builders/StudentBuilder.cs`, un constructor de datos de prueba con valores por defecto válidos y sobrescritura selectiva por campo, para que cada prueba declare solo el dato que le importa
- [ ] 8.4 Crear `tests/Haskoli.NTest/appsettings.json` con la cadena de conexión de la base de pruebas, copiada al directorio de salida, para no incrustar credenciales en el código de las pruebas
- [ ] 8.5 Crear `tests/Haskoli.NTest/Students/SqlServerDbContextFixture.cs`, que construya un `HaskoliDbContext` con `UseSqlServer` sobre la cadena configurada y, antes y después de cada prueba, vacíe **únicamente** la tabla `Student` con `DELETE FROM Student` y reinicie su columna identidad, dejando intacto el seed de `Country`
- [ ] 8.6 Verificar con una prueba mínima que el contexto se conecta, que la tabla `Student` existe y que la limpieza la deja vacía
- [ ] 8.7 Ejecutar `dotnet test` y confirmar que el proyecto se descubre y ejecuta

## 9. Pruebas de validadores

- [ ] 9.1 Crear `CreateStudentCommandValidatorTests` agrupando por criterio, con la clase externa nombrando HU01 y `[Category]` para trazabilidad
- [ ] 9.2 Cubrir obligatoriedad de documento, nombre, apellido y email con `[TestCase]` sobre los casos límite de ausencia: nulo, cadena vacía, un solo espacio, varios espacios y tabulador
- [ ] 9.3 Cubrir la aceptación de emails válidos: `usuario@dominio.com`, con subdominio y con sufijo de dos niveles
- [ ] 9.4 Cubrir el rechazo de emails inválidos como casos límite: `usuario@`, `usuario`, `@dominio.com`, `usuario@dominio` sin punto, `usuario@@dominio.com` y valores con espacios intercalados
- [ ] 9.5 Cubrir las fronteras de longitud de cada campo: longitud máxima exacta aceptada y máxima más uno rechazada
- [ ] 9.6 Crear `UpdateStudentCommandValidatorTests` replicando las reglas anteriores y añadiendo las fronteras de `Id`: cero y negativo rechazados, uno aceptado
- [ ] 9.7 Verificar que un comando con los cuatro campos válidos no produce ningún error de validación

## 10. Pruebas de handlers

- [ ] 10.1 Crear `CreateStudentCommandHandlerTests` con `IStudentService` e `IStudentRepository` sustituidos, agrupando por contexto: registro válido, documento en conflicto y email en conflicto
- [ ] 10.2 Verificar que el registro válido devuelve `succeeded` en verdadero, el estudiante con su `Id` y un mensaje de confirmación
- [ ] 10.3 Verificar que un documento ya existente produce la excepción de negocio con el mensaje literal `Ya existe un estudiante registrado con el documento 123456789.` y que no se invoca la inserción
- [ ] 10.4 Verificar que un email ya existente rechaza el registro sin insertar
- [ ] 10.5 Cubrir los casos límite de la creación: documento perteneciente a un estudiante eliminado sigue ocupado, `id` enviado por el cliente se ignora, y conflicto simultáneo de documento y email reporta el documento
- [ ] 10.6 Crear `UpdateStudentCommandHandlerTests` agrupando por contexto: actualización válida, identificador inexistente y valores en conflicto
- [ ] 10.7 Verificar el caso límite central de HU04: reenviar el mismo documento y el mismo email que el estudiante ya tenía es una operación válida, porque la verificación excluye el propio `Id`
- [ ] 10.8 Verificar que un documento de otro estudiante y un email de otro estudiante producen los mensajes literales del PRD y dejan los datos sin cambios
- [ ] 10.9 Verificar que un `Id` inexistente y un `Id` de estudiante eliminado producen la excepción de inexistencia con el mensaje `No existe un estudiante con el Id indicado.`
- [ ] 10.10 Crear `DeleteStudentCommandHandlerTests` cubriendo la eliminación exitosa con su confirmación, el identificador inexistente y el caso límite de eliminación no repetible sobre un estudiante ya eliminado
- [ ] 10.11 Crear `GetStudentHandlerTests` cubriendo el detalle de un estudiante existente, el identificador inexistente y el estudiante eliminado, los tres con el mensaje esperado
- [ ] 10.12 Crear `GetAllStudentHandlerTests` verificando la composición del predicado: sin filtros, con cada filtro por separado y con dos filtros combinados que deben satisfacerse simultáneamente
- [ ] 10.13 Cubrir los casos límite del listado en el handler: filtro que no coincide con nadie devuelve colección vacía con total cero, y filtros con cadena vacía se tratan como ausentes
- [ ] 10.14 Verificar que el total de registros de los metadatos corresponde al conjunto filtrado y no al total general

## 11. Pruebas de persistencia

- [ ] 11.1 Crear `StudentAuditTests` sobre la base de pruebas verificando el sellado de creación: `CreatedDate` en UTC, `CreatedBy` en `system`, `IsDeleted` en falso y `LastModifiedDate`, `LastModifiedBy`, `DeleteDate` y `DeletedBy` nulos
- [ ] 11.2 Verificar el sellado de modificación: `LastModifiedDate` y `LastModifiedBy` asignados, `CreatedDate` y `CreatedBy` inalterados
- [ ] 11.3 Verificar el sellado de eliminación completo, incluido `DeletedBy`, que es el hueco que corrige la tarea 3.1
- [ ] 11.4 Crear `StudentSoftDeleteTests` verificando que un estudiante eliminado desaparece de las consultas y del conteo sin condición explícita, y que `IgnoreQueryFilters` sí lo devuelve
- [ ] 11.5 Verificar que la fila del estudiante eliminado permanece físicamente en la tabla
- [ ] 11.6 Crear `StudentUniquenessTests` verificando que los índices únicos de documento y de email rechazan la inserción duplicada a nivel de base de datos, incluido el caso límite de duplicar contra un estudiante eliminado
- [ ] 11.7 Crear `StudentPaginationTests` cubriendo las fronteras: cero registros en la página uno devuelve colección vacía sin error, la última página exacta devuelve los registros restantes, y la página siguiente a la última falla
- [ ] 11.8 Cubrir las fronteras del tamaño de página: 10 y 50 aceptados, 9 y 51 rechazados
- [ ] 11.9 Verificar que el DTO devuelto no expone ningún campo de auditoría
- [ ] 11.10 Crear `StudentCollationTests` verificando contra el motor los dos escenarios que dependen de la collation: el filtro por apellido `gomez` encuentra a `GOMEZ`, y el filtro por `perez` no encuentra a `Pérez` mientras que `Pérez` sí lo encuentra
- [ ] 11.11 Verificar que `JUAN@DOMINIO.COM` y `juan@dominio.com` colisionan en el índice único de email, por ser la collation insensible a mayúsculas

## 12. Verificación funcional de la API

- [ ] 12.1 Recorrer los cinco endpoints en Swagger confirmando los códigos de estado: `200` en las operaciones exitosas, `422` en validación, `400` en duplicados y `404` en inexistencia

## 13. Cierre

- [ ] 13.1 Ejecutar `dotnet build Haskoli.sln` y verificar que la solución compila sin advertencias nuevas
- [ ] 13.2 Ejecutar `dotnet test` y confirmar que la suite completa pasa sin pruebas omitidas
- [ ] 13.3 Recorrer los escenarios de la especificación y confirmar que cada uno tiene al menos una prueba que lo cubre
- [ ] 13.4 Verificar que los endpoints de `Country` siguen respondiendo igual que antes del cambio
- [ ] 13.5 Registrar en el seguimiento del proyecto el requisito diferido de borrado lógico en cascada de las materias del estudiante (HU05-CA05), para retomarlo cuando exista la entidad de inscripción
- [ ] 13.6 Ejecutar `openspec validate add-student-management` y archivar el cambio con `openspec archive add-student-management` una vez completada la verificación
