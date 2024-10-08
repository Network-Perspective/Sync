﻿using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Text;

using Colors.Net;
using Colors.Net.StringColorExtensions;

using CsvHelper;
using CsvHelper.Configuration;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.Utils.Batching;

using Newtonsoft.Json;

using NLog;

using PowerArgs;

namespace NetworkPerspective.Sync.Cli
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [TabCompletion]
    public class InteractionsOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Text file with user data"), ArgRequired]
        public string[] Csv { get; set; }

        [ArgDescription("Text file delimiter (Default = tab delimited)"), DefaultValue("\t")]
        public string CsvDelimiter { get; set; }

        [ArgDescription("New line delimiter (Default = CRLF)")]
        public string CsvNewLine { get; set; }

        [ArgDescription("SourceId column (IdProperty)"), ArgRequired, DefaultValue("From (EmployeeId)")]
        public string FromCol { get; set; }

        [ArgDescription("TargetId column (IdProperty)"), ArgRequired, DefaultValue("To (EmployeeId)")]
        public string ToCol { get; set; }

        [ArgDescription("Timestamp column name"), ArgRequired, DefaultValue("When")]
        public string WhenCol { get; set; }

        [ArgDescription("InteractionId column name"), ArgRequired, DefaultValue("InteractionId")]
        public string InteractionId { get; set; }

        [ArgDescription("Timestamp timezone (see TimeZoneInfo.FindSystemTimeZoneById)"), ArgRequired, DefaultValue("Central European Standard Time")]
        public string TimeZone { get; set; }

        [ArgDescription("EventId column name"), ArgRequired, DefaultValue("EventId")]
        public string EventIdCol { get; set; }

        [ArgDescription("Recurrence column name"), ArgRequired, DefaultValue("RecurrenceType")]
        public string RecurrentceCol { get; set; }

        [ArgDescription("Duration column name"), ArgRequired, DefaultValue("Duration")]
        public string DurationCol { get; set; }

        [ArgDescription("One of (Chat, Email, Meeting)"), ArgRequired]
        public string DataSourceType { get; set; }

        [ArgDescription("Split requests into batches of specified number of interactions"), ArgRequired, DefaultValue(100_000)]
        public int BatchSize { get; set; }

        [ArgDescription("Run in debug mode - save request to file DebugFn, but do not send it to remote service")]
        public string DebugFn { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class InteractionsClient : IAsyncDisposable
    {
        private Dictionary<string, ColumnDescriptor> _fromCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _toCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _whenCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _eventIdCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _recurrenceCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _durationCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _interactionIdCols = new Dictionary<string, ColumnDescriptor>();

        private readonly ISyncHashedClient _client;
        private readonly IFileSystem _fileSystem;
        private readonly InteractionsOpts _options;
        private readonly Batcher<HashedInteraction> _batchSplitter;
        private long estimatedNoOfBatches = 0;
        private readonly ProgressBar _sendingProgress = new ProgressBar();

        public InteractionsClient(ISyncHashedClient client, IFileSystem fileSystem, InteractionsOpts options)
        {
            _client = client;
            _fileSystem = fileSystem;
            _options = options;
            _batchSplitter = new Batcher<HashedInteraction>(options.BatchSize);
        }

        public async ValueTask DisposeAsync()
        {
            await _batchSplitter.FlushAsync();
            _sendingProgress.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Main()
        {

            if (string.IsNullOrWhiteSpace(_options.FromCol) || string.IsNullOrWhiteSpace(_options.ToCol) ||
                string.IsNullOrWhiteSpace(_options.WhenCol))
            {
                throw new ArgException("Please provide column names (or use defaults)");
            }
            if (string.IsNullOrWhiteSpace(_options.BaseUrl) && string.IsNullOrWhiteSpace(_options.DebugFn))
            {
                throw new ArgException("Either BaseUrl or DebugFn must be specified");
            }

            // w don't want extra logging to appear on console
            LogManager.SuspendLogging();

            _fromCols = _options.FromCol.AsColumnDescriptorDictionary();
            _toCols = _options.ToCol.AsColumnDescriptorDictionary();
            _whenCols = _options.WhenCol.AsColumnDescriptorDictionary();
            _eventIdCols = _options.EventIdCol.AsColumnDescriptorDictionary();
            _recurrenceCols = _options.RecurrentceCol.AsColumnDescriptorDictionary();
            _durationCols = _options.DurationCol.AsColumnDescriptorDictionary();
            _interactionIdCols = _options.InteractionId.AsColumnDescriptorDictionary();

            var timer = Stopwatch.StartNew();

            _batchSplitter.OnBatchReady(SendBatch);

            // read the CSV             
            foreach (var fn in _options.Csv)
            {
                await ReadCsvInteractions(fn, _options);
            }

            ColoredConsole.WriteLine($"Uploading data");
            await _batchSplitter.FlushAsync();
            _sendingProgress.Dispose();

            ColoredConsole.WriteLine("Success!".Green());
            ColoredConsole.WriteLine("Time elapsed " + timer.Elapsed);
        }

        private async Task SendBatch(BatchReadyEventArgs<HashedInteraction> args)
        {
            var request = new SyncHashedInteractionsCommand()
            {
                Interactions = args.BatchItems,
                ServiceToken = _options.Token
            };

            _sendingProgress.Report((double)args.BatchNumber / estimatedNoOfBatches);

            // dump to file or send to api
            if (!string.IsNullOrWhiteSpace(_options.DebugFn))
            {
                var fileName = string.Format("{0}-batch-{1}{2}", Path.GetFileNameWithoutExtension(_options.DebugFn), $"{args.BatchNumber}", Path.GetExtension(_options.DebugFn));
                var fullPath = Path.Combine(Path.GetDirectoryName(_options.DebugFn)!, fileName);

                using (var file = _fileSystem.File.CreateText(fullPath))
                {
                    var json = JsonConvert.SerializeObject(request, Formatting.Indented);
                    await file.WriteAsync(json);
                }
            }
            else
            {
                var corellationId = await _client.SyncInteractionsAsync(request);
                ColoredConsole.WriteLine("CorellationId: " + corellationId.ToString().Cyan());
            }
        }

        private async Task ReadCsvInteractions(string fileName, InteractionsOpts args)
        {
            ColoredConsole.WriteLine($"Reading CSV {fileName}...");
            var header = new List<ColumnDescriptor>();

            if (!_fileSystem.File.Exists(fileName))
            {
                ColoredConsole.WriteLine($"File {fileName} does not exist".Red());
                return;
            }

            var fileSize = _fileSystem.FileInfo.New(fileName).Length;
            var bufferSize = 1024 * 1024 * 10; // 10MB
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = args.CsvDelimiter,
                BufferSize = bufferSize,
            };
            if (!string.IsNullOrEmpty(args.CsvNewLine))
            {
                csvConfig.NewLine = args.CsvNewLine;
            }

            using (var progress = new ProgressBar())
            using (var stream = _fileSystem.FileStream.New(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, bufferSize))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                csv.Read();

                string? value;
                for (int i = 0; csv.TryGetField<string>(i, out value); i++)
                {
                    header.Add((value ?? "").AsColumnDescriptor());
                }

                long lineCounter = 0;
                while (csv.Read())
                {
                    var interaction = new HashedInteraction()
                    {
                        Label = new List<HashedInteractionLabel>()
                    };
                    switch (args.DataSourceType.ToLower())
                    {
                        case "chat": interaction.Label.Add(HashedInteractionLabel.Chat); break;
                        case "email": interaction.Label.Add(HashedInteractionLabel.Email); break;
                        case "meeting": interaction.Label.Add(HashedInteractionLabel.Meeting); break;
                    }

                    for (int i = 0; csv.TryGetField<string>(i, out value) && i < header.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(value)) continue;

                        var field = header[i];
                        var fieldName = field.Name;
                        if (_fromCols.ContainsKey(fieldName))
                        {
                            interaction.SourceIds ??= new Dictionary<string, string>();
                            interaction.SourceIds[_fromCols[fieldName].InRoundBrackets] = value;
                        }
                        else if (_toCols.ContainsKey(fieldName))
                        {
                            interaction.TargetIds ??= new Dictionary<string, string>();
                            interaction.TargetIds[_toCols[fieldName].InRoundBrackets] = value;
                        }
                        else if (_whenCols.ContainsKey(fieldName))
                        {
                            interaction.When = value.AsUtcDate(args.TimeZone);
                        }
                        else if (_interactionIdCols.ContainsKey(fieldName))
                        {
                            interaction.InteractionId = value;
                        }
                        else if (_eventIdCols.ContainsKey(fieldName))
                        {
                            interaction.EventId = value;
                        }
                        else if (_recurrenceCols.ContainsKey(fieldName))
                        {
                            switch (value.ToLower())
                            {
                                case "daily": interaction.Label.Add(HashedInteractionLabel.RecurringDaily); break;
                                case "weekly": interaction.Label.Add(HashedInteractionLabel.RecurringWeekly); break;
                                case "monthly": interaction.Label.Add(HashedInteractionLabel.RecurringMonthly); break;
                            }
                        }
                        else if (_durationCols.ContainsKey(fieldName))
                        {
                            if (Int32.TryParse(value, out var duration))
                            {
                                interaction.DurationMinutes = duration;
                            }
                        }
                    }

                    await _batchSplitter.AddRangeAsync(new[] { interaction });

                    lineCounter++;

                    // report progress                    
                    var position = reader.BaseStream.Position;
                    progress.Report((double)position / (double)fileSize);
                }

                estimatedNoOfBatches += lineCounter / _options.BatchSize;
            }
        }
    }
}