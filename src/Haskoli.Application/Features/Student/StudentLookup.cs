using Haskoli.Application.Constants;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Exceptions.Core;
using Haskoli.Domain.Interfaces.Services;

namespace Haskoli.Application.Features.Student
{
    internal static class StudentLookup
    {
        /// <summary>
        /// Comprueba la existencia del estudiante devolviendo el mensaje que fija el PRD.
        /// El servicio genérico produce uno en inglés que nombra el tipo de entidad, y además
        /// su actualización convierte cualquier excepción en <c>MappingNotFoundException</c>,
        /// que acabaría en un 500. Por eso los handlers comprueban antes de delegar.
        /// </summary>
        public static async Task<StudentDTO> FindOrThrowAsync(IStudentService studentService, int id, CancellationToken cancellationToken)
        {
            try
            {
                return await studentService.FindStudent(id, cancellationToken);
            }
            catch (EntityNotFoundException)
            {
                throw new EntityNotFoundException(StudentMessages.NotFound);
            }
        }
    }
}
