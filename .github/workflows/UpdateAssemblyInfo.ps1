# Parse version number from source control tags
$VersionRegex = "\d+\.\d+\.\d+(\.\d+)?"
$VERSION_NUMBER = [regex]::matches($Env:BUILD_VERSION, $VersionRegex)[0]

if(!$VERSION_NUMBER.Success)
{
    Write-Error "Couldn't parse semantic version from tag: $Env:BUILD_VERSION"
}

if(!$VERSION_NUMBER.Groups[1].Success)
{
    # MSBuild expects 4 part version numbers; add build number from GitHub Actions
    $VERSION_NUMBER = "$VERSION_NUMBER.${Env:GITHUB_RUN_NUMBER}"
}

Write-Host "Source version number: $Env:BUILD_VERSION"
Write-Host "Parsed version number: $VERSION_NUMBER"

# Get friendly-length commit hash
$COMMIT_HASH = $Env:GITHUB_SHA.Substring(0, 7)

echo "VERSION_NUMBER=${VERSION_NUMBER}" >> $GITHUB_ENV
echo "COMMIT_HASH=${COMMIT_HASH}" >> $GITHUB_ENV

# Find every AssemblyInfo.cs
$files = Get-ChildItem -Path "${Env:GITHUB_WORKSPACE}\**\Properties" -Recurse -include "AssemblyInfo.*"

# Update version number in AssemblyInfo.cs
if($files)
{
    foreach ($file in $files) {
        Write-Host "Updating version in $file"
        # Disable Read-Only
        attrib $file -r
        # Find and Replace in AssemblyInfo.cs
        $content = Get-Content($file)
        $content -replace $VersionRegex, $VERSION_NUMBER | Out-File $file
        # Append Informational Version with commit hash; appears as "Product Version"
        "[assembly: AssemblyInformationalVersion(`"$VERSION_NUMBER-$COMMIT_HASH`")]" >> $file
    }
}
else
{
    Write-Warning "Couldn't find any AssemblyInfo files to update!"
}
