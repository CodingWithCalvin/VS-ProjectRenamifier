<p align="center">
  <img src="https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/logo.png" alt="Project Renamifier Logo" width="128" height="128">
</p>

<h1 align="center">Project Renamifier</h1>

<p align="center">
  <strong>Rename projects the way it should have always worked!</strong>
</p>

<p align="center">
  <a href="https://github.com/CodingWithCalvin/VS-ProjectRenamifier/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/CodingWithCalvin/VS-ProjectRenamifier?style=for-the-badge" alt="License">
  </a>
  <a href="https://github.com/CodingWithCalvin/VS-ProjectRenamifier/actions/workflows/build.yml">
    <img src="https://img.shields.io/github/actions/workflow/status/CodingWithCalvin/VS-ProjectRenamifier/build.yml?style=for-the-badge" alt="Build Status">
  </a>
</p>

<p align="center">
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier">
    <img src="https://img.shields.io/visual-studio-marketplace/v/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge" alt="Marketplace Version">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier">
    <img src="https://img.shields.io/visual-studio-marketplace/i/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge" alt="Marketplace Installations">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier">
    <img src="https://img.shields.io/visual-studio-marketplace/d/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge" alt="Marketplace Downloads">
  </a>
  <a href="https://marketplace.visualstudio.com/items?itemName=CodingWithCalvin.VS-ProjectRenamifier">
    <img src="https://img.shields.io/visual-studio-marketplace/r/CodingWithCalvin.VS-ProjectRenamifier?style=for-the-badge" alt="Marketplace Rating">
  </a>
</p>

---

Tired of the tedious, error-prone process of renaming a project in Visual Studio? Say goodbye to manual find-and-replace across dozens of files! **Project Renamifier** handles everything automatically - one click, one dialog, done!

## ✨ Features

When you rename a project, this extension handles **all** of the following automatically:

| Feature | Description |
|---------|-------------|
| **Project file rename** | Renames the `.csproj` file to match the new name |
| **Directory rename** | Renames the parent directory if it matches the old project name |
| **Project properties** | Updates `RootNamespace` and `AssemblyName` in the project file |
| **Namespace declarations** | Updates all `namespace` declarations in source files |
| **Using statements** | Updates `using`, `global using`, `using static`, and using aliases across the solution |
| **Fully qualified references** | Updates references like `OldName.MyClass` to `NewName.MyClass` |
| **Project references** | Updates `ProjectReference` paths in all projects that reference the renamed project |
| **Solution structure** | Preserves solution folder organization |

The extension shows a progress dialog with step-by-step status as it performs the rename operation, and includes error handling with rollback support if something goes wrong.

## 📸 Screenshots

![Rename Dialog](https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/rename-dialog.png)

![Progress Dialog](https://raw.githubusercontent.com/CodingWithCalvin/VS-ProjectRenamifier/main/resources/progress-dialog.png)

## 🛠️ Installation

### Visual Studio Marketplace

1. Open Visual Studio 2022 or 2026
2. Go to **Extensions > Manage Extensions**
3. Search for "Project Renamifier"
4. Click **Download** and restart Visual Studio

### Manual Installation

Download the latest `.vsix` from the [Releases](https://github.com/CodingWithCalvin/VS-ProjectRenamifier/releases) page and double-click to install.

## 🚀 Usage

1. Right-click on a project in Solution Explorer
2. Select **Renamify Project**
3. Enter the new project name in the dialog
4. Click **Rename** and watch the magic happen!

## 💻 Supported Versions

| Visual Studio | Architectures |
|---------------|---------------|
| Visual Studio 2022 (17.x) | x64 (amd64), ARM64 |
| Visual Studio 2026 (18.x) | x64 (amd64), ARM64 |

## 🤝 Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or pull requests - all feedback helps make this extension better.

### 🔧 Development Setup

1. Clone the repository
2. Open the solution in Visual Studio 2022 or 2026
3. Ensure you have the "Visual Studio extension development" workload installed
4. Install the [Extensibility Essentials 2022](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityEssentials2022) extension
5. Press F5 to launch the experimental instance

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Contributors

<!-- readme: contributors -start -->
<a href="https://github.com/CalvinAllen"><img src="https://avatars.githubusercontent.com/u/41448698?v=4&s=64" width="64" height="64" alt="CalvinAllen"></a> <a href="https://github.com/facebbs1235"><img src="https://avatars.githubusercontent.com/u/188009954?v=4&s=64" width="64" height="64" alt="facebbs1235"></a> 
<!-- readme: contributors -end -->

---

<p align="center">
  Made with ❤️ by <a href="https://github.com/CodingWithCalvin">Coding With Calvin</a>
</p>
