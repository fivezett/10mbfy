using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
namespace webpfy
{
    internal class Core
    {
        public event Action<int, int> ProgressEventHandler;

        public WebpEncoder Encoder = new WebpEncoder
        {
            FileFormat = WebpFileFormatType.Lossy,
            Quality = 100
        };

        public static string outputDir
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            }
        }

        public async Task<string> ConvertToWebp(string[] paths, CancellationToken token)
        {
            int cnt = 0;
            int total = paths.Length;
            ProgressEventHandler?.Invoke(cnt, total);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string outputDateDir = Path.Combine(outputDir, timestamp);
            try
            {
                if (!Directory.Exists(outputDateDir))
                {
                    Directory.CreateDirectory(outputDateDir);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return "";
            }

            Dictionary<string, List<string>> filenameMap = new();
            foreach (string path in paths)
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                if (!filenameMap.ContainsKey(filename))
                {
                    filenameMap[filename] = new List<string>() { path };
                }
                else
                {
                    filenameMap[filename].Add(path);
                }
            }
            Dictionary<string, string> pathDict = new();
            foreach (string filename in filenameMap.Keys)
            {
                List<string> filename_paths = filenameMap[filename];
                if (filename_paths.Count == 1)
                {
                    pathDict[filename_paths[0]] = Path.Combine(outputDateDir, filename);
                }
                else
                {
                    for (int i = 0; i < filename_paths.Count; i++)
                    {
                        pathDict[filename_paths[i]] = Path.Combine(outputDateDir, filename + $"_{i + 1}");
                    }
                }
            }
            await Task.Run(() =>
            {
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Math.Max((int)Math.Ceiling(Environment.ProcessorCount * 0.8f), 1)
                };
                Parallel.ForEach(pathDict, parallelOptions, pathPair =>
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(pathPair.Key))
                    {
                        image.SaveAsWebpAsync(pathPair.Value + ".webp", Encoder, token).Wait();
                        ProgressEventHandler?.Invoke(++cnt, total);
                    }
                });
            });
            GC.Collect();
            if (token.IsCancellationRequested)
            {
                Directory.Delete(outputDateDir, true);
                throw new TaskCanceledException();
            }
            return outputDateDir;
        }
    }
}
