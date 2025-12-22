# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Critical Rules

**These rules override all other instructions:**

1. **NEVER commit directly to main** - Always create a feature branch and submit a pull request
2. **Conventional commits** - Format: `type(scope): description`
3. **GitHub Issues for TODOs** - Use `gh` CLI to manage issues, no local TODO files. Use conventional commit format for issue titles
4. **Pull Request titles** - Use conventional commit format (same as commits)
5. **Branch naming** - Use format: `type/scope/short-description` (e.g., `feat/ui/settings-dialog`)
6. **Working an issue** - Always create a new branch from an updated main branch
7. **Check branch status before pushing** - Verify the remote tracking branch still exists. If a PR was merged/deleted, create a new branch from main instead
8. **WPF for all UI** - All UI must be implemented using WPF (XAML/C#). No web-based technologies (HTML, JavaScript, WebView)

---

### GitHub CLI Commands

```bash
gh issue list                    # List open issues
gh issue view <number>           # View details
gh issue create --title "type(scope): description" --body "..."
gh issue close <number>
```

### Conventional Commit Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `test` | Adding or updating tests |
| `chore` | Maintenance tasks |

---

## Project Overview

This is a Visual Studio 2022 extension (VSIX) called "Project Renamifier" that allows users to rename projects completely from within Visual Studio, including the filename, parent folder, namespace, and references in the solution file and other projects.

**Status**: Pre-alpha / WIP - core functionality is stubbed out but not fully implemented.

## Build Commands

```bash
# Build the solution (from repo root)
msbuild src/CodingWithCalvin.ProjectRenamifier/CodingWithCalvin.ProjectRenamifier.slnx

# Or open in Visual Studio and build (F5 to debug launches experimental VS instance)
```

## Development Setup

- Requires Visual Studio 2022 with the VS SDK workload
- Install [Extensibility Essentials 2022](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityEssentials2022) extension
- Debug launches VS experimental instance (`/rootsuffix Exp`)

## Architecture

**Extension Entry Point**: `ProjectRenamifierPackage.cs` - AsyncPackage that initializes the command on VS startup.

**Command Handler**: `Commands/RenamifyProjectCommand.cs` - Handles the "Renamify Project" context menu command. Currently a skeleton awaiting implementation. See open GitHub issues for feature requirements.

**Command Definition**: `VSCommandTable.vsct` - Defines the command and places it in the Project context menu (`IDG_VS_CTXT_PROJECT_EXPLORE`).

## Key VS SDK Patterns Used

- Uses DTE/DTE2 automation model for solution/project manipulation
- Uses VSLangProj for project reference management
- Commands registered via VSCT files and OleMenuCommandService
- **Always use WPF for user interface** (dialogs, windows, etc.)

## Coding Standards

Follow Microsoft's official guidelines:

- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)

Key points:
- Use PascalCase for public members, types, namespaces, and methods
- Use camelCase for private fields (prefix with `_` e.g., `_fieldName`)
- Use `var` when the type is obvious from the right side of the assignment
- Use meaningful, descriptive names
- Prefer `async`/`await` for asynchronous operations
- Use `ThreadHelper.ThrowIfNotOnUIThread()` when accessing VS services that require the UI thread
