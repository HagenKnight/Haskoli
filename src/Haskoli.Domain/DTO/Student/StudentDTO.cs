namespace Haskoli.Domain.DTO
{
    public class StudentDTO
    {
        public int Id { get; set; }
        public string Document { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
