# AvalonDock framework dependency

The compatibility branch uses opt-in Avalonia APIs for binding ownership, logical
children, hosted native roots, initialization, pointer capture, and compiled
resource factories. The supporting changes are reviewed in
[Avalonia fork PR #9](https://github.com/wieslawsoltes/Avalonia/pull/9), based on
the exact Avalonia 12.1.3 source commit. This is a prerelease dependency; these APIs
are not part of the public 12.1.3 NuGet release.

Before restoring this branch, run from the repository root:

```sh
python3 build/AvalonDock.Framework/bootstrap.py
dotnet build -c Release
```

The bootstrap checks out the source revision in `framework.json`, initializes its
submodules, and uses Avalonia's own package merge, build-task rewriting, signing,
and reference-assembly generation. It produces the `Avalonia` and
`Avalonia.Remote.Protocol` packages and symbols in
`artifacts/avalondock-framework/nuget`. `NuGet.Config` includes that local feed.
The other Avalonia packages retain their published versions.

CI caches the four packages by the source specification and bootstrap script;
the script verifies every package hash before reusing the cache. No official
NuGet package is overwritten, and no reflection is added to the compatibility
runtime. The source checkout and all bootstrap outputs are generated artifacts.

`--source-tree <directory>` can reuse a clean checkout at the pinned revision.
`--output <directory>` selects another feed directory. Package consumers need
both the Dock packages and this framework feed until the required framework
APIs have a published upstream release. CI includes both sets in its package
artifacts; publishing the Dock package alone would leave an unresolved dependency.

Release and nightly workflows provide downloadable bundles containing both Dock
and framework packages. Extract the bundle into a local NuGet feed, or publish
its packages to a private feed you control. The release bundle and GitHub release
assets remain available when public-feed publishing is skipped.

Before publishing Dock packages, CI checks that both exact framework package
versions already exist at the destination feed. If either is missing, or the feed
cannot be checked, CI skips that publishing step and records the reason in its
summary. These workflows never publish `Avalonia` package IDs with Dock's NuGet
credentials. Availability on nuget.org or another destination is an external
dependency; a successful bundle build does not establish public-feed availability.

Update the full commit and package version together when framework changes are
required. Use a new prerelease version for each changed package build; do not
replace a version already restored by a consumer.
