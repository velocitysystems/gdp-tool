namespace GdpTool
{
    using System;
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
                    _logger.Information("Started the Google Drive service.");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to start the Google Drive service.");
                }
            }

            // Asynchronously search the drive(s) and page the results.
            await foreach(var files in _service.GetFilesAsync(pageSize: 5))
            {
                foreach (var file in files)
                {
                    _logger.Information(file.Name);
                }
            }
        }

        #endregion
    }
}