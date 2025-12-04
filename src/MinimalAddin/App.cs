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
