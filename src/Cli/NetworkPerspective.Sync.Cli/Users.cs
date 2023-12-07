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
    public class UsersOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Text file with user data (Default = StdIn)")]
        public string? Csv { get; set; }

        [ArgDescription("Text file delimiter (Default = tab delimited)"), DefaultValue("\t")]
        public string CsvDelimiter { get; set; }

        [ArgDescription("Email column"), ArgRequired, DefaultValue("Email,Username")]
        public string EmailColumn { get; set; }

        [ArgDescription("Identifier columns (comma seperated)"), ArgRequired, DefaultValue("Email,Username,EmployeeId")]
        public string IdColumns { get; set; }

        [ArgDescription("Prop columns (comma seperated)")]
        public string PropColumns { get; set; }

        [ArgDescription("Group access columns (comma seperated)")]
        public string GroupAccessColumns { get; set; }

        [ArgDescription("Run in debug mode - save request to file DebugFn, but do not send it to remote service")]
        public string DebugFn { get; set; }

        [ArgDescription("Instruct core api to instantly process user list"), DefaultValue(false)]
        public bool Instant { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class UsersClient
    {
        private Dictionary<string, ColumnDescriptor> _emailCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _idCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _propCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _groupAccessCols = new Dictionary<string, ColumnDescriptor>();

        private readonly ISyncHashedClient _client;
        private readonly IFileSystem _fileSystem;
        private readonly UsersOpts _options;

        public UsersClient(ISyncHashedClient client, IFileSystem fileSystem, UsersOpts options)
        {
            _client = client;
            _fileSystem = fileSystem;
            _options = options;
        }

        public async Task Main()
        {
            if (string.IsNullOrWhiteSpace(_options.EmailColumn))
            {
                throw new ArgException("Email column is required");
            }
            if (string.IsNullOrWhiteSpace(_options.BaseUrl) && string.IsNullOrWhiteSpace(_options.DebugFn))
            {
                throw new ArgException("Either BaseUrl or DebugFn must be specified");
            }
            _emailCols = _options.EmailColumn.AsColumnDescriptorDictionary();
            _idCols = _options.IdColumns.AsColumnDescriptorDictionary();
            _propCols = _options.PropColumns.AsColumnDescriptorDictionary();
            _groupAccessCols = _options.GroupAccessColumns.AsColumnDescriptorDictionary();

            var timer = Stopwatch.StartNew();

            // read the CSV (tab separated by default)            
            var entities = ReadCsvEntries(_options);

            var request = new SyncUsersCommand()
            {
                Users = entities,
                ServiceToken = _options.Token,
                Instant = _options.Instant,
            };

            ColoredConsole.WriteLine("Uploading users:");
            ColoredConsole.WriteLine(" - uploading " + entities.Count.ToString().Cyan() + " records...");

            // dump to file or send to api
            if (!string.IsNullOrWhiteSpace(_options.DebugFn))
            {
                using (var file = _fileSystem.File.CreateText(_options.DebugFn))
                {
                    var json = JsonConvert.SerializeObject(request, Formatting.Indented);
                    file.Write(json);
                }
            }
            else
            {
                var corellationId = await _client.SyncUsersAsync(request);

                ColoredConsole.WriteLine("Success!".Green() + " Time elapsed " + timer.Elapsed);
                ColoredConsole.WriteLine("CorellationId: " + corellationId.ToString().Cyan());
            }
        }

        private List<UserEntity> ReadCsvEntries(UsersOpts args)
        {
            ColoredConsole.WriteLine($"Reading CSV...");
            var entities = new List<UserEntity>();
            var header = new List<ColumnDescriptor>();
            TextReader? reader = null;
            try
            {
                reader = args.Csv != null ? _fileSystem.File.OpenText(args.Csv) : Console.In;
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = args.CsvDelimiter,
                };
                var csv = new CsvReader(reader, csvConfig);

                csv.Read();

                string? value;
                for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                {
                    header.Add((value ?? "").AsColumnDescriptor());
                }
                ColoredConsole.WriteLine("Found fields: " + String.Join(", ", header.Select(h => h.Header.Cyan())));

                while (csv.Read())
                {
                    var entity = new UserEntity()
                    {
                        Ids = new Dictionary<string, string>(),
                        Props = new Dictionary<string, object>()
                    };
                    for (int i = 0; csv.TryGetField<string>(i, out value) && i < header.Count; i++)
                    {
                        var field = header[i];
                        var fieldName = field.Name;
                        if (_emailCols.ContainsKey(fieldName))
                        {
                            entity.Email = value;
                        }
                        if (_idCols.ContainsKey(fieldName))
                        {
                            entity.Ids[fieldName] = value;
                        }
                        else if (_propCols.ContainsKey(fieldName))
                        {
                            if (field.IsJsonField())
                                entity.Props[fieldName] = value == null ? null : JsonConvert.DeserializeObject(value);
                            else
                                entity.Props[fieldName] = value;
                        }
                        else if (_groupAccessCols.ContainsKey(fieldName))
                        {
                            entity.GroupAccess ??= new List<string>();
                            if (field.IsJsonField())
                            {
                                var list = value != null ? JsonConvert.DeserializeObject<List<string>>(value) : null;
                                if (list != null)
                                {
                                    foreach (var item in list)
                                    {
                                        entity.GroupAccess.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                entity.GroupAccess.Add(value);
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