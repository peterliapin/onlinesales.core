# OnlineSales - Extendable Headless CMS

- [Overview](#overview)
- [Sites powered by OnlineSales](#sites-powered-by-onlinesales)
- [Getting started](#getting-started)
    - [Overview of the Solution Structure](#overview-solution-structure)
    - [Pre-requisites for development environment setup](#prerequisites)
    - [Setting up docker containers](#setup-docker-containers)
    - [Secrets management](#secrets-management)
    - [Debugging the project in local environment](#run-project)
    - [Running automated test suite](#run-test-project)
    - [Working with Database Migrations](#working-with-migrations)
- [Plugin integration](#plugin-integration)

<a id="overview"></a>
## Overview

OnlineSale is a light-weight, extendable headless CMS written in .NET 7. It is used to manage content for software product sites as well as automate processes like free trial activation, license generation, automated sales of licenses from website, customer journey tracking and more.

<a id="sites-powered-by-onlinesales"></a>
## Sites powered by OnlineSales

- [XLTools Add-in for Excel](https://xltools.net) - powerful Excel add-in designed for business users
- [GIAnalyzer Add-in for Excel](https://gianalyzer.com) - turns your Excel into a powerful financial software
- [TagPoint](https://tagpoint.co.uk) - easy-to-use app to take care of your facilities | assets | services

<a id="getting-started"></a>
## Getting started

<a id="overview-solution-structure"></a>
### Overview of the Solution Structure

* `OnlineSales` located at /src/OnlineSales folder is the core project which consists of reusable core functionalities described in [Project Overview](#overview).

* `OnlineSales.Tests` located at /tests/OnlineSales.Tests folder is the project which consists of automated unit and integration test suite for `OnlineSales` core project.

* `docker-compose` folder contains the .yml files to spin up docker containers to provide database and logging services for development and testing environments.

* `plugins` folder contains a set of projects which can be plugged into core project via a common interface at the runtime.
    * All the generic functionalities are implemented in the core project, while specific requirements are implemented as separate projects which can be plugged into core project.

<a id="prerequisites"></a>
### Pre-requisites for development environment setup

1. Install docker desktop for windows
    * Download [docker desktop](https://docs.docker.com/desktop/install/windows-install/)

    * Prerequisites for docker installation:
        * `CPU virtualization` should be enabled in BIOS settings.
        * `Hyper-V` and `Containers` windows features should be enabled.

2. Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

3. Install/Upgrade Visual Studio IDE.
    - Visual Studio Version should be `2022 17.4` or higher, which supports .NET 7

4. Install [CLI tools for Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).

<a id="setup-docker-containers"></a>
### Setting up docker containers

OnlineSales platform integrates with two databases initially.
1. PostgreSQL
2. Elasticsearch

* Docker containers should be created to run database services for development and testing environments.

#### Setting up docker containers to run automated test suite.

1. Navigate to `docker-compose` folder in the solution.

2. Open `docker-compose.tests.yml` docker file and update if necessary.

3. Open a command line tool in the same folder location.

4. Run `docker compose up` command.

5. Open docker desktop and verify Postgres and Elasticsearch containers are up and running.

#### Setting up docker containers for development environment

1. Navigate to `docker-compose` folder in the solution.

2. Open `docker-compose.development.yml` docker file and update if necessary.

3. Open a command line tool in the same folder location.

4. Run `docker compose up` command.

5. Open docker desktop and verify Postgres and Elasticsearch containers are up and running.

<a id="secrets-management"></a>
### Secrets management

Any secrets such as user credentials should not be stored in appsettings.json file.
- Ex: Do not store `Password` for `Postgres` and `Elastic` sections of appsettings.json file.

Secrets of the appsettings.json file should hold placeholders for environment variables.
- Ex:
```
"Email": {
    "Server": "$EMAIL__SERVER",
    "UserName": "$EMAIL__USERNAME",
    "Password": "$EMAIL__PASSWORD"
  }
```
* Note the format of the placeholder: Nested configuration keys are combined with a double underscore and written in capital letters.
    - Ex: Configuration key `Email:Server` should be written as `$EMAIL__SERVER`

#### Updating appsettings secrets in local environment

Visual Studio in-built [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#secret-manager) tool can be used to manage all secrets in local development environment.

It will maintain a `secrets.json` file internally to store any configuration.

`secret.json` file will not be version controlled by default.

* In Visual Studio, by default Secret Manager can be accessed from UI.

* For Visual Studio Code, to enable it from UI, "Manage User Secrets" extension can be used.

* For any tool, Secret Manager can be used with dotnet commands

##### How to use `Secret Manager` feature in Visual Studio:

1. Right-click on `OnlineSales` project and click on `Manage User Secrets`.

2. Update the `secrets.json` file with required secrets in the format same as the `appsettings.json` file.

Secrets of the `appsettings.tests.json` file can also be managed using Secret Manager in the test project.

At runtime, secrets of `appsettings.json` file (or appsettings.tests.json) will be replaced by the values of `secrets.json`.

#### Updating appsettings secrets in pipeline

Secrets are stored as `Variables` in the pipeline and can be marked as secrets.

Pipeline variables should be assigned to Environment variables in the pipeline script.

At pipeline runtime, secrets of `appsettings.json` file (or appsettings.unittest.json) will be replaced by the corresponding environment variables.

<a id="run-project"></a>
### Debugging the project in local environment

1. Make sure above pre-requisites and configuration settings are completed.

2. Run the project from toolbar

3. Verify whether the swagger documentation is opened in the default browser window.

<a id="run-test-project"></a>
### Running automated test suite

1. Locate the project `OnlineSales.Tests`.

2. Locate `appsettings.tests.json` file and verify the database credentials.
    * Configurations available in `appsettings.tests.json` file will replace the same configurations in `appsettings.json` file at run time.
    * Refer [Secrets Management](#secrets-management) for more details on storing credentials.

3. Right-click on the project and click on `Run Tests` or `Debug Tests`.

<a id="working-with-migrations"></a>
### Working with Database Migrations

* Database will be created automatically using generated migration scripts when starting the application.
    * Application will first apply the migrations of core project followed by migrations of plugin projects

* However, migrations can be applied manually if need using [entity framework core commands](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#using-the-tools) in a command line interface or package manager console.

* When a model is updated, a new migration script should be generated to update the database using [entity framework core commands](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#using-the-tools) following the existing naming conventions.

<a id="plugin-integration"></a>
## Plugin integration
