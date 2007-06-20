#!/bin/bash
CSFILES="`find -name \"*.cs\" -type f`"
RESXFILES="`find -name \"*.resx\" -type f`"
GMCS_FLAGS="-keyfile:pgproviders.snk -target:library -out:PostgreSQLProviders.dll -debug:full -pkg:dotnet -r:System.Configuration.dll -r:Npgsql2.dll"

for r in $RESXFILES; do
  resgen /compile $r
  r="`echo $r|sed -e 's/.resx/.resources/g'`"
  GMCS_FLAGS="$GMCS_FLAGS -resource:$r,NauckIT.PostgreSQLProvider.Properties.`basename $r`"
done

echo $GMCS_FLAGS
exec gmcs $GMCS_FLAGS $CSFILES
