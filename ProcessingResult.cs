using System.Collections.Generic;
namespace Evil.MsiLauncher
{
    public class ProcessingResult
    {
        public List<PropertyItem> ChangedProperties { get; set; }

        public List<PropertyItem> AddedProperties { get; set; }

        public List<PropertyItem> RemovedProperties { get; set; }

        public ProcessingResult()
        {
            ChangedProperties = new List<PropertyItem>();
            AddedProperties = new List<PropertyItem>();
            RemovedProperties = new List<PropertyItem>();
        }
    }
}
