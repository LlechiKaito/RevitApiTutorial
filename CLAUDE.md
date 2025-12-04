# Revit API チュートリアル - Claude Code 用リファレンス

## プロジェクト概要

Revit 2026 アドインの最小構成を学ぶチュートリアル。

## ファイル構成

```
RevitApiTutorial/
├── RevitApiTutorial.sln
├── src/MinimalAddin/
│   ├── MinimalAddin.csproj    # ビルド設定
│   ├── MinimalAddin.addin     # マニフェスト（必須）
│   ├── App.cs                 # IExternalApplication
│   └── HelloWorldCommand.cs   # IExternalCommand
└── docs/FAQ.md
```

## ビルド

```bash
dotnet build src/MinimalAddin/MinimalAddin.csproj
```

出力先: `C:\ProgramData\Autodesk\Revit\Addins\2026\`

## 重要な設定

### csproj

```xml
<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
<PlatformTarget>x64</PlatformTarget>
<UseWPF>true</UseWPF>
```

### NuGet パッケージ

```xml
<PackageReference Include="RevitMCPSDK" Version="2026.0.0.5" />
```

## アドインの種類

| Type | インターフェース | 用途 |
|------|------------------|------|
| Application | `IExternalApplication` | 起動時に実行 |
| Command | `IExternalCommand` | ボタンクリックで実行 |

## 参考

- [RevitAIForGaia](../RevitAIForGaia/) - より高度な実装例
