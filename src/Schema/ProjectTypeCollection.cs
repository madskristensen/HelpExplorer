using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HelpExplorer.Schema
{
    public class ProjectTypeCollection
    {
        private ProjectTypeCollection() { }

        public ProjectType[] ProjectTypes { get; set; }

        public static async Task<ProjectTypeCollection> LoadAsync()
        {
            var dir = Path.GetDirectoryName(typeof(ProjectTypeCollection).Assembly.Location);
            var file = Path.Combine(dir, "schema", "projecttypes.json");

            using (var reader = new StreamReader(file))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<ProjectTypeCollection>(json);
            }
        }
    }

    public class ProjectType
    {
        public string Capability { get; set; }
        public string CapabilityExpression { get; set; }
        public string CapabilitiesFileName { get; set; }
        public string Text { get; set; }
        public Link[] Links { get; set; }
    }

    public class Link
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }

}
