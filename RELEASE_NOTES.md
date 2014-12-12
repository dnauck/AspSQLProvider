### 2.1 - Unreleased
* Do not open Web.config for Read/Write (Fixes #137)
* Switch to FSharp.ProjectScaffold project and build tooling
* Update to latest Npgsql

### 2.0 - 26.01.2013
* Consider duplicate emails settings while updating a user (Fixes #29)
* Fixed check to see if the provider should load the web or app config as this was breaking the ASP.NET Web Admin tool in .NET 4.0+
 (pull request #1 from steveuk/master, Fixes #131)
* Disable IL merging of Npgsql, add NuGet dependency instead

### 1.3.5 - 29.06.2011
* Update to Npgsql 2.0.11.91
* Implement merging of all required assemblies into a single distribution assembly using ILRepack
* Update to latest FxCop, Gendarme and NAnt versions
* Add NuGet package build target (Fixes #127)
* Fixed bug #16 CreateUninitializedItem(...) throws ProviderException
* Implement support for non web hosted applications (Fixes #17)
* Test for DBNull values on possible NULL fields, based on the provided database schema.(Fixes #10)

### 1.2.4 - 05.03.2009
* Update to Npgsql 2.0.4

### 1.2.3 - 26.02.2009
* Update to Npgsql 2.0.3

### 1.2.1 - 04.11.2008
* Cleanup expired session deletion timer on dispose
* Update to Npgsql 2.0.1
* Don't close connection in UpdateFailureCount before failure update (Fixes #6)

### 1.2.0 - 06.10.2008
* Update to Npgsql 2.0 RTM
* implement support for Session_End event, based on a patch by C. Akkermans (Fixes #3)
* Fixed incorrect session expiration calculation, patch by C. Akkermans (Fixes #5)
* Fixed  incorrect session expiration timestamp calculation, patch by C. Akkermans(Fixes #4)

### 1.1.9 - 20.09.2008
* Update to Npgsql 2.0 RC2

### 1.1.8 - 29.07.2008
* Update to Npgsql 2.0 RC1
* add Mono.Security.dll dependency for Npgsql
* Sign the assembly with strong name key
* Close the DataReader before a new query, work around a new check in Npgsql 2 RC1
* Add support for auto expired session deletion, default is off

### 1.1.0 - 17.11.2007
* Add Session State Store Provider

### 1.0 - 28.05.2007
* Initial release
