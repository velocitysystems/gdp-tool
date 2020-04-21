# gdp-tool
A .NET Core console application to audit file/folder permissions in Google Drive™.

**Features**
- Audit file/folder permissions
- Remove non-owner permissions^
- Supports recursive scanning
- Supports read-only operation with report written to log

*^Executed only when the `-r `flag is used.*

**Supported Platforms**
- Windows
- Linux
- Unix, macOS

**Usage**

|Flag|Description|Required|
|---|---|---|
|`-c "credentials.json"`|The path to the "credentials.json" file|True|
|`-r`|Remove non-owner permissions. *This operation cannot be reversed*.|False|

**Requirements**

Enable the Google Drive API as described [here](https://developers.google.com/drive/api/v3/enable-drive-api).

1. Go to the [Google API Console](https://console.developers.google.com/).
2. Select a project.
3. In the sidebar on the left, expand **APIs & auth** and select **APIs**.
4. In the displayed list of available APIs, click the Drive API link and click **Enable API**.


**Disclaimer**

*The material embodied in this software is provided to you "as-is" and without warranty of any kind, express, implied or otherwise, including without limitation, any warranty of fitness for a particular purpose. In no event shall the author be liable to you or anyone else for any direct, special, incidental, indirect or consequential damages of any kind, or any damages whatsoever, including without limitation, loss of profit, loss of use, savings or revenue, or the claims of third parties, whether or not the author has been advised of the possibility of such loss, however caused and on any theory of liability, arising out of or in connection with the possession, use or performance of this software.*
