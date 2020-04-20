namespace GdpTool
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Util.Store;

    /// <summary>
    /// Provides methods for requesting credentials for Google Cloud Platform API services.
    /// </summary>
    public class GoogleCredentialsService
    {
        /// <summary>
        /// Get the <see cref="UserCredential" /> to authorize API access.
        /// </summary>
        /// <param name="credentialsPath">The path to the "credentials.json" file.</param>
        /// <param name="scopes">The list of requested scopes.</param>
        /// <returns>A <see cref="UserCredential" />.</returns>
        public async Task<UserCredential> GetUserCredentialAsync(string credentialsPath, params string[] scopes)
        {
            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);

            // The file token.json stores the user's access and refresh tokens, and is created
            // automatically when the authorization flow completes for the first time.
            var tokenPath = Path.Combine(Path.GetTempPath(), "token.json");
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            return credential;
        }
    }
}