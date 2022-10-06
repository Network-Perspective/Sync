using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Colors.Net;
using Colors.Net.StringColorExtensions;

using CsvHelper;
using CsvHelper.Configuration;

using NetworkPerspective.Sync.Infrastructure.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PowerArgs;
using PowerArgs.Cli;

namespace NetworkPerspective.Sync.Cli
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TabCompletion]
    public class GroupsOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Text file with user data (Default = StdIn)")]
        public string? Csv { get; set; }

        [ArgDescription("Text file delimiter (Default = tab delimited)"), DefaultValue("\t")]
        public string CsvDelimiter { get; set; }

        [ArgDescription("Identifier column (Default Id or Code)"), ArgRequired, DefaultValue("Id,Code")]
        public string IdCol { get; set; }

        [ArgDescription("Name column (Default Id or Code)"), ArgRequired, DefaultValue("Name")]
        public string NameCol { get; set; }

        [ArgDescription("Category column (Default Id or Code)"), ArgRequired, DefaultValue("Category")]
        public string CategoryCol { get; set; }

        [ArgDescription("Parent column (Default Id or Code)"), ArgRequired, DefaultValue("ParentId,ParentCode")]
        public string ParentCol { get; set; }

        [ArgDescription("Run in debug mode - save request to file DebugFn, but do not send it to remote service")]
        public string DebugFn { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class GroupsClient
    {
        private Dictionary<string, ColumnDescriptor> _idCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _nameCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _categoryCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _parentCols = new Dictionary<string, ColumnDescriptor>();

        private readonly ISyncHashedClient _client;
        private readonly IFileSystem _fileSystem;

        public GroupsClient(ISyncHashedClient client, IFileSystem fileSystem)
        {
            _client = client;
            _fileSystem = fileSystem;
        }

        public async Task Main(GroupsOpts args)
        {
            if (string.IsNullOrWhiteSpace(args.IdCol) || string.IsNullOrWhiteSpace(args.NameCol) ||
                string.IsNullOrWhiteSpace(args.CategoryCol) || string.IsNullOrWhiteSpace(args.ParentCol))
            {
                throw new ArgException("Please provide column names (or use defaults)");
            }
            if (string.IsNullOrWhiteSpace(args.BaseUrl) && string.IsNullOrWhiteSpace(args.DebugFn))
            {
                throw new ArgException("Either BaseUrl or DebugFn must be specified");
            }

            _idCols = args.IdCol.AsColumnDescriptorDictionary();
            _nameCols = args.NameCol.AsColumnDescriptorDictionary();
            _categoryCols = args.CategoryCol.AsColumnDescriptorDictionary();
            _parentCols = args.ParentCol.AsColumnDescriptorDictionary();

            var timer = Stopwatch.StartNew();

            // read the CSV (tab separated by default)            
            var groups = ReadCsvGroups(args.Csv, args.CsvDelimiter);

            var request = new SyncHashedGroupStructureCommand()
            {
                Groups = groups,
                ServiceToken = args.Token
            };

            ColoredConsole.WriteLine("Uploading groups:");
            ColoredConsole.WriteLine(" - uploading " + groups.Count.ToString().Cyan() + " records...");

            // dump to file or send to api
            if (!string.IsNullOrWhiteSpace(args.DebugFn))
            {
                using (var file = _fileSystem.File.CreateText(args.DebugFn))
                {
                    var json = JsonConvert.SerializeObject(request, Formatting.Indented);
                    file.Write(json);
                }
            }
            else
            {
                var corellationId = await _client.SyncGroupsAsync(request);

                ColoredConsole.WriteLine("Success!".Green() + " Time elapsed " + timer.Elapsed);
                ColoredConsole.WriteLine("CorellationId: " + corellationId.ToString().Cyan());
            }
        }

        private List<HashedGroup> ReadCsvGroups(string? fileName, string delimiter)
        {
            ColoredConsole.WriteLine($"Reading CSV...");
            var result = new List<HashedGroup>();
            var header = new List<ColumnDescriptor>();
            TextReader? reader = null;
            try
            {
                reader = fileName != null ? _fileSystem.File.OpenText(fileName) : Console.In;
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    HasHeaderRecord = true,
                    Delimiter = delimiter,
                };
                var csv = new CsvReader(reader, csvConfig);

                csv.Read();

                string value;
                for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                {
                    header.Add(value.AsColumnDescriptor());
                }
                ColoredConsole.WriteLine("Found fields: " + String.Join(", ", header.Select(h => h.Header.Cyan())));

                while (csv.Read())
                {
                    var group = new HashedGroup();

                    for (int i = 0; csv.TryGetField<string>(i, out value) && i < header.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(value)) continue;

                        var field = header[i];
                        var fieldName = field.Name;
                        if (_idCols.ContainsKey(fieldName))
                        {
                            group.Id = value;
                        }
                        else if (_nameCols.ContainsKey(fieldName))
                        {
                            group.Name = value;
                        }
                        else if (_categoryCols.ContainsKey(fieldName))
                        {
                            group.Category = value;
                        }
                        else if (_parentCols.ContainsKey(fieldName))
                        {
                            group.ParentId = value;
                        }
                    }
                    result.Add(group);
                }
            }
            finally
            {
                if (fileName != null) reader?.Dispose();
            }
            return result;
        }
    }
}