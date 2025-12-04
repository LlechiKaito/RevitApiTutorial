# よくある質問（FAQ）

## Q: .addin ファイルは必要？

**必須です。**

Revit は起動時に `C:\ProgramData\Autodesk\Revit\Addins\2026\` をスキャンして `.addin` ファイルを探します。このファイルがないと DLL があっても Revit はアドインを認識できません。

```
Revit 起動
  ↓
.addin ファイルを探す
  ↓
どの DLL を読み込むか確認
どのクラスがエントリポイントか確認
  ↓
アドインをロード
```

## Q: .pdb ファイルは必要？

**本番環境では不要です。**

`.pdb` はデバッグ用のシンボルファイルです。Visual Studio でブレークポイントを使ったデバッグをする場合のみ必要です。

csproj でコピー対象から除外できます：

```xml
<ItemGroup>
  <FilesToCopy Include="$(TargetDir)*.dll" />
  <!-- *.pdb は含めない -->
</ItemGroup>
```

## Q: .vs フォルダを生成しないようにできる？

**できません。**

`.vs/` は Visual Studio が自動生成するフォルダで、生成自体を止めることはできません。

`.gitignore` に追加することで Git の追跡対象から除外できます：

```
.vs/
```

既に追跡されている場合は以下で除外：

```bash
git rm -r --cached .vs/
```

## Q: Revit にコピーされるファイルは？

ビルド後、以下が `C:\ProgramData\Autodesk\Revit\Addins\2026\` にコピーされます：

| ファイル | 説明 |
|---------|------|
| `MinimalAddin.dll` | アドイン本体 |
| `MinimalAddin.addin` | マニフェスト |

## Q: WSL でビルドできない

WSL 環境では WPF の依存関係がないため、Windows 向けビルドができません。

以下のエラーが出る場合：
```
error MSB4019: Microsoft.NET.Sdk.WindowsDesktop.targets was not found
```

**解決策**: Windows 側の Visual Studio でビルドしてください。

## Q: アドインが読み込まれない

以下を確認：

1. `.addin` ファイルが正しい場所にあるか
   ```
   C:\ProgramData\Autodesk\Revit\Addins\2026\MinimalAddin.addin
   ```

2. `.dll` ファイルが正しい場所にあるか
   ```
   C:\ProgramData\Autodesk\Revit\Addins\2026\MinimalAddin.dll
   ```

3. `FullClassName` が正しいか（名前空間.クラス名）
   ```xml
   <FullClassName>MinimalAddin.App</FullClassName>
   ```

4. `AddInId` が他のアドインと重複していないか
