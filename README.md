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
|Flag|Description|Required|
|---|---|
|`-c "credentials.json"`|The path to the "credentials.json" file|True|
|`-p [Permission]`|The permission to apply to each resource found in the scan|True|
|`-f "/path/to/the/folder"`|The path to the target folder, or blank for 'My Drive'|False|
|`-s`|If specified, the tool will save changes otherwise will only write to the log file.|False|

**Permissions**
|Permission|Description|
|---|---|
|`owner`|Owner-only permission|
|`domain-read`|Domain users can read the resource^|
|`domain-write`|Domain users can read or write to the resource^|

^ When domain permissions are set, the resource is discoverable through search.

**Disclaimer**

*The material embodied in this software is provided to you "as-is" and without warranty of any kind, express, implied or otherwise, including without limitation, any warranty of fitness for a particular purpose. In no event shall the author be liable to you or anyone else for any direct, special, incidental, indirect or consequential damages of any kind, or any damages whatsoever, including without limitation, loss of profit, loss of use, savings or revenue, or the claims of third parties, whether or not the author has been advised of the possibility of such loss, however caused and on any theory of liability, arising out of or in connection with the possession, use or performance of this software.*
