include .pipeline/generic-ci/make/aspire.mk

BACKEND_DIR := backend/Attendify

test:
	cd $(BACKEND_DIR) && dotnet test

build:
	cd $(BACKEND_DIR) && aspire publish

deploy:
	@set -eu; \
	test -n "$${SOPS_AGE_KEY:-}" || { echo "SOPS_AGE_KEY is required" >&2; exit 1; }; \
	command -v sops >/dev/null 2>&1 || { echo "sops must be installed in the deploy environment" >&2; exit 1; }; \
	test ! -e "$(BACKEND_DIR)/tools/AppHost/appsettings.Production.json" || { echo "Refusing to overwrite an existing plaintext Production settings file" >&2; exit 1; }; \
	trap 'rm -f "$(BACKEND_DIR)/tools/AppHost/appsettings.Production.json"' EXIT; \
	sops decrypt "$(BACKEND_DIR)/tools/AppHost/appsettings.Production.sops.json" > "$(BACKEND_DIR)/tools/AppHost/appsettings.Production.json"; \
	cd $(BACKEND_DIR); \
	DOTNET_ENVIRONMENT=Production aspire deploy
