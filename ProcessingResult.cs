using System.Collections.Generic;
namespace Nitridan.MsiLauncher
{
    public class ProcessingResult
    {
        public List<PropertyItem> ChangedProperties { get; set; }

        public List<PropertyItem> AddedProperties { get; set; }

        public List<PropertyItem> ChangedDirectories { get; set; }
        
        public List<DirectoryItem> Directories { get; set; }
        
        public List<FeatureItem> Features { get; set; }
        
        public List<string> Errors { get; set; }

        public ProcessingResult()
        {
            ChangedProperties = new List<PropertyItem>();
            AddedProperties = new List<PropertyItem>();
            ChangedDirectories = new List<PropertyItem>();
            Directories = new List<DirectoryItem>();
            Features = new List<FeatureItem>();
            Errors = new List<string>();
        }
    }
}
