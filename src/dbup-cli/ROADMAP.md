﻿# Road map for v.1.0

## Features to implement

[x] Implement option MarkAsExecuted for upgrade command to mark scrips as executed without actual execution of the scripts
[x] Support input encoding of script files
[x] Script filters
    [x] Wildcards
    [x] Regex

## TODO List

[ ] dbup init -> display information about successful file creation
[ ] dbup status -x -n -> remove redundunt empty line
[x] dbup status --show-executed -> brief option alias can't be -e because it is already used by --env (-e) option
[ ] subFolders: yes -> Displays strange scripts' names: e.g. subfolder.005.sql
[ ] Move logToConsole and logScriptOutput options to command line from configuration file

## Other stuff

[x] Create test for default dbup.yml file
[x] Check the existence of script folder
[x] Check a version of a config file
[x] Target the project as a tool
[ ] Publish an alpha version to NuGet.org and do some tests with it
[ ] Add CI (travis or appveyor)
[ ] Add CD (travis or appveyor) - publish NuGet
[ ] Getting started page
[ ] Wiki documentation
[ ] Start page
[ ] Publish version 1.0
[ ] Create a TFS pipeline task
[ ] Wiki documentation/getting started for the TFS task
[ ] Publish the extension to the marketplace
[ ] Add CI (travis or appveyor) - TFS Task
[ ] Add CD (travis or appveyor) - TFS Task - publish to the marketplace

## Future plans

v.1.1

[ ] Backup a DB before upgrade

v.1.2

[ ] Support for PostgreSql
[ ] Support for MySql