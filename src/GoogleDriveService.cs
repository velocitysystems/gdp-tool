namespace GdpTool
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;
    using Google.Apis.Util.Store;

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
        private GoogleDriveService(UserCredential credential)
        {
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Program.ApplicationName,
            });
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Factory method to create a new instance of the <see cref="GoogleDriveService" />.
        /// </summary>
        /// <param name="credentialsPath">The credentials path.</param>
        /// <returns></returns>
        public async static Task<GoogleDriveService> CreateAsync(string credentialsPath)
        {
            var credential = await GetUserCredentialAsync(credentialsPath);
            return new GoogleDriveService(credential);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously get files/folders using the V3 drive API.
        /// </summary>
        /// <param name="fields">The request fields.</param>
        /// <param name="pageSize">Optional page size.</param>
        /// <returns>A <see cref="IReadOnlyList<Google.Apis.Drive.v3.Data.File}" />.</returns>
        public async Task<IReadOnlyList<Google.Apis.Drive.v3.Data.File>> GetFilesAsync(string fields, int pageSize = 10)
        {
            var listRequest = _service.Files.List();
            listRequest.Fields = fields;
            listRequest.PageSize = pageSize;

            var files = (await listRequest.ExecuteAsync())?.Files.ToList();
            return files;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Get the <see cref="UserCredential" />.
        /// </summary>
        /// <param name="credentialsPath">The credentials path.</param>
        /// <returns>A <see cref="UserCredential" />.</returns>
        private static async Task<UserCredential> GetUserCredentialAsync(string credentialsPath)
        {
            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);

            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            string credPath = "token.json";
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                new[] { DriveService.Scope.DriveReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));

            return credential;
        }

        #endregion
    }
}