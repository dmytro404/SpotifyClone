
namespace SpotifyClone.Services.Storage
{
    public class DiskStorageService : IStorageService
    {
        private const String path = "C:/storage/ASP32/";
        private static readonly String[] allowedExtensions = [".jpg", ".png", ".jpeg"];

        public byte[]? Load(string filename)
        {
            String fullName = Path.Combine(path, filename);
            if (File.Exists(fullName))
            {
                return File.ReadAllBytes(fullName);
            }
            else return null;
        }

        public string Save(IFormFile file)
        {
            int dotIndex = file.FileName.LastIndexOf('.');
            if (dotIndex == -1)
            {
                throw new ArgumentException("File name must have an extension");
            }
            String ext = file.FileName[dotIndex..].ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                throw new ArgumentException($"File extension '{ext}' not supported");
            }
            String filename = Guid.NewGuid().ToString() + ext;

            using FileStream fileStream = new(
                Path.Combine(path, filename),
                FileMode.Create);

            file.CopyTo(fileStream);

            return filename;
        }
    }
}
