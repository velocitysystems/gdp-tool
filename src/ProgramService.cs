namespace GdpTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Google.Apis.Drive.v3.Data;
    using Serilog;
    using static GdpTool.Program;

    /// <summary>
    /// Provides the service implementation for the program.
    /// </summary>
    public class ProgramService
    {
        #region Fields

        private readonly Options _options;
        private readonly ILogger _logger;
        private GoogleDriveService _service;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramService" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger implementation.</param>
        public ProgramService(Options options, ILogger logger)
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
                    _logger.Information("Started the service.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to start the service.");
                }
            }

            // Scan for file(s)/folder(s) with non-owner permissions.
            var results = await ScanForNonOwnerPermissionsAsync();
            if (_options.RemoveNonOwnerPermissions && !results.Any())
            {
                _logger.Warning($"Remove flag specified, but no matching result(s) found.");
                return;
            }
            else if (!_options.RemoveNonOwnerPermissions)
            {
                _logger.Warning($"Remove flag not specified..");
                return;
            }

            // Remove non-owner permissions from results.
            await RemoveNonOwnerPermissionsAsync(results);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Asynchronously scan the drive(s) for non-owner permissions.
        /// </summary>
        /// <returns>A dictionary of files and the non-owner permissions found.</returns>
        private async Task<Dictionary<File, IReadOnlyList<Permission>>> ScanForNonOwnerPermissionsAsync()
        {
            _logger.Information("Starting scan.");

            var count = 0;
            var resultsWithNonOwnerPermissions = new Dictionary<File, IReadOnlyList<Permission>>();            
            await foreach (var results in _service.GetFilesAsync(query: "'me' in owners"))
            {
                _logger.Information("Found result(s) {start} to {end}.", count + 1, count + results.Count);
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
                        resultsWithNonOwnerPermissions.Add(result, nonOwnerPermissions.ToList());
                    }
                }

                count += results.Count;
            }

            _logger.Information("Finished scan! Found {totalCount} result(s) and {matchingCount} result(s) with non-owner permissions.", count, resultsWithNonOwnerPermissions.Count);
            return resultsWithNonOwnerPermissions;
        }

        /// <summary>
        /// Asynchronously remove non-owner permissiosn from the specified results.
        /// </summary>
        /// <param name="results">The results with non-owner permissions.</param>
        /// <returns>A task.</returns>
        private async Task RemoveNonOwnerPermissionsAsync(Dictionary<File, IReadOnlyList<Permission>> results)
        {
            _logger.Information("Removing permissions on {count} result(s).", results.Count);

            var count = 0;
            foreach (var result in results)
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
    }
}