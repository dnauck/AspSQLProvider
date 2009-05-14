@echo off
cls
Tools\NAnt\NAnt.exe -buildfile:AspSQLProvider.build -D:codemetrics.output.type=HtmlFile -nologo -logfile:nant-build-all.log.txt %*
echo %time% %date%
pause