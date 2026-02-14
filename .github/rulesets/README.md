# Branch Protection Rulesets

This directory contains GitHub branch protection rulesets that enforce quality standards before code can be merged.

## 📋 What's Protected

### Main Branch (`main-branch-protection.json`)
- **Pull Request Required**: All changes must go through a PR
- **Required Status Checks**: All CI jobs must pass:
  - Run Tests
  - Build for Windows x64
  - Build for Linux x64
  - Build for Browser WASM
- **Deletion Protection**: Branch cannot be deleted

### Develop Branch (`develop-branch-protection.json`)
- Same protection rules as main branch

## 🚀 How to Apply These Rulesets

**Note:** GitHub rulesets can only be configured through the repository settings UI or via the GitHub API. These JSON files serve as documentation and templates.

### Option 1: GitHub UI (Recommended)

1. Go to your repository on GitHub
2. Click **Settings** → **Rules** → **Rulesets**
3. Click **New ruleset** → **New branch ruleset**
4. Configure the ruleset using the settings from the JSON files:
   - Set the ruleset name
   - Set target branches (e.g., `main` or `develop`)
   - Enable **Require a pull request before merging**
   - Enable **Require status checks to pass**
   - Add the required status check names:
     - `Run Tests`
     - `Build for Windows x64`
     - `Build for Linux x64`
     - `Build for Browser WASM`
   - Enable **Block branch deletion**
5. Click **Create** to apply the ruleset

### Option 2: GitHub API

Use the GitHub REST API to create rulesets programmatically:

```powershell
# Set your GitHub token and repository details
$GITHUB_TOKEN = "your_token_here"
$OWNER = "cptn-fizzbin"
$REPO = "Zylance"

# Create ruleset for main branch
$headers = @{
    "Authorization" = "Bearer $GITHUB_TOKEN"
    "Accept" = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$ruleset = Get-Content ".github/rulesets/main-branch-protection.json" | ConvertFrom-Json
Invoke-RestMethod -Uri "https://api.github.com/repos/$OWNER/$REPO/rulesets" `
    -Method POST `
    -Headers $headers `
    -Body ($ruleset | ConvertTo-Json -Depth 10)
```

## 🔍 What This Does

Once applied, these rulesets will:

✅ **Prevent direct pushes** to main/develop - all changes must go through PRs
✅ **Require all CI builds to pass** - no merging with failing tests or builds
✅ **Protect against accidental deletion** - main/develop branches cannot be deleted
✅ **Maintain code quality** - ensures CSharpier formatting and test coverage

## 🛠️ Customization

### Adding More Required Checks

If you add new CI jobs that should block merging, add them to the `required_status_checks` array:

```json
{
  "context": "Your New Job Name",
  "integration_id": null
}
```

### Strict Status Checks

Currently `strict_required_status_checks_policy` is set to `false`, which means:
- PRs can be merged even if the branch is behind the base branch
- Status checks don't need to be run against the latest commit of the base branch

Set it to `true` if you want to require branches to be up-to-date before merging.

### Bypass Actors

The `bypass_actors` array is empty, meaning no one can bypass these rules. To allow specific users, teams, or apps to bypass:

```json
"bypass_actors": [
  {
    "actor_id": 1,
    "actor_type": "Team",
    "bypass_mode": "always"
  }
]
```

## 📚 More Info

- [GitHub Rulesets Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets)
- [Required Status Checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets#require-status-checks-to-pass)

