using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Deployment.WindowsInstaller.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Nitridan.MsiLauncher
{
    public static class MsiPackageExtensions 
    {
        internal static IEnumerable<FeatureItem> GetFeatures(this MsiPackage package)
            => GetFeatures(package.Database);
        
        public static IEnumerable<FeatureItem> GetFeatures(this Database db)
            => db.AsQueryable().Features.Select(x => new FeatureItem
                {
                   Feature = x.Feature,
                   FeatureParent = x.Feature_Parent,
                   Level = x.Level
                })
                .ToArray();
        
        internal static IEnumerable<SequenceItem> GetUiSequence(this MsiPackage package) 
            => GetUiSequence(package.Database);
        
        public static IEnumerable<SequenceItem> GetUiSequence(this Database db)
            => db.AsQueryable().InstallUISequences
                .OrderBy(x => x.Sequence)
                .Where(x => x.Sequence > 0)
                .Select(x => new SequenceItem
                {
                    Action = x.Action,
                    Condition = x.Condition,
                    Sequence = x.Sequence
                })
                .ToArray();
        
        internal static IEnumerable<DirectoryItem> GetDirectories(this MsiPackage package)
            => GetDirectories(package.Database);
        
        public static IEnumerable<DirectoryItem> GetDirectories(this Database db)
            => db.AsQueryable().Directories
                .Select(x => new DirectoryItem
                {
                    Id = x.Directory,
                    ParentId = x.Directory_Parent,
                    Name = x.DefaultDir
                })
                .ToArray();
        
        internal static IEnumerable<PropertyItem> GetRuntimeProperties(this MsiPackage package)
            => GetRuntimeProperties(package.Database);
        
        public static IEnumerable<PropertyItem> GetRuntimeProperties(this Database db)
        {          
            using (var view = db.OpenView("SELECT * FROM `_Property`"))
            {
                view.Execute();
                return view.Select(x => new PropertyItem
                { 
                    Property = x[1].ToString(), 
                    Value = x[2].ToString()
                })
                .ToArray();
            }
        }
        
        public static IDictionary<string, string> ToPropertyDictionary(this IEnumerable<PropertyItem> properties)
            => properties.ToDictionary(x => x.Property, x => x.Value);
    }
}