## Context

La Épica 1 del PRD introduce la primera entidad de negocio real de Háskóli. El proyecto ya tiene toda la maquinaria de Clean Architecture montada (MediatR, AutoMapper, FluentValidation, EF Core, repositorio genérico y Unit of Work), pero la única entidad existente, `Country`, ejerce exclusivamente el camino de lectura.

Estado actual verificado en el código:

- `Haskoli.Domain/Entities/Base/EntityBase.cs` define `EntityBase<TKey>` con `Id` (`DatabaseGeneratedOption.Identity`) y los siete campos de auditoría del PRD.
- `HaskoliDbContext.SaveChangesAsync` ya sella `CreatedDate`/`CreatedBy` al insertar y `LastModifiedDate`/`LastModifiedBy` al modificar, con el literal `"system"`. Solo itera `ChangeTracker.Entries<EntityBase<int>>()`.
- `CRUDService` (`Haskoli.Infrastructure.Common/Services/Base/CRUDService.cs`) implementa `InsertAsync`, `UpdateAsync` y `DeleteAsync`. `DeleteAsync` con `autoSave = true` ya hace borrado lógico asignando `IsDeleted` y `DeleteDate`, pero **no asigna `DeletedBy`**.
- `CRUDService.FindAsync` ya trata un registro con `IsDeleted = true` como inexistente y lanza `EntityNotFoundException`. `GetAllAsync` y `GetCount()` **no** filtran por `IsDeleted`.
- No existe ningún filtro de consulta global en la solución.
- `PagedList<T>`, `MetaData<T>`, `Pagination`, `NavLinks`, `ConverterPaging`, `IUriService`, `IDataShapeHelper` y `RequestParameter` existen y están operativos.
- Los comandos de `Country` (`CreateCountryCommandHandler`, `UpdateCountryCommandHandler`, `DeleteCountryCommandHandler`) lanzan `NotImplementedException`, y `CountryController` solo expone `GET` y `GET {id}`. El bloque de listado paginado del handler está comentado.
- El código comentado de `GetAllCountryHandler` referencia `WhereFilter`, `BuildExpressionLambda`, `GroupOp`, `WhereConditionsOp` y `Portfolio.Core.Entities.Country`, **ninguno de los cuales existe en este repositorio**. Es un residuo copiado de otro proyecto y no puede servir de base.
- `DbFactory<HaskoliDbContext>` e `IUnitOfWork<HaskoliDbContext>` están registrados como `Scoped`, y `DbFactory.Init()` cachea la instancia obtenida de `Func<HaskoliDbContext>` (resuelto del contenedor). Por lo tanto el repositorio y el Unit of Work comparten la misma instancia de `DbContext` dentro de una petición, y `CommitAsync` persiste correctamente lo que el repositorio encoló. Este punto no había sido ejercido por ninguna funcionalidad hasta ahora.
- El proyecto `tests/Haskoli.NTest` existe y está registrado en `Haskoli.sln`. Apunta a `net10.0` con `ImplicitUsings` y `Nullable` habilitados, y trae NUnit 4.3.2, `NUnit3TestAdapter` 5.0.0, `Microsoft.NET.Test.Sdk` 17.14.0 y `coverlet.collector` 6.0.4. No contiene ninguna prueba ni referencia a los proyectos de la solución.
- El contexto aplica la collation `Modern_Spanish_CI_AS` globalmente: insensible a mayúsculas, **sensible a acentos**.
- No existía ninguna migración para `HaskoliDbContext`: la carpeta `Data/Migrations` estaba vacía, de modo que el esquema nunca se había materializado desde migraciones.
- `CountryConfiguration` siembra sus 244 filas con `CreatedDate = DateTime.Now`. El valor dinámico hace que el modelo cambie en cada compilación y EF Core rechaza aplicar migraciones con `PendingModelChangesWarning`.
- `HaskoliDbContextFactory` carga únicamente `appsettings.json`, cuyas cadenas de conexión viajan en blanco por tratarse de una plantilla. Las cadenas reales viven en `appsettings.Development.json`.

