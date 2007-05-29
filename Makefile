CSC=gmcs

NPGSQL_DLL=/r:Npgsql.dll
SYSTEM_DLLS=/r:System.Web.dll /r:System.Data.dll /r:System.Configuration.dll
TARGET=NauckIT.PostgreSQLProvider.dll

all: release

SOURCES=PgProfileProvider.cs \
        SerializationHelper.cs \
        Properties/Resources.Designer.cs \
        Properties/AssemblyInfo.cs \
        PgRoleProvider.cs \
        PgMembershipProvider.cs

RESOURCES_SRC=Properties/Resources.resx
RESOURCES=Properties/Resources.resources

resource: $(RESOURCES_SRC)
	resgen2 /compile $^

debug: TARGET_PATH=./mono/debug
debug: resource $(SOURCES)
	mkdir -p $(TARGET_PATH)
	$(CSC) /target:library /debug $(SYSTEM_DLLS) $(NPGSQL_DLL) /out:$(TARGET_PATH)/$(TARGET) $(SOURCES) -resource:$(RESOURCES)

release: TARGET_PATH=./mono/release
release: resource $(SOURCES)
	mkdir -p $(TARGET_PATH)
	$(CSC) /target:library $(SYSTEM_DLLS) $(NPGSQL_DLL) /out:$(TARGET_PATH)/$(TARGET) $(SOURCES) -resource:$(RESOURCES)

clean:
	-rm -f Properties/Resources.resources
	-rm -f ../mono/debug/$(TARGET)
	-rm -f ../mono/release/$(TARGET)

.PHONY: all debug debug-full release release-full clean
