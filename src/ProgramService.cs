namespace GdpTool
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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

            // Asynchronously search the drive(s) and page the results.
            var count = 0;
            await foreach(var files in _service.GetFilesAsync(query: "'me' in owners", pageSize: 5))
            {
                _logger.Information($"Found results {count + 1} to {count + files.Count}.");
                foreach (var file in files)
                {
                    // Determine if is a file or folder.
                    var isFolder = file.MimeType == GoogleDriveService.FolderMimeType;
                    switch (isFolder)
                    {
                        case true:
                            _logger.Information($"\"{file.Name}\" (Folder)");
                            break;

                        case false:
                            _logger.Information($"\"{file.Name}\" ({file.MimeType})");
                            break;
                    }

                    // Determine the permissions.
                    _logger.Information("Permissions:");
                    foreach (var permission in file.Permissions)
                    {
                        var json = $"{{ role: {permission.Role}, type: {permission.Type}, displayName: {permission.DisplayName} }}";
                        _logger.Information(json);
                    }

                    // If user has requested to save changes, we go ahead and commit the adjusted permissions.
                    if ( _options.SaveChanges)
                    {
                        switch (_options.Permission)
                        {
                            case "owner":
                                var nonOwnerPermissions = file.Permissions.Where(q => q.Role != "owner");
                                foreach (var permission in nonOwnerPermissions)
                                {
                                    var deleted = await _service.DeletePermissionAsync(file.Id, permission.Id);
                                    var json = $"{{ role: {permission.Role}, type: {permission.Type}, displayName: {permission.DisplayName} }}";
                                    var status = deleted ? "Deleted" : "Failed to Delete";
                                    _logger.Information($"{status}: {json}");
                                }
                                break;

                            default:
                                throw new NotSupportedException("Permission type is not yet supported.");
                        }
                    }
                }

                count += files.Count;
            }

            _logger.Information($"Finished! Found {count} results in total.");
        }

        #endregion
    }
}