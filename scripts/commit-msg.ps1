#!powershell

param(
    [Parameter(Mandatory = $true)]
    [string]$CommitMsgFile
)

# Read the commit message from the provided file
$commit_msg = Get-Content -Raw -Path $CommitMsgFile

# Define the conventional commit regex pattern
$conventional_regex = '^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(!)?(\([a-z0-9-]+\))?: .+$'

# Check if the commit message matches the conventional commit pattern
if (-not ($commit_msg -match $conventional_regex)) {
    Write-Host "-------------------------------------------------------------------------"
    Write-Host " ❌ The commit message does not follow Conventional Commits specification"
    Write-Host "     (see https://www.conventionalcommits.org/en/v1.0.0):"
    Write-Host " Format: type(scope)?(!)?: subject"
    Write-Host " Allowed types:"
    Write-Host "     feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert"
    Write-Host " Example: feat(api)!: Add validation to the create-user endpoint"
    Write-Host "-------------------------------------------------------------------------"
    exit 1
}

