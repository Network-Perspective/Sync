using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;

using Colors.Net;
using Colors.Net.StringColorExtensions;

using CsvHelper;
using CsvHelper.Configuration;

using NetworkPerspective.Sync.Infrastructure.Core;

using Newtonsoft.Json;

using PowerArgs;

namespace NetworkPerspective.Sync.Cli
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TabCompletion]
    public class EntitiesOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Text file with user data (Default = StdIn)")]
        public string? Csv { get; set; }

        [ArgDescription("Text file delimiter (Default = tab delimited)"), DefaultValue("\t")]
        public string CsvDelimiter { get; set; }

        [ArgDescription("Identifier columns (comma seperated)"), ArgRequired, DefaultValue("Email,EmployeeId,Username,Domain\\User")]
        public string IdColumns { get; set; }

        [ArgDescription("Prop columns (comma seperated)")]
        public string PropColumns { get; set; }

        [ArgDescription("Group columns (comma seperated)")]
        public string GroupColumns { get; set; }

        [ArgDescription("Relationship columns (comma seperated, target id prop in bracket)"), DefaultValue("Supervisor (EmployeeId)")]
        public string RealtionshipColumns { get; set; }

        [ArgDescription("Change date columns (comma seperated)"), DefaultValue("RowDate,ChangeDate")]
        public string ChangeDateColumns { get; set; }

        [ArgDescription("Timestamp timezone (see TimeZoneInfo.FindSystemTimeZoneById)"), ArgRequired, DefaultValue("Central European Standard Time")]
        public string TimeZone { get; set; }

        [ArgDescription("Run in debug mode - save request to file DebugFn, but do not send it to remote service")]
        public string DebugFn { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class EntitiesClient
    {
        private Dictionary<string, ColumnDescriptor> _idCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _propCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _groupsCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _relationshipCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _changeDateCols = new Dictionary<string, ColumnDescriptor>();

        private readonly ISyncHashedClient _client;
        private readonly IFileSystem _fileSystem;

        public EntitiesClient(ISyncHashedClient client, IFileSystem fileSystem)
        {
            _client = client;
            _fileSystem = fileSystem;
        }

        public async Task Main(EntitiesOpts args)
        {
            if (string.IsNullOrWhiteSpace(args.IdColumns))
            {
                throw new ArgException("At least one id column required");
            }
            if (string.IsNullOrWhiteSpace(args.BaseUrl) && string.IsNullOrWhiteSpace(args.DebugFn))
            {
                throw new ArgException("Either BaseUrl or DebugFn must be specified");
            }

            _idCols = args.IdColumns.AsColumnDescriptorDictionary();
            _propCols = args.PropColumns.AsColumnDescriptorDictionary();
            _groupsCols = args.GroupColumns.AsColumnDescriptorDictionary();
            _relationshipCols = args.RealtionshipColumns.AsColumnDescriptorDictionary();
            _changeDateCols = args.ChangeDateColumns.AsColumnDescriptorDictionary();

            var timer = Stopwatch.StartNew();

            // read the CSV (tab separated by default)            
            var entities = ReadCsvEntries(args);

            var request = new SyncHashedEntitesCommand()
            {
                Entites = entities,
                ServiceToken = args.Token
            };

            ColoredConsole.WriteLine("Uploading entities:");
            ColoredConsole.WriteLine(" - uploading " + entities.Count.ToString().Cyan() + " records...");

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
                var corellationId = await _client.SyncEntitiesAsync(request);

                ColoredConsole.WriteLine("Success!".Green() + " Time elapsed " + timer.Elapsed);
                ColoredConsole.WriteLine("CorellationId: " + corellationId.ToString().Cyan());
            }
        }

        private List<HashedEntity> ReadCsvEntries(EntitiesOpts args)
        {
            ColoredConsole.WriteLine($"Reading CSV...");
            var entities = new List<HashedEntity>();
            var header = new List<ColumnDescriptor>();
            TextReader? reader = null;
            try
            {
                reader = args.Csv != null ? _fileSystem.File.OpenText(args.Csv) : Console.In;
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = Environment.NewLine,
                    HasHeaderRecord = true,
                    Delimiter = args.CsvDelimiter,
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
                    var entity = new HashedEntity()
                    {
                        Ids = new Dictionary<string, string>(),
                        Props = new Dictionary<string, object>()
                    };
                    for (int i = 0; csv.TryGetField<string>(i, out value) && i < header.Count; i++)
                    {
                        var field = header[i];
                        var fieldName = field.Name;
                        if (_idCols.ContainsKey(fieldName))
                        {
                            entity.Ids[fieldName] = value;
                        }
                        else if (_relationshipCols.ContainsKey(fieldName)) // relationship field
                        {
                            if (string.IsNullOrWhiteSpace(value)) continue;

                            entity.Relationships ??= new List<HashedEntityRelationship>();

                            entity.Relationships.Add(
                                new HashedEntityRelationship()
                                {
                                    RelationshipName = field.Name,
                                    TargetIds = new Dictionary<string, string>() { { field.InRoundBrackets, value } }
                                }
                            );
                        }
                        else if (_changeDateCols.ContainsKey(fieldName))
                        {
                            entity.ChangeDate = value.AsUtcDate(args.TimeZone);
                        }
                        else if (_propCols.ContainsKey(fieldName))
                        {
                            if (field.IsJsonField())
                                entity.Props[fieldName] = value == null ? null : JsonConvert.DeserializeObject(value);
                            else
                                entity.Props[fieldName] = value;
                        }
                        else if (_groupsCols.ContainsKey(fieldName))
                        {
                            entity.Groups ??= new List<string>();
                            if (field.IsJsonField())
                            {
                                var list = JsonConvert.DeserializeObject<List<string>>(value);
                                if (list != null)
                                {
                                    foreach (var item in list)
                                    {
                                        entity.Groups.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                entity.Groups.Add(value);
                            }

                        }
                    }
                    entities.Add(entity);
                }
            }
            finally
            {
                if (args.Csv != null) reader?.Dispose();
            }
            return entities;
        }
    }
}