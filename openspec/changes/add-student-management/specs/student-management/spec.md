## ADDED Requirements

### Requirement: Modelo de datos del estudiante

El sistema SHALL persistir un estudiante con los campos documento, nombre, apellido y email, todos obligatorios, más un identificador `Id` numérico generado por el sistema. El `Id` MUST ser inmutable y no editable por el usuario.

El estudiante SHALL heredar los campos de auditoría de la arquitectura (`CreatedDate`, `CreatedBy`, `LastModifiedDate`, `LastModifiedBy`, `IsDeleted`, `DeleteDate`, `DeletedBy`). Esos campos MUST ser gestionados exclusivamente por el sistema y MUST NOT exponerse en la API.

#### Scenario: Identificador generado por el sistema

- **WHEN** se registra un estudiante sin indicar `Id`
- **THEN** el sistema SHALL asignar un `Id` único generado por la base de datos
- **AND** el estudiante persistido SHALL contener ese `Id`

#### Scenario: Campos de auditoría ocultos en la API

- **WHEN** cualquier endpoint de estudiante devuelve un estudiante
- **THEN** la representación SHALL contener únicamente `id`, `document`, `firstName`, `lastName` y `email`
- **AND** MUST NOT contener `createdDate`, `createdBy`, `lastModifiedDate`, `lastModifiedBy`, `isDeleted`, `deleteDate` ni `deletedBy`

#### Scenario: Id ignorado si se envía en la creación

- **WHEN** una petición de registro incluye un `id` en el cuerpo
- **THEN** el sistema SHALL ignorar ese valor y generar el `Id` internamente

---

### Requirement: Contrato de la API de estudiantes

El sistema SHALL exponer la gestión de estudiantes bajo la ruta `api/Student` con las siguientes operaciones: registro (`POST`), listado paginado (`GET`), consulta por identificador (`GET {id}`), actualización (`PUT`) y eliminación (`DELETE {id}`).

Las respuestas SHALL usar el envoltorio `ApiResponse<T>` existente, con `succeeded` en `true` y `message` de confirmación en las operaciones exitosas, y `succeeded` en `false` con el detalle del fallo en los errores. La serialización SHALL usar camelCase, conforme a la configuración global de la API.

#### Scenario: Registro exitoso

- **WHEN** se envía `POST api/Student` con documento, nombre, apellido y email válidos
- **THEN** el sistema SHALL responder con estado `200`
- **AND** el cuerpo SHALL contener `succeeded: true` y el estudiante creado con su `id` asignado

#### Scenario: Consulta exitosa por identificador

- **WHEN** se envía `GET api/Student/{id}` con el `Id` de un estudiante existente y no eliminado
- **THEN** el sistema SHALL responder con estado `200`
- **AND** el cuerpo SHALL contener `id`, `document`, `firstName`, `lastName` y `email` de ese estudiante

#### Scenario: Error de validación de formato

- **WHEN** una petición de escritura incumple una regla de validación de formato
- **THEN** el sistema SHALL responder con estado `422`
- **AND** el cuerpo SHALL contener `succeeded: false` y el diccionario `errors` con el detalle por campo

---

### Requirement: Registro de estudiante

El sistema SHALL permitir registrar un estudiante proporcionando documento, nombre, apellido y email. Los cuatro campos MUST estar presentes y no vacíos; una petición que omita cualquiera de ellos o lo envíe nulo o en blanco MUST ser rechazada sin persistir nada.

#### Scenario: Datos obligatorios completos

- **WHEN** se registra un estudiante con documento, nombre, apellido y email presentes y no vacíos
- **THEN** el sistema SHALL persistir el estudiante
- **AND** SHALL devolver el estudiante creado con su `Id`

#### Scenario: Falta un campo obligatorio

- **WHEN** se intenta registrar un estudiante omitiendo el nombre
- **THEN** el sistema SHALL rechazar la operación con estado `422`
- **AND** MUST NOT persistir ningún registro
- **AND** el detalle del error SHALL identificar el campo faltante

#### Scenario: Campo obligatorio en blanco

- **WHEN** se intenta registrar un estudiante cuyo apellido es una cadena vacía o solo espacios
- **THEN** el sistema SHALL rechazar la operación con estado `422`
- **AND** MUST NOT persistir ningún registro

---

### Requirement: Unicidad del documento

El documento SHALL ser único entre los estudiantes. El sistema MUST NOT permitir registrar ni actualizar un estudiante con un documento que ya pertenezca a otro estudiante.

Al actualizar, reenviar el mismo documento que el estudiante ya tenía SHALL considerarse válido.

