namespace GdpTool.Models
{
    /// <summary>
    /// Defines enumerations used by the tool.
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Enums for <see cref="Google.Apis.Drive.v3.Data.File"/ ></see>.
        /// </summary>
        public struct File
        {
            /// <summary>
            /// Enums for file MIME type.
            /// </summary>
            public struct MimeType
            {
                /// <summary>
                /// Folder MIME type.
                /// </summary>
                public const string Folder = "application/vnd.google-apps.folder";
            }
        }

        /// <summary>
        /// Enums for <see cref="Google.Apis.Drive.v3.Data.Permission" />.
        /// </summary>
        public struct Permission
        {
            /// <summary>
            /// Enums for permission role.
            /// </summary>
            public struct Role
            {
                /// <summary>
                /// Owner permission,
                /// </summary>
                public const string Owner = "owner";

                /// <summary>
                /// Reader permission,
                /// </summary>
                public const string Reader = "reader";

                /// <summary>
                /// Writer permission.
                /// </summary>
                public const string Writer = "writer";
            }
        }
    }
}
