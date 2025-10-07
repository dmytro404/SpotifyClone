namespace SpotifyClone.Services.Kdf
{
    public interface IKdfService
    {
        String Dk(String password, String salt);
    }
}
