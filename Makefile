include .pipeline/generic-ci/make/aspire.mk

BACKEND_DIR := backend/Attendify

test:
	cd $(BACKEND_DIR) && dotnet test

build:
	cd $(BACKEND_DIR) && aspire publish

deploy:
	cd $(BACKEND_DIR) && aspire deploy
