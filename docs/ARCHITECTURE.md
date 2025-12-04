# アーキテクチャ解説

このドキュメントでは、Revit API アドインの基本的なアーキテクチャを解説します。

## 目次

1. [システム概要](#システム概要)
2. [ファイル構成](#ファイル構成)
3. [読み込みの仕組み](#読み込みの仕組み)
4. [エントリポイントの詳細](#エントリポイントの詳細)
5. [拡張パターン](#拡張パターン)

---

## システム概要

### Revit アドインの動作原理

```
┌─────────────────────────────────────────────────────────────┐
│                    Revit 2026                               │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  起動時                                                │ │
│  │   ↓                                                   │ │
│  │  C:\ProgramData\Autodesk\Revit\Addins\2026\ をスキャン │ │
│  │   ↓                                                   │ │
│  │  .addin ファイルを読み込み                            │ │
│  │   ↓                                                   │ │
│  │  指定された DLL をロード                              │ │
│  │   ↓                                                   │ │
│  │  IExternalApplication.OnStartup() を呼び出し          │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  終了時                                                │ │
│  │   ↓                                                   │ │
│  │  IExternalApplication.OnShutdown() を呼び出し          │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### アドインの種類

Revit には2種類のアドインがあります：

| 種類 | インターフェース | 用途 |
|------|------------------|------|
| Application | `IExternalApplication` | UIを持つ一般的なアドイン |
| DBApplication | `IExternalDBApplication` | UIなしのデータベース操作専用 |

**このチュートリアルでは `IExternalApplication` を使用します。**

---

## ファイル構成

### 最小構成（3ファイル）

```
MinimalAddin/
├── MinimalAddin.csproj   # ビルド設定
├── MinimalAddin.addin    # Revit マニフェスト
└── App.cs                # エントリポイント
```

### 各ファイルの役割

```
┌─────────────────────────────────────────────────────────────┐
│  MinimalAddin.csproj                                        │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ - ターゲットフレームワーク (.NET 8.0)                  │ │
│  │ - プラットフォーム (x64)                               │ │
│  │ - NuGet パッケージ参照                                 │ │
│  │ - ビルド後のファイルコピー設定                         │ │
│  └───────────────────────────────────────────────────────┘ │
│                          ↓ ビルド                          │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ MinimalAddin.dll                                       │ │
│  │ (C:\ProgramData\Autodesk\Revit\Addins\2026\ へコピー)  │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  MinimalAddin.addin                                         │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ - アドイン名                                           │ │
│  │ - DLL ファイル名                                       │ │
│  │ - エントリポイントクラス名                             │ │
│  │ - 一意の GUID                                          │ │
│  └───────────────────────────────────────────────────────┘ │
│                          ↓                                 │
│  Revit が起動時に読み込む                                   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  App.cs                                                     │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ public class App : IExternalApplication                │ │
│  │ {                                                      │ │
│  │     OnStartup()   → Revit 起動時に実行                 │ │
│  │     OnShutdown()  → Revit 終了時に実行                 │ │
│  │ }                                                      │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 読み込みの仕組み

### .addin マニフェストの詳細

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <!-- Revit 内での表示名 -->
    <Name>Minimal Addin</Name>

    <!-- DLL ファイル名（同じフォルダにある必要がある） -->
    <Assembly>MinimalAddin.dll</Assembly>

    <!-- 一意の識別子（GUID） -->
    <!-- Visual Studio で生成: ツール > GUID の作成 -->
    <AddInId>12345678-1234-1234-1234-123456789ABC</AddInId>

    <!-- エントリポイントクラスの完全修飾名 -->
    <!-- 名前空間.クラス名 の形式 -->
    <FullClassName>MinimalAddin.App</FullClassName>

    <!-- ベンダー情報 -->
    <VendorId>Tutorial</VendorId>
    <VendorDescription>Revit API Tutorial Project</VendorDescription>
  </AddIn>
</RevitAddIns>
```

### GUID の生成方法

**Visual Studio:**
1. ツール > GUID の作成
2. 形式を選択（レジストリ形式推奨）
3. 新しい GUID をコピー

**PowerShell:**
```powershell
[guid]::NewGuid()
```

**オンライン:**
- https://www.guidgenerator.com/

---

## エントリポイントの詳細

### IExternalApplication インターフェース

```csharp
public interface IExternalApplication
{
    /// <summary>
    /// Revit 起動時に呼ばれる
    /// </summary>
    /// <param name="application">Revit アプリケーションへのアクセス</param>
    /// <returns>成功: Result.Succeeded, 失敗: Result.Failed</returns>
    Result OnStartup(UIControlledApplication application);

    /// <summary>
    /// Revit 終了時に呼ばれる
    /// </summary>
    /// <param name="application">Revit アプリケーションへのアクセス</param>
    /// <returns>成功: Result.Succeeded, 失敗: Result.Failed</returns>
    Result OnShutdown(UIControlledApplication application);
}
```

### UIControlledApplication で出来ること

```csharp
public Result OnStartup(UIControlledApplication application)
{
    // 1. リボンタブの作成
    application.CreateRibbonTab("My Tab");

    // 2. リボンパネルの作成
    RibbonPanel panel = application.CreateRibbonPanel("My Tab", "My Panel");

    // 3. ボタンの追加
    PushButton button = panel.AddItem(
        new PushButtonData("MyButton", "Click Me", assemblyPath, "MyNamespace.MyCommand")
    ) as PushButton;

    // 4. イベントの登録
    application.ControlledApplication.DocumentOpened += OnDocumentOpened;

    return Result.Succeeded;
}
```

### Result 列挙型

| 値 | 意味 | 使用場面 |
|----|------|----------|
| `Result.Succeeded` | 正常完了 | 処理が成功した場合 |
| `Result.Failed` | 失敗 | エラーが発生した場合 |
| `Result.Cancelled` | キャンセル | ユーザーがキャンセルした場合 |

---

## 拡張パターン

### パターン1: 外部コマンドの追加

ボタンクリック時に実行されるコマンドを追加：

```csharp
// MyCommand.cs
public class MyCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        Document doc = uidoc.Document;

        // ここに処理を記述
        TaskDialog.Show("Info", "Command executed!");

        return Result.Succeeded;
    }
}
```

**.addin への追加:**

```xml
<RevitAddIns>
  <!-- Application アドイン -->
  <AddIn Type="Application">
    ...
  </AddIn>

  <!-- Command アドイン（追加） -->
  <AddIn Type="Command">
    <Name>My Command</Name>
    <Assembly>MinimalAddin.dll</Assembly>
    <AddInId>ANOTHER-GUID-HERE</AddInId>
    <FullClassName>MinimalAddin.MyCommand</FullClassName>
    <VendorId>Tutorial</VendorId>
  </AddIn>
</RevitAddIns>
```

### パターン2: リボンUIの追加

```csharp
public Result OnStartup(UIControlledApplication application)
{
    // カスタムタブを作成
    string tabName = "My Tools";
    application.CreateRibbonTab(tabName);

    // パネルを作成
    RibbonPanel panel = application.CreateRibbonPanel(tabName, "Tools");

    // ボタンを追加
    string assemblyPath = Assembly.GetExecutingAssembly().Location;
    PushButtonData buttonData = new PushButtonData(
        "MyButton",           // 内部名
        "Click Me",           // 表示名
        assemblyPath,         // DLL パス
        "MinimalAddin.MyCommand"  // コマンドクラス
    );

    PushButton button = panel.AddItem(buttonData) as PushButton;
    button.ToolTip = "ボタンの説明";

    return Result.Succeeded;
}
```

### パターン3: イベント処理

```csharp
public Result OnStartup(UIControlledApplication application)
{
    // ドキュメントオープン時のイベント
    application.ControlledApplication.DocumentOpened += OnDocumentOpened;

    // ドキュメント保存時のイベント
    application.ControlledApplication.DocumentSaved += OnDocumentSaved;

    return Result.Succeeded;
}

private void OnDocumentOpened(object sender, DocumentOpenedEventArgs args)
{
    Document doc = args.Document;
    TaskDialog.Show("Info", $"Opened: {doc.Title}");
}

private void OnDocumentSaved(object sender, DocumentSavedEventArgs args)
{
    Document doc = args.Document;
    TaskDialog.Show("Info", $"Saved: {doc.Title}");
}

public Result OnShutdown(UIControlledApplication application)
{
    // イベントの解除（重要！）
    application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
    application.ControlledApplication.DocumentSaved -= OnDocumentSaved;

    return Result.Succeeded;
}
```

---

## 発展的なアーキテクチャ

より大規模なアドインでは、以下のようなアーキテクチャを採用できます：

### Clean Architecture

```
src/
├── Presentation/         # UI、外部通信
│   ├── App.cs           # エントリポイント
│   └── Commands/        # IExternalCommand 実装
│
├── Application/          # ユースケース
│   ├── Commands/        # ビジネスロジック
│   └── DTOs/            # データ転送オブジェクト
│
├── Domain/              # ドメインモデル
│   ├── Entities/        # エンティティ
│   └── Services/        # ドメインサービス
│
└── Infrastructure/      # 外部依存
    └── Revit/           # Revit API 実装
```

**参考**: [RevitAIForGaia](../../RevitAIForGaia/) プロジェクト

---

## 次のステップ

1. **[GETTING_STARTED.md](GETTING_STARTED.md)** - セットアップガイド
2. **[REVIT_API_BASICS.md](REVIT_API_BASICS.md)** - Revit API の基礎
3. **[RevitAIForGaia](../../RevitAIForGaia/)** - 実践的な実装例
