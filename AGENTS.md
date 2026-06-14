# AGENTS.md

# ==================================================

# 1. MANDATORY INSTRUCTIONS

# ==================================================

Always read this entire file before analyzing, generating, modifying, deleting, moving, or integrating any code.

This file contains mandatory project-specific rules.

Preserving existing functionality is the highest priority.

The current project is the official source of truth.

Code provided by another team member must be treated as a feature to integrate, not as a replacement for the current project.

If a request conflicts with these rules, creates a high risk of breaking existing functionality, or requires changes outside the requested scope:

1. Stop before making the risky change.
2. Explain the conflict or risk.
3. List the affected files and functionality.
4. Ask for explicit confirmation before continuing.

Never claim that the project works correctly unless the relevant checks have actually been performed.

---

# ==================================================

# 2. PROJECT OVERVIEW

# ==================================================

This is an ASP.NET MVC project.

The project is currently under development by a team of three members.

Each member may work on a different feature, but all members develop from the same base project.

The project owner is responsible for:

* Maintaining the official base project.
* Receiving code from team members.
* Integrating features.
* Resolving conflicts.
* Testing the complete project.
* Merging stable code into `main`.

Many existing features already work correctly.

The goal is to continue development safely without breaking existing functionality.

Do not assume that:

* The current project structure needs improvement.
* Existing naming is incorrect.
* Existing architecture should be replaced.
* Existing code should be refactored.
* A newer implementation is automatically better.
* Code from a team member should replace the current implementation.

Follow the project's existing structure, conventions, and coding style.

---

# ==================================================

# 3. SOURCE OF TRUTH

# ==================================================

The code currently present in the project is the official base version.

When integrating code from another team member:

* Treat incoming code as a partial feature.
* Compare incoming code with the current project.
* Integrate only the required functionality.
* Preserve all existing working functionality.
* Do not overwrite an entire existing file without first comparing both versions.
* Do not assume the incoming version is newer or more correct.
* Do not replace the current project structure with the incoming structure.
* Do not copy an entire incoming folder over an existing folder without analysis.

When two implementations conflict, preserve the current behavior unless the user explicitly requests replacement.

---

# ==================================================

# 4. LANGUAGE AND NAMING RULES

# ==================================================

This project intentionally uses Vietnamese names without accents.

Examples:

* NhanVien
* KhachHang
* SanPham
* DonHang
* NhaCungCap
* KhuyenMai

These names are intentional.

DO NOT automatically translate them into English.

DO NOT rename existing:

* Classes
* Models
* Entities
* Properties
* Methods
* Variables
* Controllers
* Views
* Tables
* Columns
* Routes
* Files
* Folders

Keep the naming style already used by the project.

Examples to keep:

* NhanVien
* HoTen
* ChucVu
* DienThoai
* NgayVaoLam

Do not automatically change them to:

* Employee
* FullName
* Position
* PhoneNumber
* HireDate

New names must follow the naming style of the surrounding code.

Do not mix English naming with Vietnamese naming in the same feature unless the existing project already does so or the user explicitly requests it.

Comments and explanations may be written in Vietnamese when that helps team members understand the code.

---

# ==================================================

# 5. DATABASE RULES

# ==================================================

Database structures may use Vietnamese names without accents.

This is intentional.

Do not rename existing:

* Databases
* Tables
* Columns
* Primary keys
* Foreign keys
* Models
* Entities
* Property names
* Relationships
* Stored procedures
* Views
* Constraints

unless explicitly requested.

Do not automatically translate database names into English.

Do not:

* Delete a table.
* Delete a column.
* Rename a table or column.
* Change a data type.
* Change nullability.
* Change a primary key.
* Change a foreign key.
* Remove a relationship.
* Delete existing data.
* Recreate the database.
* Replace the current schema.

unless explicitly requested.

Before changing the database, inspect all related:

* Models.
* DbContext or database context.
* Migrations.
* Repositories.
* Services.
* Controllers.
* ViewModels.
* Views.
* API endpoints.
* JavaScript code.
* Validation rules.

Any database change must be documented clearly.

When a schema change is necessary, explain:

