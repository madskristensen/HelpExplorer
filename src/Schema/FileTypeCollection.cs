using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HelpExplorer.Schema
{
    public class FileTypeCollection
    {
        private FileTypeCollection() { }

        public FileType[] FileTypes { get; set; }

        public static async Task<FileTypeCollection> LoadAsync()
        {
            var dir = Path.GetDirectoryName(typeof(FileTypeCollection).Assembly.Location);
            var file = Path.Combine(dir, "schema", "filetypes.json");

            using (var reader = new StreamReader(file))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<FileTypeCollection>(json);
            }
        }
    }

    public class FileType
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Link[] Links { get; set; }
    }
}
