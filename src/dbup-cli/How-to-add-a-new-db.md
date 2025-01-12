# How to add a new database

1. Add a corresponding DbUp NuGet-package. Typically they are named as `dbup-<db-name>`, for example `dbup-mysql`
1. Add a new implementation of `DbProvider` in `DbProviders`
1. Add an instance `Providers.ProviderMap`
1. Create a new integration test in the `dbup-cli.integration-tests` project.
  - Add corresponding NuGet-package to the `dbup-cli.integration-tests` project
  - If the database is a relational database, copy one of the existing tests and update the methods to work with the database
    - otherwise, follow the pattern in `ContainerTest` to create a new test class 
  - Under the `Scripts` folder...
    - in `Timeout` folder, add a folder with the provider name and a script to sleep for 20 seconds.
    - in `JournalTableScript` folder, if `dbup.yml` the custom table works for the new db then there is nothing to do.  Otherwise, copy dbup.yml to a new file with the provider name and update the table and/or schema.  See the `SqlServer.yml` file as an example.