1. What will change.
2. Why it is required.
3. Which code depends on it.
4. Whether a migration is required.
5. Whether existing data may be affected.
6. How the change can be rolled back.

Never generate a destructive migration without explicit confirmation.

---

# ==================================================

# 6. CODE SAFETY RULES

# ==================================================

Before modifying code:

1. Read this `AGENTS.md` file.
2. Inspect the project structure.
3. Read the complete content of directly related files.
4. Understand the current implementation.
5. Search for references to the code being changed.
6. Check imports, namespaces, dependencies, and inheritance.
7. Check routes, models, controllers, views, and database relationships.
8. Identify existing functionality that may be affected.
9. Determine the smallest safe change.
10. Explain the planned modification before applying it.

Never modify code you do not understand.

Never rely only on file names to guess their content.

Never assume unused-looking code is safe to remove.

Never remove existing functionality unless explicitly requested.

Never hide errors by removing validation, exception handling, authorization, or business rules.

Do not replace working code merely because another implementation appears cleaner.

---

# ==================================================

# 7. CHANGE SCOPE AND LIMITS

# ==================================================

Always make the smallest possible change that satisfies the request.

Prefer fixing the specific problem.

Avoid:

* Large refactoring.
* Architecture changes.
* Mass renaming.
* Broad formatting changes.
* Unnecessary optimization.
* Rewriting entire files.
* Replacing working implementations.
* Adding unrelated functionality.
* Changing code outside the requested feature.
* Updating framework or package versions unnecessarily.

Examples:

* If asked to fix login, do not redesign the homepage.
* If asked to integrate a product page, do not modify employee management.
* If asked to fix CSS, do not change database models.
* If asked to add a controller action, do not rewrite the whole controller.
* If asked to fix one route, do not restructure all routing.

If more than 3 existing files need to be modified:

STOP before modifying them.

Explain:

* Why more than 3 files are required.
* Which files need modification.
* What will change in each file.
* Which existing features may be affected.
* Whether there is a smaller alternative.

Wait for explicit confirmation before continuing.

Exceptions to the 3-file limit:

* The user explicitly approves a larger change.
* The user explicitly requests a project-wide change.
* Multiple files are generated automatically by a framework command and the user has approved that command.

Even with approval, modify only the necessary files.

---

# ==================================================

# 8. EXISTING FUNCTIONALITY PROTECTION

# ==================================================

Existing working functionality is more important than:

* Naming improvements.
* Architecture improvements.
* Formatting improvements.
* Code style improvements.
* Performance optimization.
* Reducing duplication.
* Using newer patterns.
* Replacing an older library.
* Making code appear cleaner.

Never break an existing feature to improve code quality.

Never remove or change an existing behavior unless the request requires it.

When modifying shared code, identify all features that depend on it.

Shared code includes:

* Layouts.
* Navigation.
* Authentication.
* Authorization.
* Session handling.
* Routing.
* Database context.
* Models.
* ViewModels.
* Services.
* Repositories.
* Helpers.
* Middleware.
* Dependency injection.
* Configuration.
* Shared JavaScript.
* Shared CSS.

After modification, verify that related existing behavior remains intact.

---

# ==================================================

# 9. TEAM CODE INTEGRATION RULES

# ==================================================

When receiving code from another team member, do not immediately copy or overwrite files.

Use the following integration process:

1. Identify the feature provided by the team member.
2. Identify all incoming files that were added, modified, renamed, or deleted.
3. Compare incoming files with the current project.
4. Identify files that exist in both versions.
5. Identify shared components and dependencies.
6. Identify potential conflicts.
7. Determine which parts of the incoming code are actually required.
8. Integrate the feature incrementally.
9. Preserve existing functionality.
10. Test after each logical integration step.
11. Report every file that was added, modified, or intentionally not used.

Pay special attention to:

* `Program.cs`
* `Startup.cs`
* `appsettings.json`
* `appsettings.Development.json`
* Project `.csproj` files
* Solution `.sln` files
* Route configuration
* Dependency injection registration
* DbContext
* Models
* Controllers
* Shared layouts
* `_ViewImports.cshtml`
* `_ViewStart.cshtml`
* Authentication configuration
* Authorization configuration
* Session configuration
* Shared CSS
* Shared JavaScript
* Database migrations

