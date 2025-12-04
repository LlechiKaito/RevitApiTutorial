# Revit API チュートリアル - 最小構成ガイド

このドキュメントは、Revit API アドインの最小構成を理解するためのチュートリアルです。

## 目次

1. [概要](#概要)
2. [必須ファイル構成](#必須ファイル構成)
3. [各ファイルの詳細](#各ファイルの詳細)
4. [ビルドとデプロイ](#ビルドとデプロイ)
5. [動作確認](#動作確認)

---

## 概要

Revit API アドインを作成するために最低限必要な要素は以下の3つです：

| ファイル | 役割 |
|---------|------|
| `.addin` マニフェストファイル | Revitにアドインを認識させる設定ファイル |
| `.csproj` プロジェクトファイル | ビルド設定とパッケージ参照 |
| `App.cs` エントリポイント | アドインの起動・終了処理 |

---

## 必須ファイル構成

```
RevitApiTutorial/
├── RevitApiTutorial.sln          # ソリューションファイル
├── src/
│   └── MinimalAddin/
│       ├── MinimalAddin.csproj   # プロジェクトファイル
│       ├── MinimalAddin.addin    # マニフェストファイル
│       └── App.cs                # エントリポイント
└── README.md                     # このファイル
```

---

## 各ファイルの詳細

### 1. マニフェストファイル (`.addin`)

Revitがアドインを認識するための設定ファイルです。

**ファイル名:** `MinimalAddin.addin`

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

**各要素の説明:**

| 要素 | 必須 | 説明 |
|-----|------|-----|
| `Type` | ○ | `Application` または `DBApplication` |
| `Name` | ○ | アドイン名（Revit内での表示名） |
| `Assembly` | ○ | 出力DLLファイル名 |
| `AddInId` | ○ | 一意のGUID（重複不可） |
| `FullClassName` | ○ | 名前空間を含む完全なクラス名 |
| `VendorId` | ○ | ベンダー識別子 |
| `VendorDescription` | - | ベンダーの説明 |

**AddIn Type の違い:**

| Type | インターフェース | 用途 |
|------|----------------|------|
| `Application` | `IExternalApplication` | UIを持つ一般的なアドイン |
| `DBApplication` | `IExternalDBApplication` | UIなしのDB操作専用アドイン |

---

### 2. プロジェクトファイル (`.csproj`)

**ファイル名:** `MinimalAddin.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Revit 2026 は .NET 8.0 を使用 -->
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>

    <!-- WPF UIコンポーネントを使用する場合 -->
    <UseWPF>true</UseWPF>

    <!-- Revitは64ビット必須 -->
    <PlatformTarget>x64</PlatformTarget>

    <!-- C# 最新機能を使用 -->
    <LangVersion>latest</LangVersion>

    <!-- Null参照チェック有効化（推奨） -->
    <Nullable>enable</Nullable>

    <!-- 暗黙的using無効化（明示的なusing推奨） -->
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>

  <!-- Revit API パッケージ参照 -->
  <ItemGroup>
    <PackageReference Include="RevitMCPSDK" Version="2026.0.0.5" />
  </ItemGroup>

  <!-- ビルド後にRevitアドインフォルダへ自動コピー -->
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

**重要な設定:**

| 設定 | 値 | 理由 |
|-----|------|------|
| `TargetFramework` | `net8.0-windows10.0.19041.0` | Revit 2026 の要件 |
| `PlatformTarget` | `x64` | Revit は 64ビットのみ対応 |
| `UseWPF` | `true` | UI コンポーネント使用時に必要 |

**Revit バージョン別の .NET 要件:**

| Revit バージョン | .NET Framework/Core |
|-----------------|---------------------|
| Revit 2024 以前 | .NET Framework 4.8 |
| Revit 2025 | .NET 8.0 |
| Revit 2026 | .NET 8.0 |

---

### 3. エントリポイント (`App.cs`)

**ファイル名:** `App.cs`

```csharp
using Autodesk.Revit.UI;

namespace MinimalAddin;

/// <summary>
/// Revit アドインのエントリポイント
/// IExternalApplication インターフェースを実装する
/// </summary>
public class App : IExternalApplication
{
    /// <summary>
    /// Revit起動時に呼ばれる
    /// アドインの初期化処理を行う
    /// </summary>
    /// <param name="application">UIControlledApplication インスタンス</param>
    /// <returns>Result.Succeeded: 成功, Result.Failed: 失敗</returns>
    public Result OnStartup(UIControlledApplication application)
    {
        // 起動確認のダイアログ表示
        TaskDialog.Show("Minimal Addin", "アドインが正常に読み込まれました！");

        return Result.Succeeded;
    }

    /// <summary>
    /// Revit終了時に呼ばれる
    /// リソースの解放などのクリーンアップ処理を行う
    /// </summary>
    /// <param name="application">UIControlledApplication インスタンス</param>
    /// <returns>Result.Succeeded: 成功, Result.Failed: 失敗</returns>
    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}
```

**IExternalApplication インターフェースのメソッド:**

| メソッド | タイミング | 用途 |
|---------|----------|------|
| `OnStartup` | Revit起動時 | 初期化、リボンUI作成、イベント登録 |
| `OnShutdown` | Revit終了時 | リソース解放、クリーンアップ |

**戻り値 (Result enum):**

| 値 | 意味 |
|----|------|
| `Result.Succeeded` | 正常終了 |
| `Result.Failed` | 失敗（エラーメッセージ表示） |
| `Result.Cancelled` | キャンセル |

---

### 4. ソリューションファイル (`.sln`)

**ファイル名:** `RevitApiTutorial.sln`

```
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MinimalAddin", "src\MinimalAddin\MinimalAddin.csproj", "{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}"
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
    GlobalSection(ProjectConfigurationPlatforms) = postSolution
        {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.Debug|Any CPU.Build.0 = Debug|Any CPU
        {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.Release|Any CPU.ActiveCfg = Release|Any CPU
        {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}.Release|Any CPU.Build.0 = Release|Any CPU
    EndGlobalSection
EndGlobal
```

---

## ビルドとデプロイ

### コマンドラインでのビルド

```bash
# プロジェクトディレクトリで実行
dotnet build -c Release

# または Debug ビルド
dotnet build -c Debug
```

### デプロイ先

ビルド後、以下のファイルが自動的にRevitアドインフォルダにコピーされます：

```
C:\ProgramData\Autodesk\Revit\Addins\2026\
├── MinimalAddin.dll
├── MinimalAddin.pdb
└── MinimalAddin.addin
```

### 手動デプロイ（自動コピーが設定されていない場合）

1. ビルド出力フォルダから `.dll` と `.pdb` をコピー
2. プロジェクトフォルダから `.addin` をコピー
3. 上記ファイルを `C:\ProgramData\Autodesk\Revit\Addins\2026\` に配置

---

## 動作確認

1. プロジェクトをビルド
2. Revit 2026 を起動
3. 「Minimal Addin」ダイアログが表示されれば成功

### デバッグ方法

**Visual Studio でのデバッグ:**

1. プロジェクトのプロパティを開く
2. デバッグ > 開始動作 > 外部プログラムの開始
3. `C:\Program Files\Autodesk\Revit 2026\Revit.exe` を指定
4. F5 でデバッグ開始

**csproj に追加する設定（デバッグ用）:**

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <StartAction>Program</StartAction>
  <StartProgram>C:\Program Files\Autodesk\Revit 2026\Revit.exe</StartProgram>
</PropertyGroup>
```

---

## 次のステップ

この最小構成を理解したら、以下の機能を追加して学習を進められます：

1. **リボンUIの作成** - カスタムタブ・ボタンの追加
2. **外部コマンド (IExternalCommand)** - ボタンクリック時の処理
3. **Revit要素の取得** - FilteredElementCollector の使用
4. **要素の作成・編集** - トランザクションの使用
5. **イベント処理** - DocumentChanged, Idling イベント

---

## 参考資料

- [Revit API Docs](https://www.revitapidocs.com/)
- [Autodesk Developer Network](https://www.autodesk.com/developer-network/platform-technologies/revit)
- RevitAIForGaia プロジェクト（本チュートリアルの参照元）
