# Configuration

How to configure the NauckIT.PostgreSQLProvider

## Common Settings

* **connectionStringName** - Name of the connection string defined in the connectionStrings config section of the web.config
* **applicationName** - Name of your web application.

## Membership Provider

...

## Role Provider

...

## Profile Provider

...

## Session State Store Provider

* **enableExpiredSessionAutoDeletion** - Enables automatic session garbage collection. Possible values: _true_ or _false_, default is _false_.
* **expiredSessionAutoDeletionInterval** - The time, in milliseconds, between automatic session garbage collection processings. Default is _1800000_ (30 minutes).