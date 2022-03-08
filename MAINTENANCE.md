## Creating a release

**Serilog.Formatting.Log4Net** uses [MinVer](https://github.com/adamralph/minver) for its versioning, so a tag must exist with the chosen semantic version number in order to create an official release.

1.  Update the [CHANGELOG](CHANGELOG.md) *Unreleased* section to the chosen version and copy the release notes for step #2.

   - Add the release date
   - Update the link from `HEAD` to the chosen version

2.  Create an **[annotated](https://stackoverflow.com/questions/11514075/what-is-the-difference-between-an-annotated-and-unannotated-tag/25996877#25996877)** tag, the (multi-line) message of the annotated tag will be the content of the GitHub release. Markdown (copied from step #1) should be used.

   `git tag --annotate 1.0.0-rc.1`

3.  Push the `main` branch and ensure that the [build is successful](https://github.com/serilog-contrib/serilog-formatting-log4net/actions).

   `git push`
   
4. [Push the tag](https://stackoverflow.com/questions/5195859/how-do-you-push-a-tag-to-a-remote-repository-using-git/26438076#26438076)

   `git push --follow-tags`

Once pushed, the GitHub [Continuous Integration](https://github.com/serilog-contrib/serilog-formatting-log4net/blob/main/.github/workflows/continuous-integration.yml) workflow takes care of building, running the tests, creating the NuGet package, creating the GitHub release and finally publishing the produced NuGet package.

After the NuGet package is succesfully published:

4.  Update the `PackageValidationBaselineVersion` element in the `Serilog.Formatting.Log4Net.csproj` file to the newly released version.
5.  Delete the `CompatibilitySuppressions.xml` file if there's one.
