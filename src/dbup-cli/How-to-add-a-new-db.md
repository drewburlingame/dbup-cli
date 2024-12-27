# How to add a new database

1. Add a corresponding DbUp NuGet-package. Typically they are named as `dbup-<db-name>`, for example `dbup-mysql`
1. Add a new provider name to the Provider enum in the `ConfigFile/Provider` file
1. Add a new implementation of `DbProvider` in `DbProviders`
1. Create a new integration test in the `dbup-cli.integration-tests` project.
  - Add corresponding NuGet-package to the `dbup-cli.integration-tests` project
  - If the database is a sql database, copy one of the existing tests and update the methods to work with the database
    - otherwise, follow the pattern in `ContainerTest` to create a new test class 
  - Under the `Scripts` folder create a new folder for database scripts for tests. You can copy it from another folder. I don't recommend using one of the existing script folder.
    - Change a provider name in `dbup.yml` files
    - Change SQL in `Timeout` folder because different databases have different syntax for sleep or delay execution
    - Adjust connection strings