Restricciones heredadas de `openspec/specs/platform-runtime/spec.md`: todo en `net10.0`, EF Core como único ORM, familias `Microsoft.*` en la banda `10.0.x` y sin dependencias nuevas innecesarias. El proveedor de base de datos operativo es SQL Server (`"Database": "mssql"`).

## Goals / Non-Goals

**Goals:**

- Entregar el CRUD completo de `Student` de extremo a extremo, cumpliendo los criterios de aceptación de HU01 a HU05.
- Dejar el patrón de escritura establecido y verificado, para que las épicas siguientes lo copien en lugar de reinventarlo.
- Cubrir cada escenario de la especificación con pruebas automatizadas, verificando contra el motor real aquellos cuyo resultado depende de él, y establecer así el patrón de pruebas que heredarán las épicas siguientes.
- Cerrar los huecos de la infraestructura compartida que impiden cumplir el PRD (`DeletedBy` sin asignar, ausencia de filtro de eliminados, paginación que falla con cero resultados), con el mínimo cambio necesario y sin romper `Country`.
- Reutilizar la infraestructura existente en lugar de introducir mecanismos paralelos.

**Non-Goals:**

- Interfaz de usuario. Los criterios de presentación y confirmación (HU02-CA01, HU04-CA07, HU05-CA07) se materializan como contrato de la API; la implementación visual es un cambio aparte.
- Borrado lógico en cascada de las materias del estudiante (HU05-CA05): la entidad de inscripción no existe todavía.
- Autenticación y autorización de los endpoints de estudiante. El usuario de auditoría permanece en el literal `system` que ya usa la arquitectura.
- Reescribir el repositorio genérico, el Unit of Work o el modelo de paginación. Se ajusta lo imprescindible.
- Implementar los comandos pendientes de `Country`, más allá de no romperlos.
- Escribir pruebas para `Country` o para la infraestructura genérica más allá de lo que los escenarios de estudiantes ejercitan.
- Pruebas de extremo a extremo sobre la API en ejecución (`WebApplicationFactory`), pruebas de carga o de contrato HTTP.

## Decisions

### Nomenclatura en inglés para el código, español para los mensajes

La entidad se llama `Student` y sus propiedades `Document`, `FirstName`, `LastName`, `Email`; el JSON expuesto queda en camelCase (`document`, `firstName`, `lastName`, `email`) por la configuración global de Newtonsoft.Json. Los mensajes de error dirigidos al usuario se mantienen en español, con el texto literal del PRD.

*Por qué:* el código existente es íntegramente inglés (`Country`, `NameEs`, `CreatedBy`), y mezclar idiomas en los identificadores dentro de la misma capa genera fricción permanente. Los mensajes, en cambio, son producto y deben coincidir con el PRD palabra por palabra, porque son criterio de aceptación.

*Alternativa descartada:* nombrar la entidad `Estudiante` con propiedades en español. Coincide mejor con el PRD y con el equipo, pero rompe la consistencia del modelo y obliga a decidir el idioma de nuevo en cada entidad futura.

### `Id` de tipo `int`, no `Guid`

`Student` hereda de `EntityBase<int>`, con `Id` generado por la base de datos.

*Por qué:* el PRD admite `Guid` o `int`, pero la infraestructura está atada a `int` en tres puntos: `HaskoliDbContext.SaveChangesAsync` solo recorre `ChangeTracker.Entries<EntityBase<int>>()`, y `CRUDService.UpdateAsync`/`DeleteAsync` hacen `Convert.ToInt32(...)` sobre el `Id`. Un `Guid` quedaría fuera del sellado automático de auditoría y rompería el servicio genérico, es decir, exigiría refactorizar infraestructura compartida sin ganancia funcional para el MVP.

*Alternativa descartada:* `Guid` con generalización de la auditoría a `EntityBase<TKey>`. Es el camino correcto si en el futuro se necesitan identificadores no adivinables o generados en cliente, pero es un cambio transversal que merece su propia propuesta.

### Exclusión de eliminados mediante filtro de consulta global

`StudentConfiguration` declara `builder.HasQueryFilter(s => !s.IsDeleted)`.

