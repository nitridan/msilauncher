using System.Collections.Generic;

namespace Nitridan.MsiLauncher
{
    public class MsiChanges
    {
        public List<string> FeaturesToInstall { get; set; }
        
        public List<string> FeaturesToDisable { get; set; }
        
        public List<PropertyItem> PropertiesToWrite { get; set; }
        
        public List<PropertyItem> ChangedDirectories { get; set; }
    }
}