# Installation

How to install and run the AspSQLProvider.


## Requirements


* at least .NET 2.0 or Mono >= 1.2.4
* [Npgsql](http://pgfoundry.org/projects/npgsql)
* [PostgreSQL](http://www.postgresql.org/) Server


## Download

### Download via NuGet

[![NuGet Status](http://img.shields.io/nuget/v/NauckIT.PostgreSQLProvider.svg?style=flat)](https://www.nuget.org/packages/NauckIT.PostgreSQLProvider/)

To install the [NuGet Package][nuget], run the following command in the Package Manager Console:

    PM> Install-Package NauckIT.PostgreSQLProvider

### Latest Development Source Code

We use [Git](http://www.git-scm.com/) for source revision control and code sharing.

The Git repository is located at:

    http://github.com/dnauck/AspSQLProvider

The latest source can be checked out with the following command:

    git clone https://github.com/dnauck/AspSQLProvider.git

Run the build script `build.cmd` on Windows or `build.sh` on Unix.

## Setup

1. Copy the `NauckIT.PostgreSQLProvider.dll` into the ~/Bin directory of your Web Application.

2. Add the PostgreSQL connection string into you Web.config, e.g.:

    <connectionStrings>
    	<clear />
    	<add name="AspSQLProvider" connectionString="Server=localhost;Port=5432;Database=website_user;User Id=username;Password=mypassword;Encoding=UNICODE;Sslmode=Prefer;Pooling=true;"/>
    </connectionStrings>


3. Add and enable the AspSQLProvider in your Web.config, e.g.:

    <authentication mode="Forms">
    	<forms name=".AspNetAuth" protection="All" defaultUrl="~/Default.aspx" 
    		loginUrl="~/Login.aspx" timeout="30" path="/" requireSSL="false" 
    		slidingExpiration="true" enableCrossAppRedirects="false" />
    </authentication>
    		
    <machineKey validationKey="518A9D0E650ACE4CB22A35DA4563315098A96D0BB8E357531C7065D032099214A11D1CA074B6D66FF0836B35CEAAD0E7EEEFAED772754832E0A5F94EF8522222"
    	decryptionKey="DB5660C109E9EC70F044BA1FED99DE0C5922321C5125E84C23A1B5CA0E426909"
    	validation="SHA1" decryption="AES" />
    		
    <membership defaultProvider="PgMembershipProvider">
    	<providers>
    		<clear/>
    		<add name="PgMembershipProvider" type="NauckIT.PostgreSQLProvider.PgMembershipProvider" connectionStringName="AspSQLProvider" enablePasswordRetrieval="false" enablePasswordReset="true" requiresQuestionAndAnswer="true" passwordFormat="Hashed" applicationName="WebSite1"/>
    	</providers>
    </membership>
    		
    <roleManager enabled="true" defaultProvider="PgRoleProvider" cacheRolesInCookie="true" cookieName=".AspNetRoles" cookiePath="/" cookieProtection="All" cookieRequireSSL="false" cookieSlidingExpiration="true" createPersistentCookie="false" cookieTimeout="30" maxCachedResults="25">
    	<providers>
    		<clear/>
    		<add name="PgRoleProvider" type="NauckIT.PostgreSQLProvider.PgRoleProvider" connectionStringName="AspSQLProvider" applicationName="WebSite1"/>
    	</providers>
    </roleManager>
    		
    <profile enabled="true" defaultProvider="PgProfileProvider">
    	<providers>
    		<clear/>
    		<add name="PgProfileProvider" type="NauckIT.PostgreSQLProvider.PgProfileProvider" connectionStringName="AspSQLProvider" applicationName="WebSite1"/>
    	</providers>
    	<properties>
    		<add name="FirstName"/>
    		<add name="LastName"/>
    	</properties>
    </profile>
    
    <sessionState	mode="Custom"	customProvider="PgSessionStateStoreProvider">
    	<providers>
    		<clear/>
    		<add name="PgSessionStateStoreProvider" type="NauckIT.PostgreSQLProvider.PgSessionStateStoreProvider" enableExpiredSessionAutoDeletion="true" expiredSessionAutoDeletionInterval="60000" enableSessionExpireCallback="false" connectionStringName="AspSQLProvider" applicationName="WebSite1" />
    	</providers>
    </sessionState>
**Warning:** use your own machineKey for security reasons. [Here](http://www.leastprivilege.com/MSDNUSWebCastAuthenticationAndAuthorizationWithASPNET20.aspx) is a nice tool for generating keys inside the slides zip file, called GenerateMachineKey.

4. Create the PostgreSQL database schema with the provided DatabaseSchema.sql script.

5. After creating the tables, double check the global schema search path and make sure the schema in which you placed the tables is referenced in the current search path. If not, edit the global config file, add the schema to the search path, and restart the postgres server. Otherwise, the ASP.NET configuration will not be able to find the tables.

  [nuget]: http://nuget.org/List/Packages/NauckIT.PostgreSQLProvider