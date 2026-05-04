namespace SpotifyClone.Services.Kdf
{
    // Key Derivation Functions Service by RFC 8018 (PBKDF2, 2018)
    public interface IKdfService
    {
        string Dk(string password, string salt);
    }
}