*Por qué:* garantiza que cualquier consulta futura sobre estudiantes excluya los eliminados sin que el autor tenga que recordarlo, que es exactamente el modo en que hoy se escapa el requisito (`GetAllAsync` y `GetCount()` no filtran nada). Además hace que `GetByIdAsync` devuelva `null` para un registro eliminado, con lo cual `FindAsync`, `UpdateAsync` y `DeleteAsync` del servicio genérico responden `EntityNotFoundException` sin código adicional: eso cubre de una sola vez HU03, HU04 y HU05-CA02/CA06.

*Consecuencia que hay que manejar:* la verificación de unicidad debe atravesar el filtro explícitamente con `IgnoreQueryFilters()`, o un documento perteneciente a un estudiante eliminado parecería libre y provocaría una violación del índice único en la base de datos.

*Alternativa descartada:* predicados explícitos `x => !x.IsDeleted` en cada consulta. No toca infraestructura compartida y es más visible, pero deja el requisito a merced de la disciplina de cada desarrollador y ya se demostró frágil.

### Verificación de unicidad en el handler, respaldada por índices únicos

`StudentConfiguration` declara índices únicos sobre `Document` y `Email`. Además, los handlers de creación y actualización consultan previamente la existencia del valor y lanzan una excepción de negocio con el mensaje del PRD.

*Por qué:* el índice es la única garantía real frente a concurrencia, pero una violación de índice produce una `DbUpdateException` cuyo mensaje no sirve como respuesta al usuario. La consulta previa entrega el mensaje exacto que exigen HU01-CA02 y HU04-CA04. Las dos capas son complementarias: la primera comunica, la segunda protege.

Para consultar sin que el filtro global oculte a los eliminados, `StudentRepository` expone métodos propios que usan `DbContext.Set<Student>().IgnoreQueryFilters()`. `BaseRepository` mantiene `DbContext` como miembro `protected`, así que no hace falta modificarlo.

*Alternativa descartada:* validar unicidad dentro del validador de FluentValidation. Es posible inyectando el repositorio, pero mezcla validación de formato con consulta a base de datos y obliga a mapear el fallo al código de estado de validación (`422`), cuando se trata de una regla de negocio.

### Separación de códigos de estado: formato en `422`, negocio en `400`

Obligatoriedad y formato de email se validan con FluentValidation, que el `ValidationBehaviour` convierte en `ValidateException` y el `ErrorHandlerMiddleware` traduce a `422` con el diccionario `errors`. Las colisiones de documento y email se señalan con `EntityAlreadyExistException`, que el mismo middleware traduce a `400`. La inexistencia de un `Id` se señala con `EntityNotFoundException`, que produce `404`.

*Por qué:* respeta el mapeo que el `ErrorHandlerMiddleware` ya define y distingue "la petición está mal formada" de "la petición es válida pero contradice el estado del sistema", que es información útil para quien consuma la API.

### Unicidad que abarca también a los estudiantes eliminados

Un documento o email que pertenezca a un estudiante eliminado lógicamente no se libera para reutilización.

*Por qué:* el registro sigue existiendo físicamente y el índice único de la base de datos lo abarca; permitir la reutilización exigiría índices filtrados y abriría la puerta a dos filas con el mismo documento, lo que degrada el valor del borrado lógico como rastro de auditoría. El PRD no pide liberar el identificador.

*Coste asumido:* si se elimina un estudiante por error y se intenta volver a registrarlo con el mismo documento, la operación falla y hace falta intervención sobre el dato. Queda como pregunta abierta si el negocio necesita una reactivación.

### Reutilizar `CRUDService` en lugar de escribir un servicio propio

`StudentService` hereda de `CRUDService<...>` igual que `CountryService`, y añade únicamente los métodos de consulta específicos del dominio.

*Por qué:* `InsertAsync`, `UpdateAsync` y `DeleteAsync` ya resuelven mapeo, sellado de auditoría y commit; reimplementarlos crearía dos caminos divergentes para el mismo problema y dejaría la entidad de referencia sin representar el patrón real.

### Correcciones puntuales en la infraestructura compartida

Tres ajustes mínimos, cada uno atado a un criterio de aceptación:

