using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HelpExplorer.Schema
{
    public class ContentTypeCollection
    {
        private ContentTypeCollection() { }

        public ContentType[] ContentTypes { get; set; }

        public static async Task<ContentTypeCollection> LoadAsync()
        {
            var dir = Path.GetDirectoryName(typeof(ContentTypeCollection).Assembly.Location);
            var file = Path.Combine(dir, "schema", "contenttypes.json");

            using (var reader = new StreamReader(file))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ContentTypeCollection>(json);
            }
        }
    }

    public class ContentType
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Link[] Links { get; set; }
    }
}
