namespace Haskoli.Domain.Parameters
{
    /// <summary>
    /// Deliberadamente no recorta PageSize: el rango admitido (10 a 50) lo hace cumplir
    /// CRUDService.GetPagedAsync lanzando PageRowMinimumException o PageRowMaximumException,
    /// porque la especificación exige rechazar un tamaño fuera de rango en lugar de ajustarlo
    /// en silencio. Por eso tampoco se delega en el constructor con parámetros de
    /// RequestParameter, que trunca cualquier valor mayor que 10.
    /// </summary>
    public class GetAllStudentParameter : RequestParameter
    {
        public string? Document { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}
