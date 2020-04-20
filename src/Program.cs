namespace GdpTool
{
    using System.Threading.Tasks;
    using CommandLine;
    using Serilog;

    public class Program
    {
        /// <summary>
        /// The application name as reported to the API.
        /// </summary>
        public const string ApplicationName = "Google Drive Permissions Tool";

        /// <summary>
        /// The supported command-line options.
        /// </summary>
        public class Options
        {
            [Option('c', "credentials", Required = true, HelpText = "The path to the \"credentials.json\" file.")]
            public string CredentialsPath { get; set; }

            [Option('p', "permission", Required = true, HelpText = "The permission to apply to each resource found in the scan.")]
            public string Permission { get; set; }

            [Option('f', "folder", Required = false, HelpText = "The path to the target folder, or blank for 'My Drive'.")]
            public string FolderPath { get; set; }

            [Option('s', "save", Required = false, HelpText = "If specified, the tool will save changes otherwise will only write to the log file.")]
            public bool SaveChanges { get; set; }
        }

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static async Task Main(string[] args)
        {
            var getParserOptions = new TaskCompletionSource<Options>();

            Parser.Default.ParseArguments<Options>(args).WithParsed(options => getParserOptions.TrySetResult(options));

            var options = await getParserOptions.Task;
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("audit.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            await new ProgramService(options, logger).RunAsync();
        }
    }
}