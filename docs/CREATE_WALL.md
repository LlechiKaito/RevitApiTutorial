# 壁の生成コマンド解説

`CreateWallCommand` は Revit API で壁を生成する基本的な実装例です。

## 実行方法

1. Revit でプロジェクトを開く（レベルが必要）
2. アドインタブ → 外部ツール → Create Wall
3. 原点から X 方向に 10 フィートの壁が生成される

## コードの流れ

```
1. ドキュメント取得
2. レベル取得（壁の配置先）
3. 壁タイプ取得
4. 始点・終点を定義
5. トランザクション内で Wall.Create()
```

## 重要なポイント

### 1. トランザクション

Revit でモデルを変更する操作は **必ずトランザクション内** で行う：

```csharp
using (Transaction trans = new Transaction(doc, "Create Wall"))
{
    trans.Start();
    // 変更処理
    trans.Commit();
}
```

### 2. 単位系

Revit API の内部単位は **フィート（feet）**：

| 値 | フィート | ミリメートル |
|----|---------|-------------|
| 1 | 1 ft | 304.8 mm |
| 10 | 10 ft | 3048 mm |

変換式：
```csharp
double feet = mm * 0.00328084;
double mm = feet * 304.8;
```

### 3. FilteredElementCollector

要素を取得するためのクラス：

```csharp
// レベルを取得
Level level = new FilteredElementCollector(doc)
    .OfClass(typeof(Level))
    .Cast<Level>()
    .FirstOrDefault();

// 壁タイプを取得
WallType wallType = new FilteredElementCollector(doc)
    .OfClass(typeof(WallType))
    .Cast<WallType>()
    .FirstOrDefault();
```

### 4. Wall.Create パラメータ

```csharp
Wall.Create(
    doc,           // Document
    line,          // 壁の線（Line）
    wallType.Id,   // 壁タイプの ElementId
    level.Id,      // レベルの ElementId
    height,        // 高さ（feet）
    offset,        // オフセット（feet）
    flip,          // 反転するか
    structural     // 構造壁か
);
```

## Transaction 属性

クラスに `[Transaction(TransactionMode.Manual)]` を付ける：

```csharp
[Transaction(TransactionMode.Manual)]
public class CreateWallCommand : IExternalCommand
```

| モード | 説明 |
|--------|------|
| `Manual` | 自分でトランザクションを管理 |
| `Automatic` | Revit が自動でトランザクションを管理（非推奨） |
| `ReadOnly` | 読み取り専用（変更不可） |

## エラーハンドリング

レベルや壁タイプが見つからない場合は `Result.Failed` を返す：

```csharp
if (level == null)
{
    message = "レベルが見つかりません";
    return Result.Failed;
}
```

`message` パラメータに設定した文字列は Revit のエラーダイアログに表示される。
