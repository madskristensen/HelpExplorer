using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HelpExplorer.Schema
{
    public class Projects
    {
        private Projects() { }

        public Widget[] Widgets { get; set; }

        public static async Task<Projects> LoadAsync()
        {
            var dir = Path.GetDirectoryName(typeof(Projects).Assembly.Location);
            var file = Path.Combine(dir, "schema", "ProjectSdkStyle.json");

            using (var reader = new StreamReader(file))
            {
                var json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<Projects>(json);
            }
        }
    }

    //public class Widget
    //{
    //    public string[] Projects { get; set; }
    //    public string Text { get; set; }
    //    public Link[] Links { get; set; }
    //}

    //public class Link
    //{
    //    public string Text { get; set; }
    //    public string Url { get; set; }
    //}
    //public class Rootobject
    //{
    //    public Widget[] widgets { get; set; }
    //}

    public class Widget
    {
        public ProjectType[] Projects { get; set; }
        public string Text { get; set; }
        public Link[] Links { get; set; }
    }

    public class ProjectType
    {
        public string Guid { get; set; }
        public string Language { get; set; }
        public string Platform { get; set; }
        public string ProjectTypeExpression { get; set; }
        public string FrameworkType { get; set; }
    }

    public class Link
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
