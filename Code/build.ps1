param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string] $Version,

    [Parameter(Position = 1, Mandatory = $true)]
    [string] $Date,

    [Parameter(Position = 2, Mandatory = $true)]
    [string] $Platform,

    [Parameter(Position = 3, Mandatory = $true)]
    [scriptblock] $UpdateAssemblyVersion,

    [Parameter(Position = 4, Mandatory = $true)]
    [scriptblock] $UpdateBuildDate,

    [Parameter(Position = 5, Mandatory = $true)]
    [scriptblock] $RevertBuildDate,

    [Parameter(Position = 6, Mandatory = $true)]
    [string] $InstallationFolder
)

$ProjectFolder = "ProteusDS MAST"
$SolutionFile = "ProteusDS MAST.sln"

UpdateAssemblyVersion "$ProjectFolder/Properties/AssemblyInfo.cs" $Version
msbuild $SolutionFile -t:restore -p:RestorePackagesConfig=true
msbuild $SolutionFile /p:Configuration=Release /p:Platform=$Platform
New-Item -Path "$InstallationFolder\ProteusDS MAST" -ItemType Directory
Copy-Item "$SolutionPath\bin\Release\$Platform\*" -Destination "$InstallationFolder\ProteusDS MAST" -Recurse