El mensaje de error SHALL tener la forma `Ya existe un estudiante registrado con el documento <documento>.`

#### Scenario: Documento duplicado al registrar

- **WHEN** se intenta registrar un estudiante con el documento `123456789` y ya existe otro estudiante con ese documento
- **THEN** el sistema SHALL rechazar la operación
- **AND** el mensaje de error SHALL ser `Ya existe un estudiante registrado con el documento 123456789.`
- **AND** MUST NOT persistir ningún registro

#### Scenario: Documento duplicado al actualizar

- **WHEN** se intenta actualizar un estudiante asignándole el documento `123456789`, que pertenece a un estudiante distinto
- **THEN** el sistema SHALL rechazar la operación
- **AND** el mensaje de error SHALL ser `Ya existe un estudiante registrado con el documento 123456789.`
- **AND** los datos del estudiante MUST permanecer sin cambios

#### Scenario: Documento sin cambios al actualizar

- **WHEN** se actualiza un estudiante enviando el mismo documento que ya tenía
- **THEN** el sistema SHALL aceptar la operación

#### Scenario: Documento liberado por un estudiante eliminado

- **WHEN** se registra un estudiante con un documento que solo pertenece a un estudiante eliminado lógicamente
- **THEN** el sistema SHALL rechazar la operación por documento duplicado
- **AND** el mensaje de error SHALL indicar el documento en conflicto

---

### Requirement: Validez y unicidad del email

El email SHALL tener un formato válido, entendido como texto con una parte local, el carácter `@` y un dominio con al menos un punto y un sufijo. Valores como `usuario@` o `usuario` MUST ser rechazados.

El email SHALL ser único entre los estudiantes. Al actualizar, reenviar el mismo email que el estudiante ya tenía SHALL considerarse válido.

El mensaje de error por duplicado al actualizar SHALL tener la forma `El email <email> ya está registrado por otro estudiante.`

#### Scenario: Email con formato válido

- **WHEN** se registra un estudiante con el email `usuario@dominio.com`
- **THEN** el sistema SHALL aceptar el email

#### Scenario: Email sin dominio

- **WHEN** se intenta registrar un estudiante con el email `usuario@`
- **THEN** el sistema SHALL rechazar la operación con estado `422`
- **AND** el detalle del error SHALL identificar el email como inválido

#### Scenario: Email sin arroba

- **WHEN** se intenta registrar un estudiante con el email `usuario`
- **THEN** el sistema SHALL rechazar la operación con estado `422`
- **AND** el detalle del error SHALL identificar el email como inválido

#### Scenario: Email duplicado al registrar

- **WHEN** se intenta registrar un estudiante con un email que ya pertenece a otro estudiante
- **THEN** el sistema SHALL rechazar la operación
- **AND** MUST NOT persistir ningún registro

#### Scenario: Email duplicado al actualizar

- **WHEN** se intenta actualizar un estudiante asignándole el email `juan.perez@dominio.com`, que pertenece a un estudiante distinto
- **THEN** el sistema SHALL rechazar la operación
- **AND** el mensaje de error SHALL ser `El email juan.perez@dominio.com ya está registrado por otro estudiante.`
- **AND** los datos del estudiante MUST permanecer sin cambios

#### Scenario: Email sin cambios al actualizar

- **WHEN** se actualiza un estudiante enviando el mismo email que ya tenía
- **THEN** el sistema SHALL aceptar la operación

---

### Requirement: Auditoría de creación

Al registrar un estudiante el sistema SHALL almacenar automáticamente `CreatedDate` con la fecha y hora UTC de la operación y `CreatedBy` con el valor `system`.

Los demás campos de auditoría SHALL quedar sin valor: `LastModifiedDate` y `LastModifiedBy` en nulo, `IsDeleted` en `false`, `DeleteDate` y `DeletedBy` en nulo.

#### Scenario: Sello de creación aplicado

- **WHEN** se registra un estudiante correctamente
- **THEN** `CreatedDate` SHALL contener la fecha y hora UTC de la operación
- **AND** `CreatedBy` SHALL ser `system`

#### Scenario: Resto de auditoría sin valor tras la creación

- **WHEN** se registra un estudiante correctamente
- **THEN** `LastModifiedDate`, `LastModifiedBy`, `DeleteDate` y `DeletedBy` SHALL ser nulos
- **AND** `IsDeleted` SHALL ser `false`

#### Scenario: Sin sello de auditoría cuando la validación falla

