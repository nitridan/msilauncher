﻿using System;
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
                var initialProperties = package.GetRuntimeProperties();
                var sequenceItems = package.GetUiSequence();
                foreach (var sequenceItem in sequenceItems)
                {
                    if (sequenceItem.Action == "ExecuteAction")
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(sequenceItem.Condition) 
                        || session.EvaluateCondition(sequenceItem.Condition))
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
                }

                var finalProperties = package.GetRuntimeProperties();
                var result = CompareProperties(initialProperties, finalProperties);
                result.Errors = errors;
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
