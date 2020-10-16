# Parse version number from source control tags
$VersionRegex = "\d+\.\d+\.\d+(\.\d+)?"
$VersionNumber = [regex]::matches($Env:BUILD_VERSION, $VersionRegex)[0]

if(!$VersionNumber.Success)
{
    Write-Error "Couldn't parse semantic version from tag: $Env:BUILD_VERSION"
}

if(!$VersionNumber.Groups[1].Success)
{
    # MSBuild expects 4 part version numbers; add build number from GitHub Actions
    $VersionNumber = "$VersionNumber.${Env:GITHUB_RUN_NUMBER}"
}

Write-Host "Source version number: $Env:BUILD_VERSION"
Write-Host "Parsed version number: $VersionNumber"

# Get friendly-length commit hash
$CommitHash = $Env:GITHUB_SHA.Substring(0, 7)

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
        $content -replace $VersionRegex, $VersionNumber | Out-File $file
        # Append Informational Version with commit hash; appears as "Product Version"
        "[assembly: AssemblyInformationalVersion(`"$VersionNumber-$CommitHash`")]" >> $file
    }
}
else
{
    Write-Warning "Couldn't find any AssemblyInfo files to update!"
}
