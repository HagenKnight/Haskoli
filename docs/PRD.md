# PRD — Haskoli

**Producto**: Háskóli

**Versión del documento**: 1.0 · 2026-08-17

**Estado**: definición para MVP

**Owner de producto**: equipo InterRapidísimo

**Audiencia de este documento**: equipo de ingeniería (backend, frontend), QA, y cualquier agente de IA que vaya a decomponer este PRD en backlog.

---

## 1. Resumen ejecutivo

> Háskóli [ˈhauː.skouːlɪ] en islandés significa universidad o academia.

**Háskóli** es una aplicación web de gestión de registro de materias para estudiantes de una institución educativa.


## 2. Requerimientos

1. Realizar un CRUD que le permita a un usuario realizar un registro en línea.
1. El estudiante se adhiere a un programa de créditos.
1. Existen 10 materias.
1. Cada materia equivale a 3 créditos.
1. El estudiante sólo podrá seleccionar 3 materias.
1. Hay 5 profesores que dictan 2 materias cada uno.
1. El estudiante no podrá tener clases con el mismo profesor.
1. Cada estudiante puede ver en línea los registros de otros estudiantes.
1. El estudiante podrá ver sólo el nombre de los alumnos con quienes compartirá cada
clase.

## 3. Historias de Usuario

### Épica 1 - Gestión de Estudiantes

**Objetivo:** permitir gestionar la información básica de los estudiantes que participan en el programa académico.

**Entidad Estudiante**

| Campo     | Tipo sugerido | Requerido | Regla                                                                 |
| --------- | ------------- | --------: | --------------------------------------------------------------------- |
| Id        | Guid / int    |        Sí | Llave única generada por el sistema, inmutable y no editable          |
| Documento | String        |        Sí | Único                                                                 |
| Nombre    | String        |        Sí | —                                                                     |
| Apellido  | String        |        Sí | —                                                                     |
| Email     | String        |        Sí | Formato válido y único                                                |

Adicionalmente, la entidad hereda los campos de auditoría de la arquitectura (`CreatedDate`, `CreatedBy`, `LastModifiedDate`, `LastModifiedBy`, `IsDeleted`, `DeleteDate`, `DeletedBy`). Estos campos los gestiona el sistema, no son editables por el usuario y no se exponen en la API ni en la interfaz.



#### HU01 - Registrar estudiante

> Como usuario de la aplicación 
>
> quiero registrar un estudiante proporcionando sus datos básicos
>
> para incorporarlo al sistema.


#### CA01 — Datos obligatorios

El sistema debe solicitar:

* Documento
* Nombre
* Apellido
* Email

#### CA02 — Documento único

No se debe permitir registrar un estudiante cuyo documento ya exista.

Ejemplo:
```Csharp
Documento:  123456789
Error:      Ya existe un estudiante registrado con el documento 123456789.
```

#### CA03 — Email válido

El sistema debe validar que el email tenga un formato válido.
```Csharp
usuario@dominio.com     ✓
usuario@                ✗
usuario                 ✗
```

#### CA04 — Email único

No se debe permitir registrar dos estudiantes con el mismo email.


#### CA05 — Auditoría de creación

Al registrar el estudiante el sistema almacena automáticamente la fecha y el usuario de creación. En el MVP el usuario se registra como `system`.

```Csharp
CreatedDate = <fecha UTC de la operación>
CreatedBy   = system
```

Los demás campos de auditoría quedan sin valor:
```Csharp
LastModifiedDate = NULL
LastModifiedBy   = NULL
IsDeleted        = false
DeleteDate       = NULL
DeletedBy        = NULL
```
---


#### HU02 — Consultar información de estudiantes

> Como usuario de la aplicación
>
> quiero consultar los estudiantes registrados
>
> para visualizar y gestionar su información.

#### CA01 — Visualización de datos

| Documento | Nombre | Apellido | Email |
|-----------|--------|----------|-------|


* Los datos se deberán mostrar paginados
* No se muestran los estudiantes eliminados (ver HU05)

#### CA02 - Filtros

Uso de filtros para:
* Documento
* Apellido
* Email

