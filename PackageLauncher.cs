using System;
using System.Collections.Generic;
using System.Linq;

namespace Nitridan.MsiLauncher
{
    public static class PackageLauncher
    {
        public static ProcessingResult LaunchMsi(string msiPath)
        {
            using (var package = new MsiPackage(msiPath))
            {
                var errors = new List<string>();
                var session = package.Session;
                session["Installed"] = null;
                var directories = package.GetDirectories();
                var initialProperties = package.GetRuntimeProperties();
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

                var finalProperties = package.GetRuntimeProperties();
                var result = CompareProperties(initialProperties.ToPropertyDictionary(), finalProperties.ToPropertyDictionary());
                result.Errors = errors;
                result.Directories = directories.ToList();
                return result;
            }
        }

        private static ProcessingResult CompareProperties(IDictionary<string, string> initialProperties, 
            IDictionary<string, string> finalProperties)
        {
            var result = new ProcessingResult();
            var finalKeys = finalProperties.Keys.ToArray();
            foreach (var key in finalKeys)
            {
                var finalProperty = finalProperties[key];
                var propertyItem = new PropertyItem { Property = key, Value = finalProperty };
                if (!initialProperties.ContainsKey(key))
                {
                    result.AddedProperties.Add(propertyItem);
                }
                else
                {
                    if (initialProperties[key] != finalProperties[key])
                    {
                        result.ChangedProperties.Add(propertyItem);
                    }

                    initialProperties.Remove(key);
                }
            }

            result.RemovedProperties.AddRange(
                initialProperties.Select(x => new PropertyItem { Property = x.Key, Value = x.Value }));
            return result;
        }
    }
}
