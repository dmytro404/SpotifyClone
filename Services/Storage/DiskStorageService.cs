namespace SpotifyClone.Services.Storage
{
    public class DiskStorageService : IStorageService
    {
        private const string path = "C:/storage/ASP32/";
        private static readonly string[] allowedExtensions = [".jpg", ".png", ".jpeg", ".mp3", ".wav", ".flac"];

        public byte[]? Load(string filename)
        {
            string fullName = Path.Combine(path, filename);
            return File.Exists(fullName) ? File.ReadAllBytes(fullName) : null;
        }

        public string Save(IFormFile file)
        {
            int dotIndex = file.FileName.LastIndexOf('.');
            if (dotIndex == -1)
                throw new ArgumentException("File name must have an extension");

            string ext = file.FileName[dotIndex..].ToLower();
            if (!allowedExtensions.Contains(ext))
                throw new ArgumentException($"File extension '{ext}' not supported");

            Directory.CreateDirectory(path);

            string filename = Guid.NewGuid().ToString() + ext;
            using FileStream stream = new(Path.Combine(path, filename), FileMode.Create);
            file.CopyTo(stream);

            return filename;
        }
    }
}
