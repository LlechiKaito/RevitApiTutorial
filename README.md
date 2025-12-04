# Revit API チュートリアル - 最小構成

Revit 2026 アドインの最小構成を学ぶためのチュートリアルプロジェクトです。

## プロジェクト構成

```
RevitApiTutorial/
├── RevitApiTutorial.sln
├── src/
│   └── MinimalAddin/
│       ├── MinimalAddin.csproj    # プロジェクトファイル
│       ├── MinimalAddin.addin     # マニフェストファイル
│       ├── App.cs                 # エントリポイント
│       └── HelloWorldCommand.cs   # Hello World コマンド
└── docs/
    └── FAQ.md                     # よくある質問
```

## 必須ファイル（3つ）

| ファイル | 役割 |
|---------|------|
| `.csproj` | ビルド設定、パッケージ参照 |
| `.addin` | Revit にアドインを認識させるマニフェスト |
| `App.cs` | `IExternalApplication` 実装（エントリポイント） |

## クイックスタート

```bash
# Visual Studio でビルド
# RevitApiTutorial.sln を開いて Ctrl + Shift + B

# または dotnet CLI（Windows環境）
dotnet build src/MinimalAddin/MinimalAddin.csproj
```

ビルド後、以下が `C:\ProgramData\Autodesk\Revit\Addins\2026\` にコピーされます：
- `MinimalAddin.dll`
- `MinimalAddin.addin`

## 動作確認

1. Revit 2026 を起動
2. 「Minimal Addin」ダイアログが表示される → アドイン読み込み成功
3. アドインタブ → 外部ツール → Hello World → 「Hello World!」表示

## Revit バージョン別要件

| Revit | .NET | TargetFramework |
|-------|------|-----------------|
| 2024 以前 | .NET Framework 4.8 | net48 |
| 2025 | .NET 8.0 | net8.0-windows |
| 2026 | .NET 8.0 | net8.0-windows10.0.19041.0 |

## ブランチ

| ブランチ | 内容 |
|---------|------|
| `feature/create-wall-command` | 壁を生成する CreateWallCommand |

## 参考

- [Revit API Docs](https://www.revitapidocs.com/)
- [docs/FAQ.md](docs/FAQ.md) - よくある質問