Do not copy incoming versions of these files over current versions without a manual comparison.

Incoming code must be adapted to the current project, not the opposite.

---

# ==================================================

# 10. CONFLICT RESOLUTION RULES

# ==================================================

When current code and incoming code modify the same file:

* Do not automatically keep only the current version.
* Do not automatically keep only the incoming version.
* Do not blindly use “Accept Current Change.”
* Do not blindly use “Accept Incoming Change.”
* Do not replace the entire file.
* Do not remove code simply to make the conflict disappear.

Instead:

1. Understand the purpose of the current implementation.
2. Understand the purpose of the incoming implementation.
3. Identify overlapping and independent behavior.
4. Preserve current working behavior.
5. Add only the necessary incoming behavior.
6. Resolve duplicate names, routes, registrations, and dependencies.
7. Verify that both old and new functionality can coexist.
8. Explain any behavior that cannot be preserved.

If both implementations cannot safely coexist:

* Stop.
* Explain the conflict.
* Present the available options.
* Describe the effect of each option.
* Wait for explicit confirmation.

Never resolve a conflict based only on which file is newer.

---

# ==================================================

# 11. FILE MODIFICATION RULES

# ==================================================

Do not rename files unless explicitly requested.

Do not move files unless explicitly requested.

Do not delete files unless explicitly requested.

Do not create duplicate implementations.

Reuse existing code whenever possible.

Do not create new:

* Services.
* Repositories.
* Helpers.
* Managers.
* Utilities.
* Interfaces.
* Abstraction layers.
* Base classes.
* Configuration classes.

unless they are required by the requested feature and cannot be implemented safely using the existing structure.

Before creating a new file, verify that equivalent functionality does not already exist.

Before deleting a file, search for all references to it.

Do not modify generated files manually unless there is no safer supported method.

Do not add temporary files, backup files, or copied versions such as:

* `Controller_New.cs`
* `Model_Old.cs`
* `View_backup.cshtml`
* `final-final.cs`
* `copy.cs`

Use version control instead of duplicate backup files.

---

# ==================================================

# 12. ASP.NET MVC-SPECIFIC RULES

# ==================================================

Follow the ASP.NET MVC structure already used by the project.

Before modifying a feature, inspect the complete flow where applicable:

1. Route.
2. Controller.
3. Action method.
4. Service or repository.
5. Model or entity.
6. ViewModel.
7. View.
8. Validation.
9. Database operation.
10. Authentication and authorization.

Do not move business logic between layers unless explicitly requested.

Do not change controller or action names without checking:

* Views.
* Links.
* Forms.
* Redirects.
* JavaScript requests.
* Route attributes.
* Authorization attributes.

Do not change model properties without checking:

* Entity Framework mappings.
* ViewModels.
* Razor views.
* Model binding.
* Validation attributes.
* Database schema.
* JSON serialization.
* Existing migrations.

Do not remove model validation to make a form submit successfully.

Do not disable anti-forgery validation merely to avoid an error.

Do not weaken authentication or authorization rules without explicit permission.

Do not expose administrative actions to unauthenticated users.

Do not place secrets directly inside source code.

---

# ==================================================

# 13. API CONTRACT RULES

# ==================================================

Before creating or modifying an API endpoint, inspect:

* Route.
* HTTP method.
* Request parameters.
* Request body.
* Response body.
* Property names.
* Data types.
* Status codes.
* Authentication requirements.
* Authorization requirements.
* Frontend or external consumers.

Do not silently change an existing API contract.

For example, do not change:

```json
{
  "TenSanPham": "Sua tuoi"
}
```

to:

```json
{
  "ProductName": "Sua tuoi"
}
```

when existing code still uses `TenSanPham`.

If an API contract must change:

1. Identify every consumer.
2. Explain why the change is required.
3. Update all approved consumers.
4. Preserve backward compatibility when practical.
5. Report the change clearly.

Do not return database entities directly if the existing project uses ViewModels or DTOs.

Follow the current project pattern.

---

# ==================================================

# 14. DEPENDENCY AND PACKAGE RULES

