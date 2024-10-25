$gitStatus = git status --porcelain

if ($gitStatus) {
    Write-Host "There are uncommitted changes in the repository. Please commit or stash them before running this script."
    exit 1
}

dotnet pack /p:ContinuousIntegrationBuild=true