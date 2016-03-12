using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace Nitridan.MsiLauncher
{
    public interface IPackageLauncher
    {
        ProcessingResult LaunchMsi(string msiPath);
    }
    
    internal interface IActionResult
    {
    }
    
    internal class ActionError : IActionResult
    {
        public string Message { get; set; }
    }
    
    internal class ActionExecuted : IActionResult
    {
    }
    
    internal class ActionCostsFinalized : IActionResult
    {
        public IDictionary<string, string> Properties { get; set; }
    }
    
    public class PackageLauncher : IPackageLauncher
    {        
        public ProcessingResult LaunchMsi(string msiPath)
        {
            using (var package = new MsiPackage(msiPath))
            {
                var session = package.Session;
                session["Installed"] = null;
                var directories = package.GetDirectories();
                var initialProperties = package.GetRuntimeProperties().ToPropertyDictionary();
                var sequenceItems = package.GetUiSequence()
                    .Where(x => string.IsNullOrWhiteSpace(x.Condition) || session.EvaluateCondition(x.Condition))
                    .TakeWhile(x => x.Action != "ExecuteAction");
                var results = sequenceItems.Select(x => ExecuteAction(package, x.Action)).ToArray();                
                var finalProperties = package.GetRuntimeProperties().ToPropertyDictionary();
                var result = new ProcessingResult 
                {
                    AddedProperties = GetAddedProperties(initialProperties, finalProperties).ToList(),
                    ChangedProperties = GetChangedProperties(initialProperties, finalProperties).ToList(),
                    Errors = results.OfType<ActionError>().Select(x => x.Message).ToList(),
                    Directories = directories.ToList(),
                    Features = PopulateStates(package.GetFeatures(), session).ToList()
                };
                
                var initialDirectories = results.OfType<ActionCostsFinalized>().SingleOrDefault();
                if (initialDirectories != null)
                {
                    result.ChangedDirectories = GetChangedDirectoryProperties(initialDirectories.Properties, 
                                                                                 finalProperties, directories).ToList();     
                }
               
                return result;
            }
        }
        
        private static IActionResult ExecuteAction(MsiPackage package, string name)
        {
            try
            {
                package.Session.DoAction(name);
                return name != "CostFinalize"
                    ? (IActionResult)new ActionExecuted()
                    : new ActionCostsFinalized { Properties = package.GetRuntimeProperties().ToPropertyDictionary() };
            }
            catch (Exception ex)
            {
                return new ActionError {Message = $"Action '{name}' failed with message: '{ex.Message}'"};
            }
        }
        
        private static IEnumerable<PropertyItem> GetChangedDirectoryProperties(IDictionary<string, string> initial, 
            IDictionary<string, string> updated,
            IEnumerable<DirectoryItem> directories)
            {
                var directorySet = directories.Select(x => x.Id).ToSet();
                var directoryDictionary =  initial
                    .Where(x => directorySet.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
                
               return updated.Where(x => directoryDictionary.ContainsKey(x.Key) && x.Value != directoryDictionary[x.Key])
                .Select(x => new PropertyItem{Property = x.Key, Value = x.Value});
            }
            
        private static IEnumerable<FeatureItem> PopulateStates(IEnumerable<FeatureItem> features, Session session)
            => features.Select(x => {
                var feature = session.Features[x.Feature];
                    x.InstallState = feature.CurrentState;
                    x.ActionState = feature.RequestState;
                    return x;
            });
        
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
