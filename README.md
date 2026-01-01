# Project Renamifier

[![License](https://img.shields.io/github/license/CodingWithCalvin/VS-ProjectRenamifier?style=for-the-badge)](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/blob/main/LICENSE)
[![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)
[![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)
[![Visual Studio Marketplace Rating](https://img.shields.io/visual-studio-marketplace/r/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)

A Visual Studio extension that allows you to safely and completely rename a project from within Visual Studio.

## Features

When you rename a project, this extension handles all of the following automatically:

- **Project file rename** - Renames the `.csproj` file to match the new name
- **Directory rename** - Renames the parent directory if it matches the old project name
- **Project properties** - Updates `RootNamespace` and `AssemblyName` in the project file
- **Namespace declarations** - Updates all `namespace` declarations in source files
- **Using statements** - Updates `using`, `global using`, `using static`, and using aliases across the solution
- **Fully qualified references** - Updates references like `OldName.MyClass` to `NewName.MyClass`
- **Project references** - Updates `ProjectReference` paths in all projects that reference the renamed project
- **Solution structure** - Preserves solution folder organization

The extension shows a progress dialog with step-by-step status as it performs the rename operation, and includes error handling with rollback support if something goes wrong.

## Installation

### Visual Studio Marketplace

Install directly from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier).

### Manual Installation

1. Download the `.vsix` file from the [Releases](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/releases) page
2. Double-click the downloaded file to install

## Usage

1. Right-click on a project in Solution Explorer
2. Select **Rename Project (Renamify)**
3. Enter the new project name in the dialog
4. Click **Rename** and watch the progress as each step completes

## Supported Versions

- Visual Studio 2022 (17.x)
- Visual Studio 2026 (18.x)
- Architectures: x64 (amd64), ARM64

## Contributing

Contributions are welcome! Issues, PRs, etc.

For cloning and building this project yourself, make sure to install the [Extensibility Essentials 2022 extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityEssentials2022) for Visual Studio which enables some features used by this project.

## Contributors

<!-- readme: contributors -start -->
<!-- readme: contributors -end -->
