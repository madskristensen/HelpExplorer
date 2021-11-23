using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace HelpExplorer
{
    internal partial class OptionsProvider
    {
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("Display Multiple Types")]
        [DisplayName("Display Multiple Project Types")]
        [Description("Allows you to display multiple project types and links in the HelpExplorer Window or the first one found for your selected project.")]
        [DefaultValue(false)]
        public bool MultipleProjectsOption { get; set; } = false;

        [Category("Display Multiple Types")]
        [DisplayName("Display Multiple File Types")]
        [Description("Allows you to display multiple file types and links in the HelpExplorer Window or the first one found for your selected file.")]
        [DefaultValue(false)]
        public bool MultipleFilesOption { get; set; } = false;
#if DEBUG
        [Category("Capabilities")]
        [DisplayName("Enable Local file creation")]
        [Description("Allows you to enable or disable the export active project capabilities to a file. Then you can easily open and copy capabilities to the projecttypes.json file.")]
        [DefaultValue(true)]
        public bool CreateCapabilitiesFile { get; set; } = true;

        [Category("Capabilities")]
        [DisplayName("Local file path")]
        [Description("Allows you to export active project capabilities to a file. Then you can easily open and copy capabilities to the projecttypes.json file.")]
        [DefaultValue(@"C:\temp\Capabilities")]
        public string CapabilitiesFilePathOption { get; set; } = @"C:\temp\Capabilities";
#endif
    }
}
