namespace SpotifyClone.Data.Entities
{
    public class UserAccess
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string RoleId { get; set; } = null!;
        public UserRole Role { get; set; } = null!;

        public string Login { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Dk { get; set; } = null!;
    }
}