# ==================================================

Do not add a new package unless it is required.

Do not upgrade:

* .NET version.
* ASP.NET version.
* Entity Framework version.
* NuGet packages.
* JavaScript packages.
* CSS frameworks.
* Build tools.

unless explicitly requested.

Do not remove a dependency merely because its usage is not immediately visible.

Before adding a dependency:

1. Check whether the project already contains equivalent functionality.
2. Explain why the dependency is necessary.
3. Verify compatibility with the current target framework.
4. Identify the project file that will change.
5. Describe possible effects on build and deployment.

When modifying a `.csproj` file, preserve all existing:

* Package references.
* Project references.
* Build settings.
* Target framework settings.
* Content rules.
* Copy rules.
* User secrets configuration.

Do not modify lock files or generated dependency files unnecessarily.

---

# ==================================================

# 15. CONFIGURATION AND SECRET RULES

# ==================================================

Treat configuration files as high-risk files.

High-risk configuration files include:

* `appsettings.json`
* `appsettings.Development.json`
* `launchSettings.json`
* `.env`
* `.gitignore`
* `.csproj`
* `.sln`
* `Program.cs`
* `Startup.cs`

Before modifying one of these files:

1. Read the entire file.
2. Explain why the modification is required.
3. Make the smallest possible change.
4. Preserve all existing settings.
5. Check whether other environments depend on the setting.

Never commit or expose:

* Passwords.
* Database passwords.
* API keys.
* Access tokens.
* Private keys.
* Connection-string credentials.
* Personal secrets.

Use environment variables, user secrets, or the project's existing secret-management approach.

Do not overwrite a complete connection string when only one setting requires adjustment.

---

# ==================================================

# 15A. OAUTH AND EXTERNAL LOGIN SECRETS

# ==================================================

This project may use OAuth/external login providers such as Google, Facebook, and GitHub.

OAuth provider credentials are secrets and must be protected.

Never commit real values for:

* `Authentication:Google:ClientId`
* `Authentication:Google:ClientSecret`
* `Authentication:Facebook:AppId`
* `Authentication:Facebook:AppSecret`
* `Authentication:GitHub:ClientId`
* `Authentication:GitHub:ClientSecret`

Do not place real OAuth secrets in:

* `appsettings.json`
* `appsettings.Development.json`
* `.env`
* source code
* comments
* documentation committed to the repository

Local development rule:

* Use `dotnet user-secrets` for OAuth credentials.
* `UserSecretsId` in the `.csproj` file is allowed to be committed.
* `UserSecretsId` is not a secret.
* The actual secret values must stay outside Git.

Deployment rule:

* Use environment variables or the hosting provider's secret/application settings.
* Use double underscores for ASP.NET Core environment variables.

Examples:

```text
Authentication__Google__ClientId
Authentication__Google__ClientSecret
Authentication__Facebook__AppId
Authentication__Facebook__AppSecret
Authentication__GitHub__ClientId
Authentication__GitHub__ClientSecret
```

Before committing OAuth-related changes:

1. Check `appsettings.json`.
2. Check `appsettings.Development.json`.
3. Check `.env` files if present.
4. Check staged Git diff.
5. Confirm no real ClientSecret or access token is present.

If a secret is accidentally committed or pushed:

1. Stop immediately.
2. Tell the user.
3. Rotate/revoke the secret in the provider console.
4. Remove the secret from the repository.
5. Do not continue using the exposed secret.

Never print real OAuth ClientSecret values in final responses, logs, commits, comments, or documentation.

---

# ==================================================

# 16. GIT AND BRANCH RULES

# ==================================================

Do not push directly to `main`.

Do not merge directly into `main` unless explicitly requested and the integrated version has been checked.

Do not:

* Force push.
* Rewrite shared history.
* Run `git reset --hard`.
* Delete branches.
* Delete tags.
* Amend shared commits.
* Use destructive Git commands.
* Commit secrets.
* Commit `.env` files containing secrets.

unless explicitly requested and the consequences have been explained.

Recommended branch structure:

```text
main
└── integration
    ├── feature-trang-chu
    ├── feature-dang-nhap
    └── feature-san-pham
```