- **WHEN** un registro se rechaza por una regla de validación o de unicidad
- **THEN** el sistema MUST NOT persistir ningún registro ni campo de auditoría

---

### Requirement: Listado paginado de estudiantes

El sistema SHALL exponer el listado de estudiantes de forma paginada, devolviendo para cada estudiante su documento, nombre, apellido y email, junto con los metadatos de paginación (página actual, tamaño de página, total de registros y total de páginas).

El tamaño de página SHALL estar comprendido entre 10 y 50 inclusive; un valor fuera de ese rango SHALL rechazarse con un error de paginación.

El listado MUST NOT incluir estudiantes eliminados.

#### Scenario: Listado con resultados paginados

- **WHEN** se solicita el listado indicando página y tamaño de página
- **THEN** el sistema SHALL devolver como máximo el número de estudiantes indicado por el tamaño de página
- **AND** SHALL incluir los metadatos de paginación con el total de registros no eliminados

#### Scenario: Estudiantes eliminados excluidos del listado

- **WHEN** se solicita el listado y existen estudiantes eliminados lógicamente
- **THEN** los estudiantes eliminados MUST NOT aparecer en los resultados
- **AND** MUST NOT contarse en el total de registros

#### Scenario: Listado sin resultados

- **WHEN** se solicita la primera página y no existe ningún estudiante no eliminado
- **THEN** el sistema SHALL responder con éxito y una colección vacía
- **AND** MUST NOT devolver un error de paginación

#### Scenario: Última página exacta

- **WHEN** se solicita la página cuyo número coincide exactamente con el total de páginas disponibles
- **THEN** el sistema SHALL devolver los estudiantes restantes de esa página
- **AND** MUST NOT devolver un error de paginación

#### Scenario: Página posterior a la última

- **WHEN** se solicita una página cuyo número excede el total de páginas disponibles
- **THEN** el sistema SHALL rechazar la operación con un error de paginación

#### Scenario: Tamaño de página en los extremos admitidos

- **WHEN** se solicita el listado con un tamaño de página de 10, y luego con un tamaño de 50
- **THEN** el sistema SHALL aceptar ambas solicitudes

#### Scenario: Tamaño de página por debajo del mínimo

- **WHEN** se solicita el listado con un tamaño de página menor que 10
- **THEN** el sistema SHALL rechazar la operación con un error de paginación

#### Scenario: Tamaño de página por encima del máximo

- **WHEN** se solicita el listado con un tamaño de página mayor que 50
- **THEN** el sistema SHALL rechazar la operación con un error de paginación

---

### Requirement: Filtros del listado de estudiantes

El listado SHALL aceptar filtros opcionales e independientes por documento, apellido y email. Los filtros SHALL aplicarse por coincidencia parcial, sin distinguir mayúsculas de minúsculas pero distinguiendo acentos, conforme a la collation `Modern_Spanish_CI_AS` que la base de datos aplica. Los filtros SHALL poder combinarse, en cuyo caso el resultado SHALL satisfacer todos los filtros indicados.

Los filtros SHALL aplicarse antes de la paginación, de modo que los metadatos reflejen el total de registros filtrados.

#### Scenario: Filtro por documento

- **WHEN** se solicita el listado filtrando por un fragmento de documento
- **THEN** el resultado SHALL contener únicamente estudiantes cuyo documento contenga ese fragmento

#### Scenario: Filtro por apellido sin distinguir mayúsculas

- **WHEN** se solicita el listado filtrando por el apellido `gomez` y existe un estudiante con apellido `GOMEZ`
- **THEN** ese estudiante SHALL aparecer en el resultado

#### Scenario: Filtro por apellido distinguiendo acentos

- **WHEN** se solicita el listado filtrando por el apellido `perez` y el único estudiante candidato tiene el apellido `Pérez`
- **THEN** ese estudiante MUST NOT aparecer en el resultado
- **AND** filtrar por `Pérez` SHALL devolverlo

#### Scenario: Filtro que no coincide con ningún estudiante

- **WHEN** se solicita el listado con un filtro que ningún estudiante satisface
- **THEN** el sistema SHALL responder con éxito y una colección vacía
- **AND** el total de registros SHALL ser cero

#### Scenario: Filtros combinados

- **WHEN** se solicita el listado filtrando simultáneamente por apellido y por email
- **THEN** el resultado SHALL contener únicamente estudiantes que satisfagan ambos filtros

#### Scenario: Total de registros coherente con el filtro

- **WHEN** se solicita el listado con un filtro aplicado
- **THEN** el total de registros de los metadatos SHALL corresponder al número de estudiantes que satisfacen el filtro, no al total general

