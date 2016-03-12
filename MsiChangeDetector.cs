using System;
using System.Collections.Generic;
using System.Linq;

namespace Nitridan.MsiLauncher
{
    public class MsiChangeDetector
    {
        private const int MIN_INSTALL_VALUE = 3;
        
        private readonly IPackageLauncher _packageLauncher;
        
        private readonly MsiChangeDetectorConfiguration _configuration;
        
        public MsiChangeDetector(MsiChangeDetectorConfiguration configuration)
            : this(new PackageLauncher(), configuration)
        {
        }
        
        public MsiChangeDetector(IPackageLauncher packageLauncher, MsiChangeDetectorConfiguration configuration)
        {
            if (packageLauncher == null)
            {
                throw new ArgumentNullException(nameof(packageLauncher));    
            }
            
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            
            _packageLauncher = packageLauncher;
            _configuration = configuration;
        }
        
        public MsiChanges LaunchMsiAndDetectChanges(string msiPath)
        {
            var launchResult = this._packageLauncher.LaunchMsi(msiPath);
            return new MsiChanges
            {
                PropertiesToWrite = FilterProperties(launchResult).ToList(),
                FeaturesToInstall = launchResult.Features
                                        .Where(x => (int)x.ActionState >= MIN_INSTALL_VALUE)
                                        .Select(x => x.Feature)
                                        .ToList(),
                FeaturesToDisable = launchResult.Features
                                        .Where(x => (int)x.ActionState < MIN_INSTALL_VALUE)
                                        .Select(x => x.Feature)
                                        .ToList(),
                ChangedDirectories = launchResult.ChangedDirectories
            };
        }
        
        private IEnumerable<PropertyItem> FilterProperties(ProcessingResult result)
            => FilterExclusions(FilterDirectories(result.AddedProperties, result.Directories))
                .Concat(FilterExclusions(FilterDirectories(result.ChangedProperties, result.Directories)));              
        
        private IEnumerable<PropertyItem> FilterExclusions(IEnumerable<PropertyItem> properties)
        {
            var exclusionSet = _configuration.PermanentlyExcludedProperties.ToSet();
            return properties.Where(x => !exclusionSet.Contains(x.Property));
        }
        
        private static IEnumerable<PropertyItem> FilterDirectories(IEnumerable<PropertyItem> properties, 
            IEnumerable<DirectoryItem> directories)
        {
            var directoryIds = directories.Select(x => x.Id).ToSet();
            return properties.Where(x => !directoryIds.Contains(x.Property));
        }
    }
    
    internal static class Extensions
    {
        public static ISet<T> ToSet<T>(this IEnumerable<T> items)
            => new HashSet<T>(items);
    }
}