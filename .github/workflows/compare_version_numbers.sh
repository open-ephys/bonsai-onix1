#! /bin/bash

version_current=$(cat ./Directory.Build.props | grep -Po '(?<=VersionPrefix>).*(?=</VersionPrefix>)')
version_release=$(cat ./last-release/Directory.Build.props | grep -Po '(?<=VersionPrefix>).*(?=</VersionPrefix>)')

echo "Current Version: $version_current"
echo "Release Version: $version_release"

if [ ! $(python -c "import semver; print(semver.compare(\"$version_current\", \"$version_release\"))") == 1 ]; then
  echo "::error title=Invalid version number::Version number must be increased"
  exit 1
fi
