namespace Haskoli.Application.Constants
{
    /// <summary>
    /// Textos que el PRD fija de forma literal. Viven aquí, y no incrustados en cada handler,
    /// para que el mensaje que verifica una prueba y el que devuelve la API sean el mismo.
    /// </summary>
    public static class StudentMessages
    {
        public static string DocumentAlreadyRegistered(string document) =>
            $"Ya existe un estudiante registrado con el documento {document}.";

        /* La especificación fija este texto para la actualización; se reutiliza al registrar,
           donde solo exige rechazar la operación, para no dar dos mensajes al mismo conflicto. */
        public static string EmailAlreadyRegistered(string email) =>
            $"El email {email} ya está registrado por otro estudiante.";

        public const string NotFound = "No existe un estudiante con el Id indicado.";

        public const string Created = "Estudiante registrado correctamente.";

        public const string Updated = "Estudiante actualizado correctamente.";

        public const string Deleted = "Estudiante eliminado correctamente.";
    }
}
