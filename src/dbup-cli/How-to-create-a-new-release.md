# How to create a new release

1. Push a semver tag to github repository. eg. v2.0.0 or v2.0.0-beta
1. GitHub actions will 
   - generate and publish the nuget package 
   - create a draft GitHub release associated with the tag
1. In GitHub, add any additionally useful release notes and publish the release.