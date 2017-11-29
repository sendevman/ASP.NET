﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.IO.Extensions;

namespace Abp.Resources.Embedded
{
    public class EmbeddedResourceSet
    {
        public string RootPath { get; }

        public Assembly Assembly { get; }

        public string ResourceNamespace { get; }

        public EmbeddedResourceSet(string rootPath, Assembly assembly, string resourceNamespace)
        {
            RootPath = rootPath.EnsureEndsWith('/');
            Assembly = assembly;
            ResourceNamespace = resourceNamespace;
        }

        internal void AddResources(Dictionary<string, EmbeddedResourceItem> resources)
        {
            foreach (var resourceName in Assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(ResourceNamespace))
                {
                    continue;
                }

                using (var stream = Assembly.GetManifestResourceStream(resourceName))
                {
                    var relativePath = ConvertToRelativePath(resourceName);
                    var filePath = EmbeddedResourcePathHelper.NormalizePath(RootPath + relativePath);

                    resources[filePath] = new EmbeddedResourceItem(
                        relativePath,
                        stream.GetAllBytes(),
                        Assembly
                    );
                }
            }
        }

        private string ConvertToRelativePath(string resourceName)
        {
            return resourceName.Substring(ResourceNamespace.Length + 1);
        }

        private static string CalculateFileName(string filePath)
        {
            if (!filePath.Contains("/"))
            {
                return filePath;
            }
            return filePath.Substring(filePath.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }
    }
}