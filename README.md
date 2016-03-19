# msilauncher

Library which I wrote in order to detect installation changes inside MSI when UI installation launched.

Build status:

[![Build Status](https://travis-ci.org/nitridan/msilauncher.svg?branch=master)](https://travis-ci.org/nitridan/msilauncher)

Example usage:
```csharp
using System;
using System.Linq;
using Nitridan.MsiLauncher;

namespace ConsoleApplication
{
    public class Program
    {
        private const string Path = @"C:\Users\jenkinsbuild\Downloads\node-v5.5.0-x64.msi";

        [STAThread]
        static void Main(string[] args)
        {
            var launcher = new PackageLauncher();
            var result = launcher.LaunchMsi(Path);
            Console.WriteLine("Changed properties:");
            var changedProperties = result
                .ChangedProperties
                .Select(x => string.Format("Property: '{0}' Value: '{1}'", x.Property, x.Value))
                .ToList();

            changedProperties.ForEach(Console.WriteLine);

            Console.WriteLine("Added properties:");
            var addedProperties = result
                .AddedProperties
                .Select(x => string.Format("Property: '{0}' Value: '{1}'", x.Property, x.Value))
                .ToList();

            addedProperties.ForEach(Console.WriteLine);
            Console.ReadKey(true);
        }
    }
}

```
