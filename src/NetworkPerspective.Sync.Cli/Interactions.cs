﻿using System;
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
    public class InteractionsOpts : ICommonOpts
    {
        [ArgDescription("Connector token"), ArgRequired]
        public string Token { get; set; }

        [ArgDescription("Api base url")]
        public string BaseUrl { get; set; }

        [ArgDescription("Text file with user data (Default = StdIn)")]
        public string? Csv { get; set; }

        [ArgDescription("Text file delimiter (Default = tab delimited)"), DefaultValue("\t")]
        public string CsvDelimiter { get; set; }

        [ArgDescription("New line delimiter (Default = Environment.NewLine)")]
        public string CsvNewLine { get; set; }

        [ArgDescription("SourceId column (IdProperty)"), ArgRequired, DefaultValue("From (EmployeeId)")]
        public string FromCol { get; set; }

        [ArgDescription("TargetId column (IdProperty)"), ArgRequired, DefaultValue("To (EmployeeId)")]
        public string ToCol { get; set; }

        [ArgDescription("Timestamp column name"), ArgRequired, DefaultValue("When")]
        public string WhenCol { get; set; }

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

        [ArgDescription("Split requests into batches of specified number of interactions"), ArgRequired, DefaultValue(100000)]
        public int BatchSize { get; set; }

        [ArgDescription("Run in debug mode - save request to file DebugFn, but do not send it to remote service")]
        public string DebugFn { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class InteractionsClient
    {
        private Dictionary<string, ColumnDescriptor> _fromCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _toCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _whenCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _eventIdCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _recurrenceCols = new Dictionary<string, ColumnDescriptor>();
        private Dictionary<string, ColumnDescriptor> _durationCols = new Dictionary<string, ColumnDescriptor>();

        private readonly ISyncHashedClient _client;
        private readonly IFileSystem _fileSystem;

        public InteractionsClient(ISyncHashedClient client, IFileSystem fileSystem)
        {
            _client = client;
            _fileSystem = fileSystem;
        }

        public async Task Main(InteractionsOpts args)
        {
            if (string.IsNullOrWhiteSpace(args.FromCol) || string.IsNullOrWhiteSpace(args.ToCol) ||
                string.IsNullOrWhiteSpace(args.WhenCol))
            {
                throw new ArgException("Please provide column names (or use defaults)");
            }
            if (string.IsNullOrWhiteSpace(args.BaseUrl) && string.IsNullOrWhiteSpace(args.DebugFn))
            {
                throw new ArgException("Either BaseUrl or DebugFn must be specified");
            }

            _fromCols = args.FromCol.AsColumnDescriptorDictionary();
            _toCols = args.ToCol.AsColumnDescriptorDictionary();
            _whenCols = args.WhenCol.AsColumnDescriptorDictionary();
            _eventIdCols = args.EventIdCol.AsColumnDescriptorDictionary();
            _recurrenceCols = args.RecurrentceCol.AsColumnDescriptorDictionary();
            _durationCols = args.DurationCol.AsColumnDescriptorDictionary();

            var timer = Stopwatch.StartNew();

            //// read the CSV (tab separated by default)            
            await ReadCsvInteractions(args, (batch, data) => SendBatch(batch, data, args));

            ColoredConsole.WriteLine("Success!".Green() + " Time elapsed " + timer.Elapsed);
        }

        private async Task SendBatch(int batchNo, List<HashedInteraction> interactions, InteractionsOpts args)
        {
            var request = new SyncHashedInteractionsCommand()
            {
                Interactions = interactions,
                ServiceToken = args.Token
            };

            ColoredConsole.WriteLine("Uploading interactions:");
            ColoredConsole.WriteLine(" - uploading " + interactions.Count.ToString().Cyan() + " records...");

            // dump to file or send to api
            if (!string.IsNullOrWhiteSpace(args.DebugFn))
            {
                var fileName = String.Format("{0}-batch-{1}{2}", Path.GetFileNameWithoutExtension(args.DebugFn), $"{batchNo}", Path.GetExtension(args.DebugFn));
                var fullPath = Path.Combine(Path.GetDirectoryName(args.DebugFn)!, fileName);

                using (var file = _fileSystem.File.CreateText(fullPath))
                {
                    var json = JsonConvert.SerializeObject(request, Formatting.Indented);
                    file.Write(json);
                }
            }
            else
            {
                var corellationId = await _client.SyncInteractionsAsync(request);
                ColoredConsole.WriteLine("CorellationId: " + corellationId.ToString().Cyan());
            }

            await Task.CompletedTask;
        }

        private async Task ReadCsvInteractions(InteractionsOpts args, Func<int, List<HashedInteraction>, Task> processBatch)
        {
            ColoredConsole.WriteLine($"Reading CSV...");
            var result = new List<HashedInteraction>();
            var header = new List<ColumnDescriptor>();
            TextReader? reader = null;
            try
            {
                reader = args.Csv != null ? _fileSystem.File.OpenText(args.Csv) : Console.In;
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    NewLine = args.CsvNewLine ?? Environment.NewLine,
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

                var batchNo = 0;
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
                    result.Add(interaction);

                    if (result.Count >= args.BatchSize)
                    {
                        await processBatch(batchNo, result);
                        batchNo++;
                        result = new List<HashedInteraction>();
                    }
                }

                if (result.Count > 0)
                {
                    await processBatch(batchNo, result);
                }
            }
            finally
            {
                if (args.Csv != null) reader?.Dispose();
            }
        }
    }
}