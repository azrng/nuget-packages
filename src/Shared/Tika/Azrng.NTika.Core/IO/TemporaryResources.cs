using System;
using System.Collections.Generic;
using System.IO;

namespace Azrng.NTika.Core.IO
{
    public class TemporaryResources : IDisposable
    {
        private readonly List<IDisposable> _resources = new();
        private readonly List<string> _tempFiles = new();

        public FileInfo CreateTemporaryFile(string suffix)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + suffix);
            _tempFiles.Add(path);
            var file = new FileInfo(path);
            // Create empty file
            using (File.Create(path)) { }
            return file;
        }

        public void AddResource(IDisposable resource)
        {
            _resources.Add(resource);
        }

        public void Dispose()
        {
            foreach (var resource in _resources)
            {
                try
                {
                    resource.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            foreach (var file in _tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            _resources.Clear();
            _tempFiles.Clear();
        }
    }
}
