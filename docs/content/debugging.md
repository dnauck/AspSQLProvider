# Debugging

Every Exception is written by *System.Diagnostics.Trace.WriteLine(...)* to the `DefaultTraceListner` for security reasons.


If you want to see the Exception you've to enable Tracing as described here:

http://msdn2.microsoft.com/en-us/library/b0ectfxd(vs.80).aspx

Example:

    <configuration>
    	...
    	<system.web>
    		...
    		<trace enabled="true" />
    	</system.web>
    	
    	<system.diagnostics>
    		<trace>
    			<listeners >
    				<add name="WebPageTraceListener" type="System.Web.WebPageTraceListener, System.Web, Version=2.0.3600.0, Culture=neutral, [[PublicKeyToken]]=b03f5f7f11d50a3a"/>
    				<!-- Log trace information into a logfile -->
    				<!--<add name="TextTraceListener" type="System.Diagnostics.TextWriterTraceListener, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" initializeData="TextWriterOutput.log" />-->
    			</listeners>
    		</trace>
    	</system.diagnostics>
    </configuration>