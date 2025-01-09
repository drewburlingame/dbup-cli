## Improved testing

*[x] TestContainers
*[x] DRY up the tests
*[x] assert failure reason when checking for exit code != 0
*[ ] integration tests, assert a table is or is not created, not just the journal entry. 
  *[ ] reenable MySql timeout test to see if table is not created on timeout but journal is updated 
*[ ] inline config files so they're easy to view with the tests instead of forcing to navigate to a separate folder
*[ ] move configs that aren't inlined into folders with the tests so it's more obvious how they are used.
*[x] use Verify for tests
*[x] organize unit tests by command where possible
*[x] convert to xunit or nunit
*[x] remove Option to simplify code
*[ ] convert to CommandDotNet to simplify command definitions 
  *[ ] xunit logging to console
  *[ ] DropDb snapshot 
  * Breaking Changes
    * `version` command is now `--version` option
    * `--env` must now be repeated for each file. `--env file1.env file2.env` is now `--env file1.env --env file2.env`
    * case-sensitive enums. verbosity must be lowercase - `-v detail`
*[ ] structured logging for better debugging and monitoring in deployed environments

---

This is a trimmed list from the original repo. Keeping here until I can evaluate and rehome the tasks to take on.

## TODO List

*[ ] dbup init -> display information about successful file creation
*[ ] dbup status -x -n -> remove redundant empty line
*[ ] subFolders: yes -> Displays strange scripts' names: e.g. subfolder.005.sql

## Considerations

*[ ] Publish the extension to the marketplace
*[ ] Drop database for PostgreSQL and MySQL
*[ ] Backup a database
*[ ] Automatic backup a database before upgrade
*[ ] Support of Sqlite