#### Scenario: Sin filtros indicados

- **WHEN** se solicita el listado sin indicar ningún filtro
- **THEN** el sistema SHALL devolver todos los estudiantes no eliminados, paginados

---

### Requirement: Consulta de estudiante por identificador

El sistema SHALL permitir consultar el detalle de un estudiante por su `Id`, devolviendo `Id`, documento, nombre, apellido y email.

Si el `Id` no corresponde a ningún estudiante, o corresponde a un estudiante eliminado, el sistema SHALL responder con estado `404` y el mensaje `No existe un estudiante con el Id indicado.`

#### Scenario: Estudiante existente

- **WHEN** se consulta un estudiante por el `Id` de un estudiante no eliminado
- **THEN** el sistema SHALL devolver su `Id`, documento, nombre, apellido y email

#### Scenario: Identificador inexistente

- **WHEN** se consulta un estudiante con un `Id` que no existe
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

#### Scenario: Estudiante eliminado no consultable

- **WHEN** se consulta un estudiante que fue eliminado lógicamente
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

---

### Requirement: Actualización de estudiante

El sistema SHALL permitir actualizar documento, nombre, apellido y email de un estudiante identificado por su `Id`. El `Id` y los campos de auditoría MUST NOT ser editables.

Los cuatro campos editables MUST seguir siendo obligatorios: no se acepta enviarlos vacíos o nulos. Se aplican las mismas reglas de unicidad de documento y de validez y unicidad de email que en el registro.

Tras una actualización exitosa el sistema SHALL devolver el estudiante con sus datos actualizados junto con un mensaje de confirmación.

#### Scenario: Actualización exitosa

- **WHEN** se actualiza un estudiante existente con documento, nombre, apellido y email válidos
- **THEN** el sistema SHALL persistir los nuevos valores
- **AND** SHALL devolver el estudiante actualizado con un mensaje de confirmación

#### Scenario: Identificador inexistente al actualizar

- **WHEN** se intenta actualizar un estudiante con un `Id` que no existe
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

#### Scenario: Estudiante eliminado no actualizable

- **WHEN** se intenta actualizar un estudiante que fue eliminado lógicamente
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

#### Scenario: Campo obligatorio vacío al actualizar

- **WHEN** se intenta actualizar un estudiante enviando el nombre vacío
- **THEN** el sistema SHALL rechazar la operación con estado `422`
- **AND** los datos del estudiante MUST permanecer sin cambios

#### Scenario: Identificador no modificable

- **WHEN** se actualiza un estudiante
- **THEN** el `Id` del estudiante persistido MUST permanecer igual al indicado en la petición

---

### Requirement: Auditoría de modificación

Tras una actualización exitosa el sistema SHALL almacenar automáticamente `LastModifiedDate` con la fecha y hora UTC de la operación y `LastModifiedBy` con el valor `system`.

`CreatedDate` y `CreatedBy` MUST permanecer inmutables. Si la operación falla por validación, el sistema MUST NOT alterar ningún campo de auditoría.

#### Scenario: Sello de modificación aplicado

- **WHEN** se actualiza un estudiante correctamente
- **THEN** `LastModifiedDate` SHALL contener la fecha y hora UTC de la operación
- **AND** `LastModifiedBy` SHALL ser `system`

#### Scenario: Datos de creación inmutables

- **WHEN** se actualiza un estudiante correctamente
- **THEN** `CreatedDate` y `CreatedBy` SHALL conservar los valores asignados en el registro

#### Scenario: Auditoría intacta cuando la actualización falla

- **WHEN** una actualización se rechaza por validación o por unicidad
- **THEN** ningún campo de auditoría del estudiante MUST cambiar

---

### Requirement: Eliminación lógica de estudiante

El sistema SHALL permitir eliminar un estudiante identificado por su `Id`. La eliminación SHALL implementarse como borrado lógico: el registro SHALL conservarse en la base de datos con `IsDeleted` en `true` y MUST NOT borrarse físicamente.

Una vez eliminado, el estudiante SHALL dejar de existir para todos los efectos del sistema: no aparece en el listado, no es consultable por `Id` y no puede actualizarse.

Si el `Id` no existe, el sistema SHALL responder con estado `404` y el mensaje `No existe un estudiante con el Id indicado.`

#### Scenario: Eliminación exitosa

- **WHEN** se elimina un estudiante existente
- **THEN** el sistema SHALL marcar el registro con `IsDeleted` en `true`
- **AND** el registro SHALL seguir presente físicamente en la base de datos
- **AND** el sistema SHALL devolver una confirmación de la operación

