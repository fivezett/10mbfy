namespace webpfy
{
    internal class Pickup
    {
        public static List<string> pickup(string[] files)
        {
            List<string> fileList = new List<string>(files);
            List<string> additionalFiles = new List<string>();
            foreach (var file in fileList)
            {
                additionalFiles.AddRange(pickupInChildren(file));
            }
            fileList.AddRange(additionalFiles);
            fileList = fileList.Where(e => isPicture(e) != "").Distinct().ToList();
            return fileList;
        }

        private static List<string> pickupInChildren(string filePath)
        {
            return (
                isDirectory(filePath)
                ? Directory.GetFiles(filePath, "*", SearchOption.AllDirectories).ToList()
                : new List<string>()
            );
        }

        private static bool isDirectory(string filePath)
        {
            return File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
        }

        private static string isPicture(string filePath)
        {
            string fileEx = Path.GetExtension(filePath).ToLower();
            switch (fileEx)
            {
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".bmp":
                case ".gif":
                case ".webp":
                    return fileEx;
                default:
                    return "";
            }
        }

    }
}
