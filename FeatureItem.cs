using Microsoft.Deployment.WindowsInstaller;

namespace Nitridan.MsiLauncher
{
    public class FeatureItem
    {
        public string Feature { get; set; }
        
        public string FeatureParent { get; set; }
        
        public int Level { get; set; }
        
        public InstallState InstallState { get; set; }
        
        public InstallState ActionState { get; set; }        
    }
}