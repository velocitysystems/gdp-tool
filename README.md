# gdp-tool
A .NET Core console application to scan and modify file/folder permissions in Google Drive™.

**Features**
- Scan and modify Google Drive™ file/folder permissions
- Supports recursive scanning
- Supports 'audit-only' mode with log file output

**Supported Platforms**
- Windows
- Linux
- Unix, macOS

**Usage**
|Flag|Description|
|---|---|
|`-c "credentials.json"`|The path to the "credentials.json" file|
|`-t "/path/to/the/resource"`|The path to the target resource i.e. file/folder|
|`-p [Permission]`|The permission to apply to each resource found in the scan|
|`-a`|Run the tool in 'audit-only' mode and debug changes to log file^|

^ When run in 'audit-only' mode, no changes are made but a simulated run occurs only.

**Permissions**
|Permission|Description|
|---|---|
|`owner`|Owner-only permission|
|`domain-read`|Domain users can read the resource^|
|`domain-write`|Domain users can read or write to the resource^|

^ When domain permissions are set, the resource is discoverable through search.
