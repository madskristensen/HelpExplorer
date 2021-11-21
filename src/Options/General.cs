using System.ComponentModel;

namespace HelpExplorer
{
    internal partial class OptionsProvider
    {
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("Project Types")]
        [DisplayName("Display Multiple Project Types")]
        [Description("Allows you to display multiple project types and links in the HelpExplorer Window or the first one found for your selected project.")]
        [DefaultValue(false)]
        public bool MultipleProjectsOption { get; set; } = false;

        [Category("File Types")]
        [DisplayName("Display Multiple File Types")]
        [Description("Allows you to display multiple file types and links in the HelpExplorer Window or the first one found for your selected file.")]
        [DefaultValue(false)]
        public bool MultipleFilesOption { get; set; } = false;

        [Category("Capabilities")]
        [DisplayName("Local file path to write project capabilities file")]
        [Description("Allows you to export active project capabilities to a file. Then you can easily open and copy capabilities to the projecttypes.json file.")]
        [DefaultValue(@"C:\temp\Capabilities")]
        public string CapabilitiesFilePathOption { get; set; } = @"C:\temp\Capabilities";
    }
}
