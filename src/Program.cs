namespace GdpTool
{
    using System;
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

            [Option('r', "remove", Required = false, HelpText = "Remove non-owner permissions. This operation cannot be reversed.")]
            public bool RemoveNonOwnerPermissions { get; set; }
        }

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
                await new ProgramService(result.Options, logger).RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unhandled exception");
            }            
        }
    }
}