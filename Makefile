# Makefile for DStream Console Output Provider
# Creates a single self-contained binary at the exact location expected by dstream.hcl

default: build

.PHONY: help build clean verify test rebuild
.DEFAULT_GOAL := help

# Project configuration
PROJECT_NAME = console-output-provider
PROJECT_FILE = $(PROJECT_NAME).csproj
TARGET_FRAMEWORK = net9.0
RUNTIME_ID = osx-x64
BUILD_CONFIG = Release

# Output directory structure to match dstream.hcl expectations
BIN_DIR = bin/$(BUILD_CONFIG)/$(TARGET_FRAMEWORK)/$(RUNTIME_ID)
TARGET_BINARY = $(BIN_DIR)/$(PROJECT_NAME)

help: ## Show available make targets with descriptions
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

build: ## Build single self-contained binary
build:
	@echo "Building $(PROJECT_NAME)..."
	@mkdir -p $(BIN_DIR)
	/usr/local/share/dotnet/dotnet publish $(PROJECT_FILE) \
		--configuration $(BUILD_CONFIG) \
		--runtime $(RUNTIME_ID) \
		--self-contained true \
		--output $(BIN_DIR) \
		--property:PublishSingleFile=true \
		--property:PublishTrimmed=false \
		--property:IncludeNativeLibrariesForSelfExtract=true
	# Clean up extra files - keep only the main executable
	@echo "Cleaning up deployment files (keeping only single binary)..."
	@find $(BIN_DIR) -type f ! -name "$(PROJECT_NAME)" -delete
	@echo "✅ Single binary created at: $(TARGET_BINARY)"
	@ls -la $(TARGET_BINARY)

clean: ## Remove all build artifacts
clean:
	@echo "Cleaning build artifacts..."
	rm -rf bin/
	rm -rf obj/
	rm -rf out/
	@echo "✅ Clean complete"

verify: ## Check binary exists in correct location
verify:
	@echo "Verifying binary location..."
	@if [ -x "$(TARGET_BINARY)" ]; then \
		echo "✅ Binary found and executable: $(TARGET_BINARY)"; \
		echo "   Size: $$(du -h $(TARGET_BINARY) | cut -f1)"; \
		echo "   Expected by dstream.hcl: ../dstream-console-output-provider/$(BIN_DIR)/$(PROJECT_NAME)"; \
	else \
		echo "❌ Binary not found or not executable: $(TARGET_BINARY)"; \
		exit 1; \
	fi

test: build ## Test provider with sample config
test: build
	@echo "Testing provider with sample config..."
	@echo '{"outputFormat": "simple"}' | $(TARGET_BINARY)

rebuild: clean build ## Clean and build from scratch