#### Scenario: Identificador inexistente al eliminar

- **WHEN** se intenta eliminar un estudiante con un `Id` que no existe
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

#### Scenario: Eliminación no repetible

- **WHEN** se intenta eliminar un estudiante que ya fue eliminado
- **THEN** el sistema SHALL responder con estado `404`
- **AND** el mensaje SHALL ser `No existe un estudiante con el Id indicado.`

#### Scenario: Invisibilidad tras la eliminación

- **WHEN** un estudiante ha sido eliminado
- **THEN** MUST NOT aparecer en el listado paginado
- **AND** su consulta por `Id` SHALL responder `404`
- **AND** su actualización SHALL responder `404`

---

### Requirement: Auditoría de eliminación

Al eliminar un estudiante el sistema SHALL almacenar automáticamente `DeleteDate` con la fecha y hora UTC de la operación y `DeletedBy` con el valor `system`, además de `IsDeleted` en `true`.

#### Scenario: Sello de eliminación completo

- **WHEN** se elimina un estudiante correctamente
- **THEN** `IsDeleted` SHALL ser `true`
- **AND** `DeleteDate` SHALL contener la fecha y hora UTC de la operación
- **AND** `DeletedBy` SHALL ser `system`

#### Scenario: Datos de creación preservados tras eliminar

- **WHEN** se elimina un estudiante correctamente
- **THEN** `CreatedDate` y `CreatedBy` SHALL conservar sus valores originales

---

### Requirement: Exclusión sistemática de registros eliminados

El sistema SHALL excluir los estudiantes con `IsDeleted` en `true` de toda consulta de lectura de forma predeterminada, mediante un mecanismo aplicado en la capa de persistencia y no delegado a cada consulta individual.

Las operaciones que verifican unicidad de documento y email SHALL considerar también los registros eliminados, de modo que un documento o email usado por un estudiante eliminado no pueda reutilizarse.

#### Scenario: Consulta nueva excluye eliminados sin código adicional

- **WHEN** se agrega una consulta de lectura sobre estudiantes sin incluir una condición explícita sobre `IsDeleted`
- **THEN** los estudiantes eliminados MUST NOT aparecer en su resultado

#### Scenario: Conteo excluye eliminados

- **WHEN** se calcula el total de registros para la paginación
- **THEN** el total MUST NOT incluir estudiantes eliminados

#### Scenario: Unicidad considera eliminados

- **WHEN** se valida la unicidad de un documento o email
- **THEN** la verificación SHALL alcanzar también a los estudiantes eliminados lógicamente

---

### Requirement: Verificación automatizada de los criterios de aceptación

Cada escenario de esta especificación SHALL estar cubierto por al menos una prueba automatizada en el proyecto `tests/Haskoli.NTest`.

Los escenarios cuyo resultado depende del motor de base de datos —la coincidencia de los filtros bajo la collation y el alcance de los índices únicos— SHALL verificarse contra SQL Server, y MUST NOT sustituirse por un motor que no reproduzca ese comportamiento.

Las pruebas que ejercitan la persistencia SHALL partir de un estado conocido, dejando la tabla de estudiantes vacía antes de cada prueba, y MUST NOT alterar los datos de referencia de otras tablas.

Las pruebas SHALL nombrarse de forma que identifiquen la historia de usuario, el contexto que ejercitan y el comportamiento esperado, y SHALL agruparse por historia de usuario y criterio de aceptación.

#### Scenario: Escenarios dependientes del motor verificados contra SQL Server

- **WHEN** se ejecutan las pruebas que cubren el filtro insensible a mayúsculas, el filtro sensible a acentos y la unicidad de email
- **THEN** SHALL ejecutarse contra una base con la collation `Modern_Spanish_CI_AS`
- **AND** su resultado SHALL reflejar el comportamiento real del motor, no el de un sustituto

#### Scenario: Aislamiento entre pruebas de persistencia

- **WHEN** se ejecuta la suite completa
- **THEN** cada prueba de persistencia SHALL partir de una tabla de estudiantes vacía
- **AND** el resultado de una prueba MUST NOT depender del orden en que se ejecuten

#### Scenario: Trazabilidad entre criterio y prueba

- **WHEN** se revisa un escenario de esta especificación
- **THEN** SHALL poder identificarse la prueba que lo verifica a partir del nombre de su agrupación y de su método

#### Scenario: Fallo visible ante una regresión

- **WHEN** se altera el comportamiento descrito por cualquier escenario de esta especificación
- **THEN** al menos una prueba SHALL fallar
