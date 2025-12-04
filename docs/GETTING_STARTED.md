# セットアップガイド

このドキュメントでは、Revit API アドイン開発の環境構築からビルド・実行までを解説します。

## 目次

1. [前提条件](#前提条件)
2. [開発環境の構築](#開発環境の構築)
3. [プロジェクトの作成](#プロジェクトの作成)
4. [ビルドと配置](#ビルドと配置)
5. [動作確認](#動作確認)
6. [デバッグ方法](#デバッグ方法)
7. [トラブルシューティング](#トラブルシューティング)

---

## 前提条件

### 必要なソフトウェア

| ソフトウェア | バージョン | 用途 |
|-------------|-----------|------|
| Autodesk Revit | 2026 | アドイン実行環境 |
| Visual Studio | 2022 | IDE（推奨） |
| .NET SDK | 8.0 | ビルドに必要 |

### インストール確認

```powershell
# .NET SDK のバージョン確認
dotnet --version
# 出力例: 8.0.xxx

# Revit のインストール確認
Test-Path "C:\Program Files\Autodesk\Revit 2026\Revit.exe"
# 出力: True
```

---

## 開発環境の構築

### 1. Visual Studio 2022 のインストール

1. [Visual Studio 2022](https://visualstudio.microsoft.com/) をダウンロード
2. インストーラーで以下のワークロードを選択：
   - **.NET デスクトップ開発**
   - **デスクトップ開発 (C++)**（オプション）

### 2. .NET 8.0 SDK のインストール

Visual Studio 2022 に含まれていますが、個別にインストールする場合：

1. [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) をダウンロード
2. インストーラーを実行

### 3. VS Code（代替）

Visual Studio の代わりに VS Code を使用する場合：

```bash
# C# 拡張機能をインストール
code --install-extension ms-dotnettools.csharp

# C# Dev Kit（推奨）
code --install-extension ms-dotnettools.csdevkit
```

---

## プロジェクトの作成

### 方法1: コマンドラインで作成

```bash
# 1. プロジェクトディレクトリを作成
mkdir -p RevitApiTutorial/src/MinimalAddin
cd RevitApiTutorial

# 2. ソリューションファイルを作成
dotnet new sln -n RevitApiTutorial

# 3. クラスライブラリプロジェクトを作成
cd src/MinimalAddin
dotnet new classlib -n MinimalAddin -f net8.0-windows

# 4. ソリューションにプロジェクトを追加
cd ../..
dotnet sln add src/MinimalAddin/MinimalAddin.csproj
```

### 方法2: Visual Studio で作成

1. Visual Studio を起動
2. 「新しいプロジェクトの作成」をクリック
3. 「クラス ライブラリ」を選択
4. プロジェクト名: `MinimalAddin`
5. フレームワーク: `.NET 8.0`

### プロジェクトファイルの編集

`MinimalAddin.csproj` を以下のように編集：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Revit 2026 用の設定 -->
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>

    <!-- WPF を使用する場合 -->
    <UseWPF>true</UseWPF>

    <!-- 64ビット必須 -->
    <PlatformTarget>x64</PlatformTarget>

    <!-- C# 最新機能を使用 -->
    <LangVersion>latest</LangVersion>

    <!-- Null 参照チェック有効化 -->
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Revit API パッケージ -->
  <ItemGroup>
    <PackageReference Include="RevitMCPSDK" Version="2026.0.0.5" />
  </ItemGroup>

  <!-- ビルド後に自動配置 -->
  <Target Name="CopyAddIn" AfterTargets="Build">
    <PropertyGroup>
      <AddInDir>C:\ProgramData\Autodesk\Revit\Addins\2026\</AddInDir>
    </PropertyGroup>
    <ItemGroup>
      <FilesToCopy Include="$(TargetDir)*.dll" />
      <FilesToCopy Include="$(TargetDir)*.pdb" />
    </ItemGroup>
    <MakeDir Directories="$(AddInDir)" Condition="!Exists('$(AddInDir)')" />
    <Copy SourceFiles="@(FilesToCopy)" DestinationFolder="$(AddInDir)" SkipUnchangedFiles="false" />
    <Copy SourceFiles="$(ProjectDir)MinimalAddin.addin"
          DestinationFiles="$(AddInDir)MinimalAddin.addin"
          SkipUnchangedFiles="false" />
  </Target>
</Project>
```

### マニフェストファイルの作成

`MinimalAddin.addin` を作成：

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Minimal Addin</Name>
    <Assembly>MinimalAddin.dll</Assembly>
    <AddInId>12345678-1234-1234-1234-123456789ABC</AddInId>
    <FullClassName>MinimalAddin.App</FullClassName>
    <VendorId>Tutorial</VendorId>
    <VendorDescription>Revit API Tutorial - Minimal Configuration</VendorDescription>
  </AddIn>
</RevitAddIns>
```

**注意**: `AddInId` は一意の GUID に変更してください。

### エントリポイントの作成

`App.cs` を作成（既存の `Class1.cs` を削除）：

```csharp
using Autodesk.Revit.UI;

namespace MinimalAddin;

/// <summary>
/// Revit アドインのエントリポイント
/// </summary>
public class App : IExternalApplication
{
    /// <summary>
    /// Revit 起動時に呼ばれる
    /// </summary>
    public Result OnStartup(UIControlledApplication application)
    {
        TaskDialog.Show("Minimal Addin", "アドインが正常に読み込まれました！");
        return Result.Succeeded;
    }

    /// <summary>
    /// Revit 終了時に呼ばれる
    /// </summary>
    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}
```

---

## ビルドと配置

### コマンドラインでビルド

```bash
# Debug ビルド
dotnet build src/MinimalAddin/MinimalAddin.csproj -c Debug

# Release ビルド
dotnet build src/MinimalAddin/MinimalAddin.csproj -c Release
```

### Visual Studio でビルド

1. ソリューションを開く
2. `Ctrl + Shift + B` または「ビルド」>「ソリューションのビルド」

### 配置先の確認

ビルド後、以下のファイルが自動配置されます：

```
C:\ProgramData\Autodesk\Revit\Addins\2026\
├── MinimalAddin.dll
├── MinimalAddin.pdb
├── MinimalAddin.addin
└── (その他の依存 DLL)
```

### 手動配置（自動配置が失敗した場合）

```powershell
# 配置先ディレクトリ
$addinDir = "C:\ProgramData\Autodesk\Revit\Addins\2026"

# ビルド出力からコピー
Copy-Item "src\MinimalAddin\bin\Debug\net8.0-windows10.0.19041.0\*.dll" $addinDir
Copy-Item "src\MinimalAddin\bin\Debug\net8.0-windows10.0.19041.0\*.pdb" $addinDir
Copy-Item "src\MinimalAddin\MinimalAddin.addin" $addinDir
```

---

## 動作確認

### 1. Revit を起動

1. Revit 2026 を起動
2. 「Minimal Addin」ダイアログが表示されれば成功

### 2. 読み込み確認

表示されない場合は、Revit の Add-ins タブを確認：
- 「アドインマネージャー」でアドインが認識されているか確認

---

## デバッグ方法

### Visual Studio でのデバッグ

#### 1. プロジェクト設定

プロジェクトのプロパティを開き、デバッグ設定を追加：

**csproj に追加:**

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <StartAction>Program</StartAction>
  <StartProgram>C:\Program Files\Autodesk\Revit 2026\Revit.exe</StartProgram>
</PropertyGroup>
```

#### 2. デバッグ実行

1. コードにブレークポイントを設定
2. F5 キーでデバッグ開始
3. Revit が起動し、ブレークポイントで停止

### ログ出力

デバッグ用のログ出力：

```csharp
using System.Diagnostics;

public Result OnStartup(UIControlledApplication application)
{
    // デバッグ出力（Visual Studio の出力ウィンドウに表示）
    Debug.WriteLine("OnStartup called");

    // ダイアログ表示（確実に確認したい場合）
    TaskDialog.Show("Debug", "OnStartup called");

    return Result.Succeeded;
}
```

### Revit ジャーナルファイル

エラーの詳細は Revit のジャーナルファイルで確認：

```
%LOCALAPPDATA%\Autodesk\Revit\Autodesk Revit 2026\Journals\
```

---

## トラブルシューティング

### アドインが読み込まれない

**原因1: .addin ファイルが見つからない**

```powershell
# 確認
Test-Path "C:\ProgramData\Autodesk\Revit\Addins\2026\MinimalAddin.addin"
```

**原因2: DLL が見つからない**

```powershell
# 確認
Test-Path "C:\ProgramData\Autodesk\Revit\Addins\2026\MinimalAddin.dll"
```

**原因3: FullClassName が間違っている**

- 名前空間とクラス名が一致しているか確認
- `MinimalAddin.App` の形式

**原因4: GUID が重複している**

- 他のアドインと同じ GUID を使用していないか確認

### ビルドエラー

**エラー: RevitMCPSDK が見つからない**

```bash
# NuGet パッケージを復元
dotnet restore
```

**エラー: TargetFramework が無効**

- `net8.0-windows10.0.19041.0` が正しいか確認
- .NET 8.0 SDK がインストールされているか確認

### 実行時エラー

**エラー: TypeLoadException**

- 依存 DLL が不足している可能性
- すべての依存ファイルがアドインフォルダにコピーされているか確認

**エラー: FileNotFoundException**

- Assembly タグのファイル名が正しいか確認
- DLL が正しい場所にあるか確認

---

## 次のステップ

1. **[ARCHITECTURE.md](ARCHITECTURE.md)** - アーキテクチャの理解
2. **[REVIT_API_BASICS.md](REVIT_API_BASICS.md)** - Revit API の基礎
3. **リボンUIの追加** - カスタムボタンの作成
4. **外部コマンドの追加** - `IExternalCommand` の実装
