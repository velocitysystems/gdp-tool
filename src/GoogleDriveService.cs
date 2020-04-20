namespace GdpTool
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;

    /// <summary>
    /// Provides services for Google Drive using the available APIs.
    /// </summary>
    public class GoogleDriveService
    {
        #region Fields

        private readonly DriveService _service;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveService" /> class.
        /// </summary>
        /// <param name="credential">The <see cref="UserCredential" />.</param>
        public GoogleDriveService(UserCredential credential)
        {
            // Create Drive API service.
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Program.ApplicationName,
            });
        }

        #endregion
    }
}