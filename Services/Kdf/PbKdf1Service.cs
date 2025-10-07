namespace SpotifyClone.Services.Kdf
{
    //Implementation by sec.5.1 RFC 2898

    public class PbKdf1Service : IKdfService
    {
        const int c = 3;
        const int dkLen = 20;

        public String Dk(String password, String salt)
        {
            String t = Hash(password + salt);
            for (int i = 0; i < c - 1; i++)
            {
                t = Hash(t);
            }
            return t[..dkLen];
        }

        private static String Hash(String input) => Convert.ToHexString(
            System.Security.Cryptography.SHA1.HashData(
                System.Text.Encoding.UTF8.GetBytes(input)));
    }
}
