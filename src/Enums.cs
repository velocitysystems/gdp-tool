namespace GdpTool
{
    /// <summary>
    /// Defines enumerations used by the tool.
    /// </summary>
    public static class Enums
    {
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
