using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MinimalAddin;

/// <summary>
/// Hello World を表示する外部コマンド
/// IExternalCommand インターフェースを実装する
/// </summary>
[Transaction(TransactionMode.Manual)]
public class HelloWorldCommand : IExternalCommand
{
    /// <summary>
    /// コマンド実行時に呼ばれる
    /// </summary>
    /// <param name="commandData">Revit アプリケーションへのアクセス</param>
    /// <param name="message">エラーメッセージ（失敗時に設定）</param>
    /// <param name="elements">エラー要素（失敗時に設定）</param>
    /// <returns>Result.Succeeded: 成功, Result.Failed: 失敗</returns>
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        TaskDialog.Show("Hello World", "Hello World!");

        return Result.Succeeded;
    }
}
