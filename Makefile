# Kopernicus Makefile
# Copyright 2016 Thomas

# We only support roslyn compiler at the moment
CS := csc
MONO_ASSEMBLIES := /usr/lib/mono/2.0

# Build Outputs
CURRENT_DIR := $(shell pwd)
PLUGIN_DIR := $(CURRENT_DIR)/build/Debug/GameData/KittopiaTech/Plugins
RELEASE_DIR := $(CURRENT_DIR)/build/Release
BUILD_DIR := $(RELEASE_DIR)/GameData/KittopiaTech/Plugins
PLUGIN := $(PLUGIN_DIR)/KittopiaTech.dll

# Code paths
CODE := $(CURRENT_DIR)/src

# Assembly References
CORLIB := $(MONO_ASSEMBLIES)/mscorlib.dll,$(MONO_ASSEMBLIES)/System.dll,$(MONO_ASSEMBLIES)/System.Core.dll
REFS := $(CORLIB),Assembly-CSharp.dll,UnityEngine.dll,UnityEngine.UI.dll,Kopernicus.dll,Kopernicus.OnDemand.dll,Kopernicus.Components.dll,Kopernicus.Parser.dll

# Zip File
ZIP_NAME := KittopiaTech-$(shell git describe --tags)-$(shell date "+%Y-%m-%d").zip

### BUILD TARGETS ###
all: plugin
kittopia: $(PLUGIN)
plugin: kittopia copy_plugin_files
	cd $(RELEASE_DIR); zip -r $(ZIP_NAME) .
	
### LIBRARIES ###
$(PLUGIN): generate_dirs
	$(CS) /debug+ /debug:portable /out:$(PLUGIN) /nostdlib+ /target:library /platform:anycpu /recurse:$(CODE)/*.cs /reference:$(REFS)

### UTILS ###
generate_dirs:
	mkdir -p $(PLUGIN_DIR)
	mkdir -p $(BUILD_DIR)
copy_plugin_files:
	cp $(PLUGIN) $(BUILD_DIR)
clean:
	rm -r $(PLUGIN_DIR)
	rm $(RELEASE_DIR)/$(ZIP_NAME)
	rm *.dll