# Revit API 基礎知識

このドキュメントでは、Revit API の基本概念と頻出パターンを解説します。

## 目次

1. [基本概念](#基本概念)
2. [ドキュメント操作](#ドキュメント操作)
3. [要素の取得](#要素の取得)
4. [要素の作成](#要素の作成)
5. [トランザクション](#トランザクション)
6. [パラメータ操作](#パラメータ操作)
7. [単位変換](#単位変換)
8. [スレッドセーフティ](#スレッドセーフティ)

---

## 基本概念

### 主要なクラス階層

```
UIApplication
└── Application
    └── Document
        └── Element
            ├── Wall
            ├── Floor
            ├── Column
            └── ...
```

### 重要なクラス

| クラス | 説明 | 用途 |
|--------|------|------|
| `UIApplication` | Revit UI へのアクセス | ダイアログ表示、選択など |
| `Application` | Revit アプリケーション | ドキュメント作成、開く |
| `Document` | Revit プロジェクト | 要素の取得・作成 |
| `Element` | すべての要素の基底クラス | 壁、床、柱など |
| `ElementId` | 要素の識別子 | 要素の参照 |
| `Transaction` | 変更のまとまり | 作成・編集・削除 |

### UIControlledApplication vs UIApplication

```csharp
// IExternalApplication で受け取る
// Revit 起動時、まだドキュメントは開いていない
public Result OnStartup(UIControlledApplication application)
{
    // UIControlledApplication: リボンUI操作、イベント登録
    application.CreateRibbonTab("My Tab");
}

// IExternalCommand で受け取る
// ドキュメントが開いている状態
public Result Execute(ExternalCommandData commandData, ...)
{
    // UIApplication: ドキュメントアクセス可能
    UIApplication uiApp = commandData.Application;
    Document doc = uiApp.ActiveUIDocument.Document;
}
```

---

## ドキュメント操作

### ドキュメントの取得

```csharp
// コマンド内で
public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
{
    // UIApplication を取得
    UIApplication uiApp = commandData.Application;

    // UIDocument を取得（選択などのUI操作に使用）
    UIDocument uidoc = uiApp.ActiveUIDocument;

    // Document を取得（要素操作に使用）
    Document doc = uidoc.Document;

    return Result.Succeeded;
}
```

### ドキュメントの種類確認

```csharp
if (doc.IsFamilyDocument)
{
    // ファミリードキュメント
}
else
{
    // プロジェクトドキュメント
}
```

---

## 要素の取得

### FilteredElementCollector

要素を効率的に取得するためのクラス：

```csharp
// すべての壁を取得
var walls = new FilteredElementCollector(doc)
    .OfClass(typeof(Wall))
    .ToElements();

// すべての壁タイプを取得
var wallTypes = new FilteredElementCollector(doc)
    .OfClass(typeof(WallType))
    .ToElements();

// カテゴリでフィルタ
var doors = new FilteredElementCollector(doc)
    .OfCategory(BuiltInCategory.OST_Doors)
    .WhereElementIsNotElementType()
    .ToElements();
```

### フィルタの組み合わせ

```csharp
// 複数条件でフィルタ
var structuralWalls = new FilteredElementCollector(doc)
    .OfClass(typeof(Wall))
    .Cast<Wall>()
    .Where(w => w.StructuralUsage == StructuralWallUsage.Bearing);
```

### ID で要素を取得

```csharp
// ElementId から要素を取得
ElementId id = new ElementId(12345);
Element element = doc.GetElement(id);

// 型を指定して取得
Wall wall = doc.GetElement(id) as Wall;
if (wall != null)
{
    // 壁として処理
}
```

---

## 要素の作成

### 壁の作成

```csharp
// 始点と終点を定義
XYZ start = new XYZ(0, 0, 0);
XYZ end = new XYZ(10, 0, 0);  // 単位: feet

// 線を作成
Line line = Line.CreateBound(start, end);

// レベルを取得
Level level = new FilteredElementCollector(doc)
    .OfClass(typeof(Level))
    .FirstElement() as Level;

// 壁タイプを取得
WallType wallType = new FilteredElementCollector(doc)
    .OfClass(typeof(WallType))
    .FirstElement() as WallType;

// 壁を作成（トランザクション内で）
Wall wall = Wall.Create(
    doc,           // ドキュメント
    line,          // 壁の線
    wallType.Id,   // 壁タイプ
    level.Id,      // レベル
    10,            // 高さ (feet)
    0,             // オフセット
    false,         // 反転
    false          // 構造
);
```

### 床の作成

```csharp
// 輪郭を定義
List<Curve> curves = new List<Curve>();
curves.Add(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
curves.Add(Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 10, 0)));
curves.Add(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(0, 10, 0)));
curves.Add(Line.CreateBound(new XYZ(0, 10, 0), new XYZ(0, 0, 0)));

CurveLoop curveLoop = CurveLoop.Create(curves);
List<CurveLoop> curveLoops = new List<CurveLoop> { curveLoop };

// 床タイプを取得
FloorType floorType = new FilteredElementCollector(doc)
    .OfClass(typeof(FloorType))
    .FirstElement() as FloorType;

// 床を作成
Floor floor = Floor.Create(doc, curveLoops, floorType.Id, level.Id);
```

### ファミリーインスタンスの作成（ドア、窓など）

```csharp
// ファミリーシンボル（タイプ）を取得
FamilySymbol doorSymbol = new FilteredElementCollector(doc)
    .OfClass(typeof(FamilySymbol))
    .OfCategory(BuiltInCategory.OST_Doors)
    .FirstElement() as FamilySymbol;

// シンボルをアクティブ化（初回のみ必要）
if (!doorSymbol.IsActive)
{
    doorSymbol.Activate();
    doc.Regenerate();
}

// 配置位置
XYZ location = new XYZ(5, 0, 0);

// ドアを作成
FamilyInstance door = doc.Create.NewFamilyInstance(
    location,
    doorSymbol,
    wall,           // ホスト（壁）
    level,
    StructuralType.NonStructural
);
```

---

## トランザクション

### 基本的な使い方

Revit ドキュメントを変更する操作は、必ずトランザクション内で行う必要があります：

```csharp
using (Transaction trans = new Transaction(doc, "Create Wall"))
{
    // トランザクション開始
    trans.Start();

    try
    {
        // 要素を作成・編集・削除
        Wall wall = Wall.Create(doc, line, wallTypeId, levelId, height, 0, false, false);

        // 成功したらコミット
        trans.Commit();
    }
    catch (Exception ex)
    {
        // エラー時はロールバック
        trans.RollBack();
        throw;
    }
}
```

### トランザクショングループ

複数のトランザクションをまとめる場合：

```csharp
using (TransactionGroup transGroup = new TransactionGroup(doc, "Multiple Operations"))
{
    transGroup.Start();

    using (Transaction trans1 = new Transaction(doc, "Step 1"))
    {
        trans1.Start();
        // 操作1
        trans1.Commit();
    }

    using (Transaction trans2 = new Transaction(doc, "Step 2"))
    {
        trans2.Start();
        // 操作2
        trans2.Commit();
    }

    // すべて成功したらアシミレート
    transGroup.Assimilate();
}
```

### サブトランザクション

トランザクション内での一時的な変更：

```csharp
using (Transaction trans = new Transaction(doc, "Main Operation"))
{
    trans.Start();

    using (SubTransaction subTrans = new SubTransaction(doc))
    {
        subTrans.Start();
        // 一時的な変更
        subTrans.Commit();  // または RollBack()
    }

    trans.Commit();
}
```

---

## パラメータ操作

### パラメータの取得

```csharp
// ビルトインパラメータを取得
Parameter heightParam = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
if (heightParam != null)
{
    double height = heightParam.AsDouble();  // feet
}

// 名前でパラメータを取得
Parameter customParam = wall.LookupParameter("My Custom Parameter");
if (customParam != null)
{
    string value = customParam.AsString();
}
```

### パラメータの設定

```csharp
using (Transaction trans = new Transaction(doc, "Set Parameter"))
{
    trans.Start();

    Parameter param = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
    if (param != null && !param.IsReadOnly)
    {
        param.Set(15.0);  // feet
    }

    trans.Commit();
}
```

### パラメータの種類

```csharp
Parameter param = element.get_Parameter(BuiltInParameter.SOME_PARAM);

switch (param.StorageType)
{
    case StorageType.Double:
        double d = param.AsDouble();
        break;
    case StorageType.Integer:
        int i = param.AsInteger();
        break;
    case StorageType.String:
        string s = param.AsString();
        break;
    case StorageType.ElementId:
        ElementId id = param.AsElementId();
        break;
}
```

---

## 単位変換

### Revit の内部単位

**重要**: Revit API は内部的に **フィート（feet）** を使用します。

```csharp
// mm → feet
public static double MillimetersToFeet(double mm)
{
    return mm * 0.00328084;  // 1mm = 0.00328084 feet
}

// feet → mm
public static double FeetToMillimeters(double feet)
{
    return feet * 304.8;  // 1 feet = 304.8 mm
}
```

### UnitUtils クラス（Revit 2022+）

```csharp
// mm → 内部単位
double internalLength = UnitUtils.ConvertToInternalUnits(
    1000,  // 1000mm
    UnitTypeId.Millimeters
);

// 内部単位 → mm
double mmLength = UnitUtils.ConvertFromInternalUnits(
    internalLength,
    UnitTypeId.Millimeters
);
```

### 変換の例

```csharp
// 3000mm の壁を作成
double heightMm = 3000;
double heightFeet = heightMm * 0.00328084;

XYZ start = new XYZ(
    0 * 0.00328084,      // 0mm
    0 * 0.00328084,      // 0mm
    0                     // Z = 0
);

XYZ end = new XYZ(
    10000 * 0.00328084,  // 10000mm
    0 * 0.00328084,      // 0mm
    0                     // Z = 0
);
```

---

## スレッドセーフティ

### Revit API のスレッド制約

**重要**: Revit API は**メインUIスレッド**からのみ呼び出し可能です。

```csharp
// ❌ 間違い: バックグラウンドスレッドからの呼び出し
Task.Run(() =>
{
    // これはエラーになる
    Wall.Create(doc, line, typeId, levelId, height, 0, false, false);
});

// ✅ 正しい: External Event を使用
public class MyExternalEventHandler : IExternalEventHandler
{
    public void Execute(UIApplication app)
    {
        // ここはメインスレッドで実行される
        Document doc = app.ActiveUIDocument.Document;
        using (Transaction trans = new Transaction(doc, "Create"))
        {
            trans.Start();
            Wall.Create(doc, line, typeId, levelId, height, 0, false, false);
            trans.Commit();
        }
    }

    public string GetName() => "My Event Handler";
}
```

### External Event の使用

```csharp
// 1. ハンドラーを定義
public class MyHandler : IExternalEventHandler
{
    public Action<UIApplication> Task { get; set; }

    public void Execute(UIApplication app)
    {
        Task?.Invoke(app);
    }

    public string GetName() => "My Handler";
}

// 2. イベントを作成
MyHandler handler = new MyHandler();
ExternalEvent externalEvent = ExternalEvent.Create(handler);

// 3. 別スレッドからイベントを発生
handler.Task = (app) =>
{
    // メインスレッドで実行される
    Document doc = app.ActiveUIDocument.Document;
    // 処理
};
externalEvent.Raise();
```

### Idling イベント

定期的な処理に使用：

```csharp
public Result OnStartup(UIControlledApplication application)
{
    application.Idling += OnIdling;
    return Result.Succeeded;
}

private void OnIdling(object sender, IdlingEventArgs e)
{
    UIApplication uiApp = sender as UIApplication;
    // ここでRevit APIを安全に呼び出せる
}

public Result OnShutdown(UIControlledApplication application)
{
    application.Idling -= OnIdling;
    return Result.Succeeded;
}
```

---

## よく使うパターン

### 要素の削除

```csharp
using (Transaction trans = new Transaction(doc, "Delete Element"))
{
    trans.Start();

    // 単一要素の削除
    doc.Delete(elementId);

    // 複数要素の削除
    doc.Delete(new List<ElementId> { id1, id2, id3 });

    trans.Commit();
}
```

### 要素のコピー

```csharp
using (Transaction trans = new Transaction(doc, "Copy Element"))
{
    trans.Start();

    // 移動ベクトル
    XYZ translation = new XYZ(10, 0, 0);  // X方向に10feet

    // コピー
    ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElement(
        doc,
        elementId,
        translation
    );

    trans.Commit();
}
```

### 要素の移動

```csharp
using (Transaction trans = new Transaction(doc, "Move Element"))
{
    trans.Start();

    XYZ translation = new XYZ(5, 0, 0);
    ElementTransformUtils.MoveElement(doc, elementId, translation);

    trans.Commit();
}
```

---

## 次のステップ

1. **壁の作成コマンドを実装** - `IExternalCommand` を使用
2. **リボンUIの追加** - カスタムボタンの作成
3. **パラメータ操作** - 要素のプロパティを変更
4. **[RevitAIForGaia](../../RevitAIForGaia/)** - より高度な実装例

## 参考リソース

- [Revit API Docs](https://www.revitapidocs.com/) - API リファレンス
- [The Building Coder](https://thebuildingcoder.typepad.com/) - Revit API ブログ
- [Autodesk Developer Network](https://www.autodesk.com/developer-network) - 公式ドキュメント