1. **`CRUDService.DeleteAsync` asigna `DeletedBy = "system"`.** Hoy asigna `IsDeleted` y `DeleteDate` pero omite el autor, y HU05-CA04 lo exige. Se hace en el servicio y no en `SaveChangesAsync` porque allí un borrado lógico es indistinguible de una modificación corriente, y sellarlo ahí sobrescribiría `LastModified*`.
2. **`CRUDService.GetPagedAsync` tolera el conjunto vacío.** La guarda actual, `pageNumber > Math.Ceiling(_iCount / (double)pageSize)`, lanza `PageRowIndexNotFound` para la página 1 cuando no hay registros, porque el total de páginas es 0. Un listado vacío es un resultado legítimo, no un error. Se añade la condición de que existan registros antes de evaluar el límite superior.
3. **Alineación de los límites de tamaño de página.** `CRUDService.GetPagedAsync` exige `pageSize` entre 10 y 50, mientras que el constructor de `RequestParameter` lo recorta a un máximo de 10 (`pageSize > 10 ? 10 : pageSize`). El parámetro de consulta de estudiantes normaliza el valor dentro del rango que el servicio acepta, en lugar de heredar el recorte incoherente.

*Riesgo aceptado:* los puntos 2 y 3 alteran el comportamiento de la paginación para toda entidad que use el servicio genérico. `Country` no ejerce hoy esa ruta (su handler paginado está comentado), así que el impacto real es nulo, pero queda registrado.

### Filtros construidos explícitamente sobre `Expression`

El listado recibe un parámetro dedicado, `GetAllStudentParameter : RequestParameter`, con `Document`, `LastName` y `Email` opcionales. El handler compone un único predicado `Expression<Func<Student, bool>>` combinando con `AND` solo los filtros presentes, y lo pasa a la sobrecarga de `GetPagedAsync` que acepta predicado, de modo que el filtrado y el conteo ocurren en la base de datos.

*Por qué:* el mecanismo declarativo que sugiere el código comentado de `Country` no existe en este repositorio, y la coincidencia parcial insensible a mayúsculas se resuelve con `Contains` sobre una columna con collation `Modern_Spanish_CI_AS`, que es la que el `HaskoliDbContext` ya aplica globalmente. No hace falta ni `ToLower()` ni una dependencia nueva.

*Alternativa descartada:* reutilizar el campo `Search` único de `RequestParameter` aplicándolo a los tres campos con `OR`. Es menos código, pero HU02-CA02 pide filtros por campo, que es una capacidad distinta y más precisa.

### Respuesta paginada en `ApiResponse<MetaData<StudentDTO>>`

El listado devuelve el envoltorio estándar sobre `MetaData<StudentDTO>`, construido a partir de `PagedList<StudentDTO>` mediante el `ConverterPaging` ya registrado en AutoMapper.

*Por qué:* es el contrato que la arquitectura ya definió para colecciones paginadas y el único que transporta los metadatos que HU02-CA01 requiere.

*Trampa conocida:* el constructor de `PagedList` lanza `ArgumentNullException` si `IUriService` es nulo, y `IUriService` está registrado como singleton que captura `HttpContext.Request` en el momento de su construcción. El handler debe recibirlo por inyección igual que `GetAllCountryHandler`, y la ruta debe pasarse desde el controlador (`Request.Path.Value`) para que los enlaces de navegación se generen.

### Estrategia de pruebas en tres niveles

Los criterios de aceptación no viven todos en la misma capa, así que se verifican donde realmente ocurren, y cada uno en un solo sitio:

| Nivel | Qué verifica | Cómo se aísla |
| --- | --- | --- |
| Validadores | Obligatoriedad de los cuatro campos y formato del email (HU01-CA01/CA03, HU04-CA03) | Sin dobles: el validador es una función pura sobre el DTO |
| Handlers | Unicidad y sus mensajes literales, exclusión del propio `Id` al actualizar, traducción a excepciones, forma de la respuesta y composición de filtros (HU01-CA02/CA04, HU03, HU04-CA01/CA04/CA05/CA07, HU05-CA01/CA06) | `IStudentService` e `IStudentRepository` sustituidos con NSubstitute |
| Persistencia | Sellado de auditoría, borrado lógico, filtro global, unicidad efectiva en base de datos, collation y paginación (HU01-CA05, HU02-CA01, HU04-CA06, HU05-CA03/CA04) | `HaskoliDbContext` real sobre SQL Server, vaciando `Student` entre pruebas |

