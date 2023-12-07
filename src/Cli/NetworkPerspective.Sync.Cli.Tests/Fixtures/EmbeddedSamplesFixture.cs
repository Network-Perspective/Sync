using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Cli.Tests.Fixtures
{
    public class EmbeddedSamplesFixture
    {
        private const string SamplesFolder = "Samples";

        public MockFileSystem FileSystem { get; }

        public EmbeddedSamplesFixture()
        {
            // Put all embedded files in mock file system
            var assembly = Assembly.GetExecutingAssembly();

            // Prefix: "{Namespace}.{Folder}."
            var prefix = assembly.GetName().Name! + "." + SamplesFolder + ".";

            var resourcePaths = assembly.GetManifestResourceNames()
                .Where(str => str.StartsWith(prefix));

            var files = new Dictionary<string, MockFileData>();
            foreach (var sample in resourcePaths)
            {
                using Stream stream = assembly.GetManifestResourceStream(sample)!;
                using StreamReader reader = new StreamReader(stream);

                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"            
                // strip prefix
                files.Add(sample.Replace(prefix, ""), reader.ReadToEnd());
            }

            FileSystem = new MockFileSystem(files);
        }
    }
}