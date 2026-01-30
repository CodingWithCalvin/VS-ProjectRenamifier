<div align="center">

<img src="https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/logo.png" alt="Project Renamifier Logo" width="256"/>

---

[![License](https://img.shields.io/github/license/CodingWithCalvin/VS-ProjectRenamifier?style=for-the-badge)](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/blob/main/LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/CodingWithCalvin/VS-ProjectRenamifier/build.yml?style=for-the-badge)](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/actions/workflows/build.yml)

[![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)
[![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)
[![Visual Studio Marketplace](https://img.shields.io/badge/VS%20Marketplace-Install-blue?style=for-the-badge&logo=visualstudio)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)
[![Visual Studio Marketplace Rating](https://img.shields.io/visual-studio-marketplace/r/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)

🚀 **Rename projects the way it should have always worked!**

Tired of the tedious, error-prone process of renaming a project in Visual Studio? Say goodbye to manual find-and-replace across dozens of files! **Project Renamifier** handles everything automatically — one click, one dialog, done! ✨

</div>

## ✨ Features

When you rename a project, this extension handles **all** of the following automatically:

| Feature | Description |
|---------|-------------|
| 📁 **Project file rename** | Renames the `.csproj` file to match the new name |
| 📂 **Directory rename** | Renames the parent directory if it matches the old project name |
| ⚙️ **Project properties** | Updates `RootNamespace` and `AssemblyName` in the project file |
| 📝 **Namespace declarations** | Updates all `namespace` declarations in source files |
| 📦 **Using statements** | Updates `using`, `global using`, `using static`, and using aliases across the solution |
| 🔗 **Fully qualified references** | Updates references like `OldName.MyClass` to `NewName.MyClass` |
| 🔧 **Project references** | Updates `ProjectReference` paths in all projects that reference the renamed project |
| 🗂️ **Solution structure** | Preserves solution folder organization |

The extension shows a progress dialog with step-by-step status as it performs the rename operation, and includes error handling with rollback support if something goes wrong. 🛡️

## 📸 Screenshots

![Rename Dialog](https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/rename-dialog.png)

![Progress Dialog](https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/progress-dialog.png)

## 📥 Installation

### Visual Studio Marketplace (Recommended)

[![Install from VS Marketplace](https://img.shields.io/badge/Install%20from-VS%20Marketplace-purple?style=for-the-badge&logo=visualstudio)](https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier)

### Manual Installation

1. 📥 Download the `.vsix` file from the [Releases](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/releases) page
2. 🖱️ Double-click the downloaded file to install

## 🎮 Usage

1. 🖱️ Right-click on a project in Solution Explorer
2. 📋 Select **Renamify Project**
3. ✏️ Enter the new project name in the dialog
4. 🚀 Click **Rename** and watch the magic happen!

## 💻 Supported Versions

| Visual Studio | Architectures |
|---------------|---------------|
| 🟢 Visual Studio 2022 (17.x) | x64 (amd64), ARM64 |
| 🟢 Visual Studio 2026 (18.x) | x64 (amd64), ARM64 |

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🤝 Contributing

Contributions are welcome! Issues, PRs, feature requests — bring it on! 💪

For cloning and building this project yourself, make sure to install the [Extensibility Essentials 2022 extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityEssentials2022) for Visual Studio which enables some features used by this project.

## 👥 Contributors

<!-- readme: contributors -start -->
[![CalvinAllen](https://avatars.githubusercontent.com/u/41448698?v=4&s=64)](https://github.com/CalvinAllen)
<!-- readme: contributors -end -->

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/CodingWithCalvin">Coding With Calvin</a>
</p>
