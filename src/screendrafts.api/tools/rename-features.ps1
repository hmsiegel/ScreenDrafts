<#
.SYNOPSIS
    Renames generic Command/Query files and classes to descriptive names based on their feature context.

.DESCRIPTION
    This script:
    1. Finds all Command.cs, CommandHandler.cs, Query.cs, QueryHandler.cs, and Request.cs files
    2. Renames files and classes based on their parent folder structure
    3. Updates all references throughout the codebase
    4. Creates a detailed log of all changes

.EXAMPLE
    .\rename-features.ps1

.EXAMPLE
    .\rename-features.ps1 -DryRun
    (Shows what would be renamed without making changes)
#>

[CmdletBinding()]
param(
    [switch]$DryRun,

    [string]$RootPath = "src\modules",

    [switch]$Detailed
)

# Configuration
$ErrorActionPreference = "Stop"
$logFile = "rename-features-log-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"

# Explicit entity naming map (reliable; avoids bad singularization like Movies -> Movy)
# Extend as needed.
$EntityMap = @{
    "Movies"      = @{ Singular = "Movie";     Plural = "Movies" }
    "People"      = @{ Singular = "Person";    Plural = "People" }
    "Drafts"      = @{ Singular = "Draft";     Plural = "Drafts" }
    "DraftParts"  = @{ Singular = "DraftPart"; Plural = "DraftParts" }
    "Categories"  = @{ Singular = "Category";  Plural = "Categories" }
    "Campaigns"   = @{ Singular = "Campaign";  Plural = "Campaigns" }
    "Drafters"    = @{ Singular = "Drafter";   Plural = "Drafters" }
    "Hosts"       = @{ Singular = "Host";      Plural = "Hosts" }
}

function Get-EntityInfo {
    param([string]$FeatureArea)

    if ($EntityMap.ContainsKey($FeatureArea)) {
        return $EntityMap[$FeatureArea]
    }

    # Safe fallback: trim a single trailing 's' if present.
    # (No ies->y rule here; it produces junk like Movies -> Movy.)
    $singular = $FeatureArea
    if ($singular.EndsWith("s")) {
        $singular = $singular.Substring(0, $singular.Length - 1)
    }

    return @{ Singular = $singular; Plural = $FeatureArea }
}

function Build-UseCasePrefix {
    param(
        [string]$UseCase,
        [string]$EntityPart
    )

    # Avoid duplicated entity suffixes (e.g., CreateDraftDraft)
    if ($UseCase -like "*$EntityPart*") {
        return $UseCase
    }

    return "$UseCase$EntityPart"
}