*Por qué así:* verificar la auditoría o el filtro global con dobles de prueba solo comprobaría que el doble hace lo que se le dijo. Esos criterios son afirmaciones sobre lo que la base de datos acaba conteniendo, y exigen un contexto real. A la inversa, levantar una base de datos para comprobar que un email sin arroba se rechaza es desperdicio: eso es una función pura.

*Regla sobre dobles:* se sustituyen únicamente las fronteras que cruzan a otra capa. Las entidades y los DTO se construyen con datos reales mediante un constructor de datos de prueba (`StudentBuilder`) con valores por defecto válidos y sobrescritura selectiva, de modo que cada prueba declare solo el dato que le importa y el lector no tenga que adivinar cuál de los campos causa el comportamiento.

### Agrupación y nomenclatura equivalente a describe/it

NUnit no tiene `describe`/`it`, pero su equivalente idiomático es anidar clases marcadas con `[TestFixture]`: la clase externa nombra la historia de usuario, la clase anidada nombra el contexto o criterio, y el método nombra el comportamiento esperado. El resultado se lee como una frase completa en el ejecutor de pruebas y en la salida de `dotnet test`.

```
RegistrarEstudiante                          → describe (HU01)
  CuandoElDocumentoYaPerteneceAOtroEstudiante  → describe anidado (CA02)
    RechazaElRegistroConElMensajeDelPrd()        → it
    NoPersisteNingunRegistro()                   → it
```

Los métodos se nombran en tercera persona describiendo el comportamiento observable, nunca el nombre del método bajo prueba, para que el fallo comunique qué se rompió y no dónde. Los casos límite que solo varían en el dato de entrada usan `[TestCase]`, manteniendo una única aserción conceptual por prueba.

Cada clase externa se etiqueta con `[Category]` indicando la historia de usuario, lo que permite ejecutar por épica con `dotnet test --filter` y da la trazabilidad entre criterio y prueba que la especificación exige.

### Las pruebas de persistencia corren contra SQL Server real

Las pruebas de persistencia instancian `HaskoliDbContext` con `UseSqlServer` sobre la misma base que usa el desarrollo, y vacían la tabla `Student` antes y después de cada prueba reiniciando su columna identidad. La limpieza se acota a `Student`: el seed de 244 países de `Country` queda intacto, porque ninguna prueba de estudiantes tiene motivo para tocarlo.

*Por qué:* los criterios que esta capa verifica son afirmaciones sobre lo que el motor acaba haciendo. Los índices únicos, el filtro global y la collation `Modern_Spanish_CI_AS` son comportamiento del motor, y comprobarlos sobre un sustituto solo demuestra cómo se comporta el sustituto. En particular, dos escenarios de la especificación son inverificables fuera de SQL Server: que el filtro por `gomez` encuentre a `GOMEZ` y que el filtro por `perez` **no** encuentre a `Pérez`. Con esta decisión pasan de estar documentados como limitación a estar realmente cubiertos.

*Consecuencia asumida:* la suite necesita una instancia de SQL Server accesible; `dotnet test` no corre en una máquina sin motor. Es un intercambio deliberado de portabilidad por fidelidad.

*Consecuencia asumida:* al compartir base con el desarrollo, ejecutar la suite borra los estudiantes que hubiera cargados. Se acepta porque `Student` es una tabla de trabajo sin datos de valor, y la limpieza no alcanza a ninguna otra tabla. Si en algún momento la base de desarrollo pasa a contener datos que importen, la salida natural es apuntar la cadena de las pruebas a una base propia; la configuración ya está aislada en el `appsettings.json` del proyecto de pruebas precisamente para que ese cambio sea de una línea.

