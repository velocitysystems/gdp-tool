namespace GdpTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using GdpTool.Models;
    using GdpTool.Services;
    using Google.Apis.Drive.v3.Data;
    using Serilog;

    public class Program
    {
        #region Constants

        /// <summary>
        /// The application name as reported to the API.
        /// </summary>
        public const string ApplicationName = "Google Drive Permissions Tool";

        #endregion

        #region Fields

        private readonly Options _options;
        private readonly ILogger _logger;
        private GoogleDriveService _service;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Program" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger implementation.</param>
        public Program(Options options, ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Run the program service.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RunAsync()
        {
            if (_service is null)
            {
                try
                {
                    _service = await GoogleDriveService.CreateAsync(_options.CredentialsPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to start the service.");
                }
            }

            _logger.Information("Starting scan.");

            var resultsCount = 0;
            var matchesCount = 0;

            /// Asynchronously scan the drive(s) for non-owner permissions.
            await foreach (var results in _service.GetFilesAsync(query: "'me' in owners"))
            {
                _logger.Information("Found result(s) {start} to {end}.", resultsCount + 1, resultsCount + results.Count);

                var matches = new Dictionary<File, IReadOnlyList<Permission>>();
                foreach (var result in results)
                {
                    LogFileInformation(result);
                    foreach (var permission in result.Permissions)
                    {
                        _logger.Information("{@permission}", new { permission.Role, permission.Type, permission.DisplayName });
                    }

                    var nonOwnerPermissions = result.Permissions.Where(q => q.Role != Enums.Permission.Role.Owner);
                    if (nonOwnerPermissions.Any())
                    {
                        _logger.Information("Found {count} non-owner permissions.", nonOwnerPermissions.Count());
                        matches.Add(result, nonOwnerPermissions.ToList());
                    }
                }

                // Remove permissions if matches found and remove flag is specified.
                if (matches.Any() && _options.RemoveNonOwnerPermissions)
                {
                    await RemoveNonOwnerPermissionsAsync(matches);
                }

                matchesCount += matches.Count;
                resultsCount += results.Count;
            }

            if (_options.RemoveNonOwnerPermissions && matchesCount == 0)
            {
                _logger.Warning($"Remove flag specified, but no matching result(s) found.");
            }
            else if (!_options.RemoveNonOwnerPermissions)
            {
                _logger.Warning($"Remove flag not specified.");
            }

            _logger.Information("Finished! Found {resultsCount} result(s) and {matchesCount} result(s) with non-owner permissions.", resultsCount, matchesCount);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Asynchronously remove non-owner permissiosn from the specified matches.
        /// </summary>
        /// <param name="matches">The matches with non-owner permissions.</param>
        /// <returns>A task.</returns>
        private async Task RemoveNonOwnerPermissionsAsync(Dictionary<File, IReadOnlyList<Permission>> matches)
        {
            _logger.Information("Removing permissions on {count} result(s).", matches.Count);

            var count = 0;
            foreach (var result in matches)
            {
                var file = result.Key;
                var nonOwnerPermissions = result.Value;

                LogFileInformation(file);
                foreach (var permission in nonOwnerPermissions)
                {
                    _logger.Information("{@permission}", new { permission.Role, permission.Type, permission.DisplayName });

                    // Get the object from the API to ensure: (1) it exists, (2) it is unchanged since the scan.
                    var obj = await _service.GetPermissionAsync(file.Id, permission.Id);
                    if (obj is null)
                    {
                        _logger.Warning("Permission no longer exists. Skipping.");
                        continue;
                    }

                    // Verify the cached permission against the object.
                    if (!(obj.Role == permission.Role
                        && obj.Type == permission.Type
                        && obj.DisplayName == permission.DisplayName
                        && obj.ExpirationTime == permission.ExpirationTime))
                    {
                        _logger.Warning("Permission has changed since the scan. Skipping.");
                        continue;
                    }

                    // Permission exists and is unchanged, so we can remove it.
                    var deleted = await _service.DeletePermissionAsync(file.Id, permission.Id);
                    if (deleted)
                    {
                        count++;
                        _logger.Information("Removed permission {id}.", permission.Id);
                    }
                    else
                    {
                        _logger.Error("Failed to remove permission {id}.", permission.Id);
                        _logger.Information("The permission no longer exists, or was removed when its parent permission was deleted.");
                    }
                }
            }

            _logger.Information("Finished! Removed {count} permission(s).", count);
        }

        /// <summary>
        /// Log file information to the logger.
        /// </summary>
        /// <param name="file">The file or folder.</param>
        private void LogFileInformation(File file)
        {
            var isFolder = file.MimeType == GoogleDriveService.FolderMimeType;
            switch (isFolder)
            {
                case true:
                    _logger.Information("{name}", file.Name);
                    break;

                case false:
                    _logger.Information("{name} [{mimeType}]", file.Name, file.MimeType);
                    break;

            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static async Task Main(string[] args)
        {
            var getParserOptions = new TaskCompletionSource<(bool Parsed, Options Options)>();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => getParserOptions.TrySetResult((true, options)))
                .WithNotParsed(options => getParserOptions.TrySetResult((false, default)));

            var result = await getParserOptions.Task;
            if (!result.Parsed)
            {
                return;
            }

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("audit.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                await new Program(result.Options, logger).RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unhandled exception");
            }            
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// The supported command-line options.
        /// </summary>
        public class Options
        {
            [Option('c', "credentials", Required = true, HelpText = "The path to the \"credentials.json\" file.")]
            public string CredentialsPath { get; set; }

            [Option('r', "remove", Required = false, HelpText = "Remove non-owner permissions. This operation cannot be reversed.")]
            public bool RemoveNonOwnerPermissions { get; set; }
        }

        #endregion
    }
}