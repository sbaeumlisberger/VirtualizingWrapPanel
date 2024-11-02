$gitStatus = git status --porcelain

if ($gitStatus) {
    Write-Host "There are uncommitted changes in the repository. Please commit or stash them before running this script."
    exit 1
}

$currentBranch = git rev-parse --abbrev-ref HEAD

if ($currentBranch -ne "master") {
    Write-Host "You are not on the master branch. Please switch to the master branch before running this script."
    exit 1
}

git fetch

$gitStatus = git status

if ($gitStatus -match "Your branch is ahead") {
    Write-Host "There are local commits that have not been pushed to the remote."
    exit 1
}

dotnet pack /p:ContinuousIntegrationBuild=true