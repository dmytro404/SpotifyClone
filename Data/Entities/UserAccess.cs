using SpotifyClone.Data.Entities;

namespace SpotifyClone.Data.Entities
{
    public class UserAccess
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public String RoleId { get; set; } = null!;
        public String Login { get; set; } = null!;
        public String Salt { get; set; } = null!;
        public String Dk { get; set; } = null!;  // DK derived key RFC 8018 


        public User User { get; set; } = null!;
        public UserRole Role { get; set; } = null!;
    }
}