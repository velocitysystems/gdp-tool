namespace GdpTool
{
   using CommandLine;

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
            [Option('c', "crdentials", Required = true, HelpText = "The path to the \"credentials.json\" file.")]
            public string Credentials { get; set; }

            [Option('t', "target", Required = true, HelpText = "The path to the target resource i.e. file/folder.")]
            public string Target { get; set; }

            [Option('p', "permission", Required = false, HelpText = "The permission to apply to each resource found in the scan.")]
            public string Permission { get; set; }

            [Option('a', "audit", Required = false, HelpText = "Run the tool in 'audit-only' mode and debug changes to a log file.")]
            public bool Audit { get; set; }
        }

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    // Implementation goes here.
                });
        }
    }
}