*Alternativa descartada:* SQLite en memoria. Es portable y rápido, pero traduce `string.Contains` a `instr()`, que distingue mayúsculas, y no reproduce la collation. Habría dejado los dos escenarios de búsqueda sin verificación real y la unicidad de email insensible a mayúsculas (`JUAN@X.COM` frente a `juan@x.com`) comportándose distinto que en producción.

*Alternativa descartada:* el proveedor InMemory de EF Core. No aplica índices únicos ni filtros de consulta con fidelidad, que son justo dos de los mecanismos a verificar. Microsoft desaconseja usarlo para validar comportamiento relacional.

*Alternativa descartada:* Testcontainers. Da fidelidad total sin depender de una instancia instalada, pero exige Docker en la máquina de desarrollo, que hoy no forma parte del entorno del proyecto.

### La collation se garantiza al crear la base, no en la migración

EF Core no emite `ALTER DATABASE ... COLLATE` en una migración inicial, aunque el modelo declare `UseCollation`. La migración generada crea las tablas pero no fija la collation, de modo que las columnas heredan la de la base y, en última instancia, la del servidor.

Por eso la base se crea explícitamente con `CREATE DATABASE [Haskoli] COLLATE Modern_Spanish_CI_AS` antes de aplicar la migración. Verificado tras aplicarla: las columnas de texto de `Student` reportan `Modern_Spanish_CI_AS`.

*Alternativa descartada:* añadir a mano un `AlterDatabase` en el `Up()` de la migración. `ALTER DATABASE ... COLLATE` exige acceso exclusivo a la base y no reescribe la collation de las columnas ya existentes, así que resuelve mal un problema que la creación de la base resuelve bien.

*Alternativa descartada:* declarar la collation columna por columna en `StudentConfiguration`. Sería independiente de cómo se creó la base, pero convierte en local una decisión que es de todo el esquema y obliga a repetirla en cada entidad futura.

## Risks / Trade-offs

- **Modificar `CRUDService` afecta a todas las entidades presentes y futuras** → Los tres cambios son aditivos o correctivos, ninguno altera una firma pública. `Country` no ejerce las rutas tocadas (sus comandos lanzan `NotImplementedException` y su paginación está comentada), por lo que la superficie de regresión real se limita a `Student`. Verificar que la solución compile completa tras el cambio.

- **El filtro de consulta global puede ocultar registros donde sí se los necesita** → La única operación que legítimamente necesita ver los eliminados es la verificación de unicidad, y se resuelve con `IgnoreQueryFilters()` en métodos explícitos del repositorio de estudiantes. Cualquier consulta futura que requiera ver eliminados deberá hacerlo igual de explícitamente, lo cual es el comportamiento deseado.

- **`FindAsync` del repositorio devuelve entidades del caché del contexto** → `BaseRepository.GetByIdAsync` usa `_dbSet.FindAsync`, que primero consulta el rastreador de cambios. Dentro de una misma petición esto no genera inconsistencias porque cada operación es atómica, pero conviene no encadenar lectura y escritura del mismo registro en un solo handler asumiendo estado fresco.

- **Los enlaces de navegación de la paginación pueden salir con un tamaño de página incorrecto** → `PagedList` construye los enlaces con `new RequestParameter(...)`, cuyo constructor recorta `PageSize` a 10. Con un tamaño de 20 o 50, los enlaces anunciarían 10. Es un defecto preexistente ajeno a esta épica; los metadatos numéricos (`Paging`) sí son correctos. Si se decide corregirlo, es un cambio aparte porque toca el contrato de paginación de toda la API.

- **Índice único sobre `Email` y `Document` con collation insensible a mayúsculas** → La collation global `Modern_Spanish_CI_AS` hace que `JUAN@DOMINIO.COM` y `juan@dominio.com` colisionen en el índice. Es el comportamiento correcto para un email, pero conviene tenerlo presente al interpretar los errores de duplicado.

- **`EntityBase.Id` está marcado con `DatabaseGeneratedOption.Identity` y `CommandDTO` expone `Id`** → Un cliente podría enviar `id` al crear. El mapeo debe ignorarlo explícitamente en la creación para que EF no intente insertar un valor en una columna identidad.