---

#### HU03 — Consultar estudiante

> Como usuario de la aplicación
> 
> quiero consultar el detalle de un estudiante
> 
> para visualizar toda su información registrada.

El estudiante se consulta por su `Id`.

Debe retornar:
```csharp
Id
Documento
Nombre
Apellido
Email
```

Un estudiante eliminado no es consultable (ver HU05).

---

#### HU04 — Actualizar información del estudiante

> Como usuario de la aplicación
>
> quiero actualizar la información de un estudiante
>
> para mantener sus datos actualizados.

#### CA01 — Identificación del estudiante

El estudiante a actualizar se identifica por su `Id`, el cual no es editable.

```Csharp
Id:     8f3a...c21
Error:  No existe un estudiante con el Id indicado.
```

#### CA02 — Campos editables

Se permite actualizar:

* Documento
* Nombre
* Apellido
* Email

Los campos `Id` y los de auditoría no son editables.

#### CA03 — Datos obligatorios

Documento, Nombre, Apellido y Email siguen siendo obligatorios; no se acepta enviarlos vacíos o nulos.

#### CA04 — Documento único al modificar

El documento puede modificarse siempre que el nuevo valor no exista en otro estudiante. Si se envía el mismo documento que ya tenía el estudiante, la operación es válida.

```Csharp
Documento:  123456789
Error:      Ya existe un estudiante registrado con el documento 123456789.
```

#### CA05 — Email válido y único

Se aplican las mismas validaciones de formato del CA03 de HU01. El email no puede pertenecer a otro estudiante; sí se acepta si es el mismo que ya tenía el estudiante.

```Csharp
Email:      juan.perez@dominio.com
Error:      El email juan.perez@dominio.com ya está registrado por otro estudiante.
```

#### CA06 — Auditoría de modificación

Tras una actualización exitosa el sistema almacena automáticamente la fecha y el usuario de la modificación. En el MVP el usuario se registra como `system`.

```Csharp
LastModifiedDate = <fecha UTC de la operación>
LastModifiedBy   = system
```

`CreatedDate` y `CreatedBy` permanecen inmutables. Si la operación falla por validación, no se altera ningún campo de auditoría.

#### CA07 — Confirmación del resultado

Tras una actualización exitosa el sistema retorna el estudiante con sus datos actualizados y muestra un mensaje de confirmación.

---

#### HU05 — Eliminar estudiante

> Como usuario de la aplicación
>
> quiero eliminar un estudiante
>
> para retirarlo del sistema cuando su registro ya no debe existir.

#### CA01 — Estudiante existente

El estudiante se identifica por su `Id`. Si no existe, se retorna error:

```Csharp
Error:      No existe un estudiante con el Id indicado.
```

#### CA02 — Eliminación del sistema

Una vez eliminado, el estudiante deja de existir para todos los efectos del sistema: no aparece en el listado de HU02, no es consultable en HU03 y no puede actualizarse mediante HU04.

#### CA03 — Borrado lógico

La eliminación se implementa como borrado lógico usando los campos de auditoría de la arquitectura. El registro se conserva en la base de datos y no se borra físicamente.

```Csharp
IsDeleted = true
```

#### CA04 — Auditoría de eliminación

Al eliminar, el sistema almacena automáticamente la fecha y el usuario de la eliminación. En el MVP el usuario se registra como `system`.

```Csharp
DeleteDate = <fecha UTC de la operación>
DeletedBy  = system
```

#### CA05 — Eliminación en cascada

Los registros de materias asociados al estudiante también se eliminan mediante borrado lógico, aplicando los mismos campos de auditoría (`IsDeleted`, `DeleteDate`, `DeletedBy`).

#### CA06 — Eliminación no repetible

No se puede eliminar un estudiante ya eliminado; el sistema responde como si el `Id` no existiera.

```Csharp
Error:      No existe un estudiante con el Id indicado.
```

#### CA07 — Confirmación explícita

La interfaz solicita confirmación al usuario antes de ejecutar la eliminación, advirtiendo que la acción retira al estudiante del sistema.


