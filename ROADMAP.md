## Improved testing

*[x] TestContainers
*[x] DRY up the tests
*[x] assert failure reason when checking for exit code != 0
*[ ] is https://github.com/drewburlingame/dbup-cli/issues/8 still a problem?
  * if not, what is the result of status for runAlways scripts? what about for a new runAlways script?
*[ ] integration tests, assert a table is or is not created, not just the journal entry. 
  *[ ] reenable MySql timeout test to see if table is not created on timeout but journal is updated 
*[ ] inline config files so they're easy to view with the tests instead of forcing to navigate to a separate folder
*[ ] move configs that aren't inlined into folders with the tests so it's more obvious how they are used.
*[x] use Verify for tests
*[x] organize unit tests by command where possible
*[x] convert to xunit or nunit
*[x] remove Option to simplify code
*[x] convert to CommandDotNet to simplify command definitions 
  *[x] xunit logging to console
  *[x] DropDb snapshot
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