- **La suite deja de ser ejecutable en una máquina sin SQL Server** → Es la contrapartida aceptada de verificar collation e índices contra el motor real. Si más adelante hace falta correr las pruebas en integración continua, la salida es levantar el motor en el agente o aislar las pruebas de persistencia en una categoría propia; el resto de la suite (validadores y handlers) no toca base de datos y seguiría corriendo en cualquier entorno.

- **Las pruebas comparten base con el desarrollo** → Ejecutar la suite vacía la tabla `Student`. La limpieza se acota a esa tabla y no alcanza al seed de `Country`, y la cadena vive en el `appsettings.json` del proyecto de pruebas para poder apuntarla a una base propia en cuanto los datos de desarrollo pasen a importar.

- **La migración inicial no fija la collation** → EF Core no emite `ALTER DATABASE ... COLLATE` en una migración inicial, así que la base debe crearse ya con `Modern_Spanish_CI_AS`. Una base creada por defecto por EF o por el servidor heredaría otra collation y cambiaría en silencio el resultado de los filtros y de la unicidad de email. Confirmar la collation de las columnas después de aplicar la migración en cualquier entorno nuevo.

- **Fijar la fecha del seed de `Country` cambia datos existentes** → Donde el seed ya se hubiera aplicado, la nueva migración actualizaría `CreatedDate` de las 244 filas al valor constante. Es un campo de auditoría de datos de referencia, sin significado funcional, pero conviene no confundirlo con una modificación real.

- **Una suite que depende de detalles internos se vuelve un lastre** → Las pruebas de handlers verifican el comportamiento observable (excepción lanzada, mensaje, contenido de la respuesta) y no el número de invocaciones a un doble, salvo cuando el criterio de aceptación es precisamente que algo no se persistió. Verificar interacciones por defecto ataría la suite a la implementación y encarecería cualquier refactor.

- **Longitud de los campos sin definir en el PRD** → El PRD no especifica longitudes máximas para documento, nombre, apellido ni email. Se fijan valores conservadores en la configuración y en los validadores para que coincidan entre sí; si el negocio necesita otros, el ajuste es una migración trivial.

## Migration Plan

1. Implementar el modelo, la configuración y el `DbSet`, y generar la migración de EF Core sobre `HaskoliDbContext` con `--output-dir Data/Migrations`, usando `Haskoli.Api` como proyecto de arranque.
2. Revisar el archivo de migración generado antes de aplicarlo, verificando que cree la tabla `Student` con los dos índices únicos y que no arrastre cambios no deseados del modelo de `Country`.
3. Aplicar la migración con `dotnet ef database update --context HaskoliDbContext`.
4. Implementar y verificar las capas restantes de abajo hacia arriba: repositorio y servicio, luego features de MediatR, luego controlador.
5. Verificar los escenarios de la especificación vía Swagger.

**Rollback:** la migración es puramente aditiva (una tabla nueva, sin alterar tablas existentes), así que revertirla con `dotnet ef database update <MigraciónAnterior>` no destruye datos previos. Los ajustes en `CRUDService` son correcciones de comportamiento reversibles por código, sin efecto en el esquema.

## Open Questions

- **Usuario de auditoría real.** Todo se sella con el literal `"system"`, tal como el PRD dispone para el MVP. Cuando los endpoints se protejan con JWT (la infraestructura de identidad ya existe), habrá que sustituirlo por el usuario autenticado, probablemente vía un `ICurrentUserService` inyectado en el `DbContext`. No es parte de esta épica.
- **Reactivación de estudiantes eliminados.** Con la decisión de no liberar documento ni email, un estudiante eliminado por error no puede volver a registrarse con los mismos datos. ¿Necesita el negocio una operación de reactivación, o basta con la intervención manual sobre el dato?
- **Autorización de los endpoints.** Todo el CRUD queda anónimo, coherente con "usuario de la aplicación" del PRD y con el `CountryController` actual. ¿Debe restringirse a un rol antes de salir del MVP?
- **Alcance del `Search` genérico.** Se implementan los tres filtros por campo que pide HU02-CA02. Queda pendiente si además se quiere una búsqueda libre sobre los tres campos a la vez.
