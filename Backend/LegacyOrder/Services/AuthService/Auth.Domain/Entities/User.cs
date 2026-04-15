namespace Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        
        public string Name { get; set; } = default!;
        public string Surname { get; set; } = default!;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; }
    }
}