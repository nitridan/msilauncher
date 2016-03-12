using System.Collections.Generic;

namespace Nitridan.MsiLauncher
{   
    public class MsiChangeDetectorConfiguration 
    {
        public List<string> PermanentlyExcludedProperties { get; set; }
        
        public MsiChangeDetectorConfiguration()
        {
            PermanentlyExcludedProperties = new List<string>();    
        }
    }
}