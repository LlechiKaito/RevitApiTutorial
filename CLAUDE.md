# Revit API チュートリアル - Claude Code 用クイックリファレンス

## プロジェクト概要

これは **Revit API アドイン開発の最小構成を学ぶ**ためのチュートリアルプロジェクトです。

**目的**: Revit 2026 アドインの基本構造と必須ファイルを理解する

**対象**: Revit API 初学者、C# 開発者

## プロジェクト構成

```
RevitApiTutorial/
├── RevitApiTutorial.sln          # ソリューションファイル
├── src/
│   └── MinimalAddin/
│       ├── MinimalAddin.csproj   # プロジェクトファイル
│       ├── MinimalAddin.addin    # Revit マニフェストファイル
│       └── App.cs                # エントリポイント
├── docs/
│   ├── ARCHITECTURE.md           # アーキテクチャ解説
│   ├── GETTING_STARTED.md        # セットアップガイド
│   └── REVIT_API_BASICS.md       # Revit API 基礎知識
├── CLAUDE.md                     # このファイル
└── README.md                     # メインドキュメント
```

## クイックスタート

```bash
# 1. ビルド
dotnet build src/MinimalAddin/MinimalAddin.csproj

# 2. Revit 2026 を起動
# アドインが自動的に読み込まれ、ダイアログが表示される
```

## 必須ファイル（3つ）

### 1. マニフェストファイル (.addin)

Revit にアドインを認識させる設定ファイル。

**配置先**: `C:\ProgramData\Autodesk\Revit\Addins\2026\`

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>アドイン名</Name>
    <Assembly>DLLファイル名.dll</Assembly>
    <AddInId>一意のGUID</AddInId>
    <FullClassName>名前空間.クラス名</FullClassName>
    <VendorId>ベンダーID</VendorId>
  </AddIn>
</RevitAddIns>
```

### 2. プロジェクトファイル (.csproj)

ビルド設定とパッケージ参照を定義。

**重要な設定**:
- `TargetFramework`: net8.0-windows10.0.19041.0（Revit 2026）
- `PlatformTarget`: x64（必須）
- `RevitMCPSDK`: NuGet パッケージ

### 3. エントリポイント (App.cs)

`IExternalApplication` インターフェースを実装。

```csharp
public class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        // Revit 起動時の処理
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        // Revit 終了時の処理
        return Result.Succeeded;
    }
}
```

## Revit バージョン別要件

| Revit | .NET | TargetFramework |
|-------|------|-----------------|
| 2024 以前 | .NET Framework 4.8 | net48 |
| 2025 | .NET 8.0 | net8.0-windows |
| 2026 | .NET 8.0 | net8.0-windows10.0.19041.0 |

## ビルド後のファイル配置

```
C:\ProgramData\Autodesk\Revit\Addins\2026\
├── MinimalAddin.dll        # ビルド出力
├── MinimalAddin.pdb        # デバッグシンボル
└── MinimalAddin.addin      # マニフェスト
```

## デバッグ方法

1. Visual Studio でプロジェクトを開く
2. デバッグ > 開始動作 > 外部プログラム
3. `C:\Program Files\Autodesk\Revit 2026\Revit.exe` を指定
4. F5 でデバッグ開始

## よくあるエラー

| エラー | 原因 | 解決方法 |
|--------|------|----------|
| アドインが読み込まれない | .addin ファイルが見つからない | 配置先を確認 |
| DLL が見つからない | パスが間違っている | Assembly 要素を確認 |
| FullClassName エラー | 名前空間が一致しない | 完全修飾名を確認 |

## ドキュメント

- **[README.md](README.md)** - メインドキュメント
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** - アーキテクチャ解説
- **[docs/GETTING_STARTED.md](docs/GETTING_STARTED.md)** - セットアップガイド
- **[docs/REVIT_API_BASICS.md](docs/REVIT_API_BASICS.md)** - Revit API 基礎

## 参考プロジェクト

このチュートリアルは [RevitAIForGaia](../RevitAIForGaia/) を参考にしています。
より高度な実装（Clean Architecture、WebSocket通信など）はそちらを参照してください。

---

**最終更新**: 2025-12-04
**Revit バージョン**: 2026
**.NET バージョン**: 8.0