Recommended integration process:

1. Ensure `main` is up to date.
2. Create or update the `integration` branch.
3. Merge or copy one feature at a time.
4. Resolve conflicts manually.
5. Build and test after each feature.
6. Commit each feature separately.
7. Merge `integration` into `main` only after verification.

Do not integrate all team members' changes at once without intermediate checks.

Do not copy project folders over one another using file-system overwrite as a substitute for Git merge.

Before suggesting a destructive Git command, provide a safer alternative.

---

# ==================================================

# 17. PRE-CHANGE RESPONSE REQUIREMENTS

# ==================================================

Before making code changes, explain:

1. What will be changed.
2. Why the change is needed.
3. Which files will be modified.
4. Which existing features may be affected.
5. How the change will preserve current functionality.
6. What checks will be performed afterward.

Use a concise format such as:

```text
Planned change:
- Purpose:
- Files to modify:
- Existing functionality at risk:
- Safety approach:
- Validation:
```

Do not start modifying code before providing this explanation.

For a trivial change limited to one obvious line, the explanation may be brief, but it must still identify the file and purpose.

---

# ==================================================

# 18. POST-CHANGE RESPONSE REQUIREMENTS

# ==================================================

After making changes, report:

## Files added

* List every new file.
* Explain the purpose of each file.

## Files modified

* List every modified file.
* Explain exactly what changed.
* Explain why the change was required.

## Files deleted

* List every deleted file.
* Explain why deletion was necessary.
* State that deletion was explicitly requested or approved.

## Existing functionality preserved

* List the relevant existing features that were intentionally preserved.

## Validation performed

Report only checks that were actually performed, such as:

* Build succeeded.
* Tests passed.
* Application started.
* Route was verified.
* Database migration was inspected.
* Static analysis completed.

## Possible side effects

* Explain known risks or behavior changes.

## Not verified

Clearly state anything that could not be verified, such as:

* Database was unavailable.
* Required environment variables were missing.
* External API was unavailable.
* The application could not be executed.
* Only static code inspection was performed.

Never say “everything works” without evidence.

---

# ==================================================

# 19. ERROR HANDLING AND UNCERTAINTY

# ==================================================

If project context is unclear:

* Do not guess.
* Do not invent missing files.
* Do not invent database structures.
* Do not invent routes.
* Do not invent package versions.
* Do not assume the project uses a specific ASP.NET MVC version.

First inspect available project files.

If critical information is still missing, explain exactly what is missing.

If multiple safe solutions exist:

1. Explain the options.
2. Describe advantages and risks.
3. Recommend the smallest and safest option.
4. Wait for confirmation when the options produce meaningfully different behavior.

If a requested change cannot be verified, state that limitation clearly.

Do not hide compiler errors or runtime errors with unrelated workarounds.

Fix the root cause within the requested scope whenever possible.

---

# ==================================================

# 20. VALIDATION AFTER CHANGES

# ==================================================

After each change, perform all applicable checks.

Check for:

* Syntax errors.
* Compilation errors.
* Missing namespaces.
* Missing references.
* Incorrect imports.
* Incorrect route names.
* Broken links.
* Duplicate routes.
* Duplicate dependency registrations.
* Missing dependency registrations.
* Model-binding errors.
* Validation errors.
* Null-reference risks.
* Incorrect database mappings.
* Migration conflicts.
* Broken Razor syntax.
* Incorrect ViewModel types.
* Missing views.
* Incorrect redirects.
* Changed API contracts.
* Authentication regressions.
* Authorization regressions.
* Lost existing functionality.

Use only commands appropriate for the current project.

