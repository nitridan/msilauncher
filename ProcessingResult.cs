using System.Collections.Generic;
namespace Nitridan.MsiLauncher
{
    public class ProcessingResult
    {
        public List<PropertyItem> ChangedProperties { get; set; }

        public List<PropertyItem> AddedProperties { get; set; }

        public List<PropertyItem> RemovedProperties { get; set; }
        
        public List<DirectoryItem> Directories { get; set; }
        
        public List<string> Errors { get; set; }

        public ProcessingResult()
        {
            ChangedProperties = new List<PropertyItem>();
            AddedProperties = new List<PropertyItem>();
            RemovedProperties = new List<PropertyItem>();
            Directories = new List<DirectoryItem>();
            Errors = new List<string>();
        }
    }
}