# Helper function to write to both console and log
function Write-Log {
    param($Message, $Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $logFile -Value $logMessage
}

# Helper function to get new names based on folder structure
function Get-NewNames {
    param(
        [string]$FilePath,
        [string]$CurrentFileName
    )

    $directory = Split-Path -Parent $FilePath
    $parts = $directory -split [regex]::Escape([IO.Path]::DirectorySeparatorChar)

    # Find the feature area and use case
    # Pattern: ...\Features\{FeatureArea}\{UseCase}\
    $featuresIndex = -1
    for ($i = 0; $i -lt $parts.Count; $i++) {
        if ($parts[$i] -eq "Features" -or $parts[$i] -like "*.Features") {
            $featuresIndex = $i
            break
        }
    }

    if ($featuresIndex -eq -1 -or $featuresIndex + 2 -ge $parts.Count) {
        return $null
    }

    $featureArea = $parts[$featuresIndex + 1]  # e.g., "Campaigns"
    $useCase = $parts[$featuresIndex + 2]      # e.g., "Get", "Create"

    # Determine singular or plural for feature
    $entity = Get-EntityInfo -FeatureArea $featureArea

    # List/Search should use the plural entity part for BOTH Query and Request
    $entityPart = $entity.Singular
    if ($useCase -in @("List", "Search")) {
        $entityPart = $entity.Plural
    }

    $prefix = Build-UseCasePrefix -UseCase $useCase -EntityPart $entityPart

    $newNames = @{}

    switch ($CurrentFileName) {
        "Command.cs" {
            $newNames.FileName = "$prefix`Command.cs"
            $newNames.ClassName = "$prefix`Command"
            $newNames.OldClassName = "Command"
        }
        "CommandHandler.cs" {
            $newNames.FileName = "$prefix`CommandHandler.cs"
            $newNames.ClassName = "$prefix`CommandHandler"
            $newNames.OldClassName = "CommandHandler"
        }
        "Query.cs" {
            $newNames.FileName = "$prefix`Query.cs"
            $newNames.ClassName = "$prefix`Query"
            $newNames.OldClassName = "Query"
        }
        "QueryHandler.cs" {
            $newNames.FileName = "$prefix`QueryHandler.cs"
            $newNames.ClassName = "$prefix`QueryHandler"
            $newNames.OldClassName = "QueryHandler"
        }
        "Request.cs" {
            $newNames.FileName = "$prefix`Request.cs"
            $newNames.ClassName = "$prefix`Request"
            $newNames.OldClassName = "Request"
        }
        default {
            return $null
        }
    }

    $newNames.FeatureArea = $featureArea
    $newNames.UseCase = $useCase
    $newNames.Namespace = Get-NamespaceFromFile -FilePath $FilePath

    if (-not $newNames.Namespace) {
        return $null
    }

    return $newNames
}

function Get-NamespaceFromFile {
    param([string]$FilePath)

    $content = Get-Content -Path $FilePath -Raw

    # file-scoped: namespace Foo.Bar;
    if ($content -match '(?m)^\s*namespace\s+([A-Za-z0-9_.]+)\s*;') {
        return $Matches[1]
    }

    # block-scoped: namespace Foo.Bar {
    if ($content -match '(?m)^\s*namespace\s+([A-Za-z0-9_.]+)\s*\{') {
        return $Matches[1]
    }

    return $null
}

# Helper function to update file content
function Update-FileContent {
    param(
        [string]$FilePath,
        [string]$OldClassName,
        [string]$NewClassName
    )

    $content = Get-Content -Path $FilePath -Raw

    # Replace class/record declarations
    $patterns = @(
        "class $OldClassName\b",
        "record $OldClassName\b",
        "interface $OldClassName\b"
    )

    foreach ($pattern in $patterns) {
        $replacement = $pattern -replace $OldClassName, $NewClassName
        $content = $content -replace $pattern, $replacement
    }

    if (-not $DryRun) {
        Set-Content -Path $FilePath -Value $content
    }
}

# Helper function to find and update references
function Update-References {
    param(
        [string]$RootPath,
        [string]$OldClassName,
        [string]$NewClassName,
        [string]$Namespace
    )

    $csFiles = Get-ChildItem -Path $RootPath -Filter "*.cs" -Recurse
    $updatedFiles = @()

    foreach ($file in $csFiles) {
        $content = Get-Content -Path $file.FullName -Raw
        $originalContent = $content

        # Pattern to match the class name in various contexts
        # Match as word boundary to avoid partial matches
        $patterns = @(
            "\b$OldClassName\b"
        )

        $hasMatch = $false
        foreach ($pattern in $patterns) {
            if ($content -match $pattern) {
                $hasMatch = $true
                break
            }
        }

        if ($hasMatch) {
            # Only replace if it's in the same namespace or a using statement exists
            $shouldReplace = $false

            $nsEsc = [regex]::Escape($Namespace)

            # matches both `namespace X.Y.Z;` and `namespace X.Y.Z {`
            if ($content -match "(?m)^\s*namespace\s+$nsEsc(\s*;|\s*\{)") {
                $shouldReplace = $true
            }
            elseif ($content -match "(?m)^\s*using\s+$nsEsc\s*;") {
                $shouldReplace = $true
            }
            elseif ($content -match "\b$nsEsc\.$OldClassName\b") {
                # fully-qualified usage: Namespace.Command
                $shouldReplace = $true
            }

            if ($shouldReplace) {
                foreach ($pattern in $patterns) {
                    $content = $content -replace $pattern, $NewClassName
                }

                if ($content -ne $originalContent) {
                    if (-not $DryRun) {
                        Set-Content -Path $file.FullName -Value $content
                    }
                    $updatedFiles += $file.FullName
                }
            }
        }
    }

    return $updatedFiles
}

# Main execution
Write-Log "========================================" -Color Cyan
Write-Log "Feature Rename Script Started" -Color Cyan
Write-Log "Root Path: $RootPath" -Color Cyan
Write-Log "Dry Run: $DryRun" -Color Cyan
Write-Log "Resolved Root Path: $(Resolve-Path $RootPath)" -Color Cyan
Write-Log "========================================" -Color Cyan
Write-Log ""

$filesToRename = @(
    "Command.cs",
    "CommandHandler.cs",
    "Query.cs",
    "QueryHandler.cs",
    "Request.cs"
)

# Find all files to rename
$allFilesToProcess = @()
foreach ($fileName in $filesToRename) {
    $files = Get-ChildItem -Path $RootPath -Filter $fileName -Recurse
    foreach ($file in $files) {
        # Skip if not in Features folder
        if ($file.DirectoryName -notmatch '(\\|/)(Features|[^\\\/]*\.Features)(\\|/)') {
            continue
        }

        $newNames = Get-NewNames -FilePath $file.FullName -CurrentFileName $fileName
        if ($newNames) {
            $allFilesToProcess += @{
                File     = $file
                NewNames = $newNames
            }
        }
    }
}

Write-Log "Found $($allFilesToProcess.Count) files to process" -Color Yellow
Write-Log ""

# Group by namespace to handle references efficiently
$byNamespace = $allFilesToProcess | Group-Object { $_.NewNames.Namespace }

$totalRenamed = 0
$totalReferencesUpdated = 0

foreach ($group in $byNamespace) {
    Write-Log "Processing namespace: $($group.Name)" -Color Magenta
    Write-Log "----------------------------------------" -Color Magenta

    foreach ($item in $group.Group) {
        $file = $item.File
        $newNames = $item.NewNames

        $newFilePath = Join-Path -Path $file.DirectoryName -ChildPath $newNames.FileName

        Write-Log "  File: $($file.Name)" -Color White
        Write-Log "    → New File: $($newNames.FileName)" -Color Green
        Write-Log "    → Old Class: $($newNames.OldClassName)" -Color White
        Write-Log "    → New Class: $($newNames.ClassName)" -Color Green

        if (-not $DryRun) {
            # Update the class name in the file content
            Update-FileContent -FilePath $file.FullName -OldClassName $newNames.OldClassName -NewClassName $newNames.ClassName

            # Rename the file
            if (Test-Path $newFilePath) {
                Write-Log "    ⚠ Warning: Target file already exists: $newFilePath" -Color Yellow
            }
            else {
                Rename-Item -Path $file.FullName -NewName $newNames.FileName
                $totalRenamed++
            }
        }
        else {
            $totalRenamed++
        }

        # Update references in other files
        Write-Log "    → Searching for references..." -Color Cyan
        $updatedFiles = Update-References -RootPath $RootPath -OldClassName $newNames.OldClassName -NewClassName $newNames.ClassName -Namespace $newNames.Namespace

        if ($updatedFiles.Count -gt 0) {
            Write-Log "    → Updated $($updatedFiles.Count) reference file(s)" -Color Green
            if ($Detailed) {
                foreach ($updatedFile in $updatedFiles) {
                    Write-Log "      • $updatedFile" -Color DarkGray
                }
            }
            $totalReferencesUpdated += $updatedFiles.Count
        }

        Write-Log ""
    }
}

Write-Log "========================================" -Color Cyan
Write-Log "Summary" -Color Cyan
Write-Log "========================================" -Color Cyan
Write-Log "Total files renamed: $totalRenamed" -Color Green
Write-Log "Total reference files updated: $totalReferencesUpdated" -Color Green
Write-Log "Log file: $logFile" -Color Cyan
Write-Log ""

if ($DryRun) {
    Write-Log "This was a DRY RUN - no changes were made" -Color Yellow
    Write-Log "Run without -DryRun to apply changes" -Color Yellow
}
else {
    Write-Log "✓ Renaming completed successfully!" -Color Green
    Write-Log ""
    Write-Log "Recommended next steps:" -Color Yellow
    Write-Log "1. Build the solution to verify no compilation errors" -Color White
    Write-Log "2. Run tests to ensure functionality is intact" -Color White
    Write-Log "3. Review the log file for details: $logFile" -Color White
    Write-Log "4. Commit changes to source control" -Color White
}

Write-Log ""
Write-Log "========================================" -Color Cyan