Possible checks include:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run
```

For projects containing frontend dependencies, applicable checks may also include:

```bash
npm install
npm run build
npm test
```

Do not run destructive database commands automatically.

Do not apply migrations to a real or production database without explicit permission.

If commands cannot be run, perform static inspection and clearly state that runtime verification was not completed.

---

# ==================================================

# 21. HIGH-RISK CHANGES

# ==================================================

The following actions are considered high risk:

* Modifying more than 3 existing files.
* Changing database schema.
* Creating or applying migrations.
* Changing authentication.
* Changing authorization.
* Changing session handling.
* Changing global routing.
* Changing dependency injection.
* Changing application startup.
* Changing target framework.
* Updating packages.
* Replacing a shared layout.
* Replacing shared JavaScript or CSS.
* Renaming public classes or properties.
* Changing API contracts.
* Deleting files.
* Moving files.
* Replacing an entire implementation.
* Running destructive Git commands.
* Running destructive database commands.

Before a high-risk change:

1. Stop.
2. Explain why it is necessary.
3. List all affected files.
4. Explain the effect on existing features.
5. Present a rollback approach.
6. Ask for explicit confirmation.

---

# ==================================================

# 22. PROHIBITED BEHAVIOR

# ==================================================

The AI must not:

* Ignore this file.
* Modify code before reading related files.
* Replace the project with incoming team code.
* Rewrite working code without necessity.
* Make unrelated changes.
* Rename Vietnamese entities into English automatically.
* Perform mass renaming.
* Delete apparently unused code without reference analysis.
* Disable validation to hide errors.
* Disable security features to make code run.
* Add packages unnecessarily.
* Upgrade versions unnecessarily.
* Change database structures silently.
* Change API contracts silently.
* Overwrite shared configuration files.
* Create duplicate implementations.
* Claim unperformed tests were completed.
* Claim the project is fully working without verification.
* Push directly to `main`.
* Force push.
* Expose secrets.
* Apply destructive migrations automatically.
* Use destructive Git commands without approval.
* Resolve conflicts by blindly choosing one entire side.

---

# ==================================================

# 23. PRIORITY ORDER

# ==================================================

When rules appear to compete, follow this priority order:

Priority 1:
Preserve existing working functionality.

Priority 2:
Read and follow `AGENTS.md`.

Priority 3:
Protect data, credentials, authentication, and authorization.

Priority 4:
Treat the current project as the official source of truth.

Priority 5:
Keep the project's Vietnamese naming style.

Priority 6:
Stay within the exact requested scope.

Priority 7:
Make the smallest possible change.

Priority 8:
Integrate incoming code instead of replacing current code.

Priority 9:
Ask before high-risk or large modifications.

Priority 10:
Avoid unnecessary refactoring, abstraction, optimization, and dependency changes.

Priority 11:
Verify changes and report limitations honestly.

---

# ==================================================

# 24. REQUIRED WORKFLOW FOR TEAM CODE

# ==================================================

When the user provides code from a team member and asks to integrate it, follow this exact workflow:

## Step 1: Read

* Read `AGENTS.md`.
* Read the current project structure.
* Read the incoming code.
* Read all directly related current files.

## Step 2: Compare

Identify:

* New files.
* Modified files.
* Deleted files.
* Duplicate files.
* Shared files.
* Changed models.
* Changed routes.
* Changed dependencies.
* Changed database structures.
* Changed API contracts.

## Step 3: Assess risk

Explain:

* What can be integrated safely.
* What may conflict.
* What existing functionality is at risk.
* Whether more than 3 files are required.
* Whether confirmation is required.

## Step 4: Integrate minimally

* Add only the required code.
* Preserve current logic.
* Reuse current components.
* Avoid replacing complete files.
* Avoid unrelated formatting.
* Avoid architecture changes.

## Step 5: Validate

* Build the project.
* Run applicable tests.
* Check routes.
* Check references.
* Check database mappings.
* Check authentication and authorization.
* Check existing related functionality.

## Step 6: Report

List:

* Files added.
* Files modified.
* Files deliberately left unchanged.
* Incoming files not used.
* Conflicts resolved.
* Existing functionality preserved.
* Checks performed.
* Checks not performed.
* Remaining risks.

---

# ==================================================

# 25. FINAL RULE

# ==================================================

When integrating code from another team member:

* Do not treat incoming code as a full replacement.
* Treat it as a feature that must be adapted to the current project.
* Compare before modifying.
* Preserve existing functionality.
* Modify only what is necessary.
* Never overwrite complete files without analysis.
* Never make unrelated improvements.
* Never guess about missing context.
* Stop and explain before any high-risk change.
* Report all modifications accurately.
* Be honest about what was and was not tested.
