using System;
using System.Collections.Generic;
using System.Linq;

namespace Nitridan.MsiLauncher
{
    public interface IPackageLauncher
    {
        ProcessingResult LaunchMsi(string msiPath);
    }
    
    public class PackageLauncher : IPackageLauncher
    {
        public ProcessingResult LaunchMsi(string msiPath)
        {
            using (var package = new MsiPackage(msiPath))
            {
                var errors = new List<string>();
                var session = package.Session;
                session["Installed"] = null;
                var directories = package.GetDirectories();
                var initialProperties = package.GetRuntimeProperties().ToPropertyDictionary();
                var sequenceItems = package.GetUiSequence()
                    .Where(x => string.IsNullOrWhiteSpace(x.Condition) || session.EvaluateCondition(x.Condition))
                    .TakeWhile(x => x.Action != "ExecuteAction");
                foreach (var sequenceItem in sequenceItems)
                {
                    try
                    {
                        session.DoAction(sequenceItem.Action);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
                
                var finalProperties = package.GetRuntimeProperties().ToPropertyDictionary();
                return new ProcessingResult 
                {
                    AddedProperties = GetAddedProperties(initialProperties, finalProperties).ToList(),
                    RemovedProperties = GetAddedProperties(finalProperties, initialProperties).ToList(),
                    ChangedProperties = GetChangedProperties(initialProperties, finalProperties).ToList(),
                    Errors = errors,
                    Directories = directories.ToList(),
                    Features = package.GetFeatures().ToList()
                };
            }
        }
        
        private static IEnumerable<PropertyItem> GetAddedProperties(IDictionary<string, string> initial,
            IDictionary<string, string> final)
            => final.Where(x => !initial.ContainsKey(x.Key))
                .Select(x => new PropertyItem {Property = x.Key, Value = x.Value});
                
        private static IEnumerable<PropertyItem> GetChangedProperties(IDictionary<string, string> initial,
            IDictionary<string, string> final)
            => final.Where(x => initial.ContainsKey(x.Key) && x.Value == initial[x.Key])
                .Select(x => new PropertyItem {Property = x.Key, Value = x.Value});
    }
}
