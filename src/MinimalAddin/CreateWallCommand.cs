using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MinimalAddin;

/// <summary>
/// 壁を生成する外部コマンド
/// </summary>
[Transaction(TransactionMode.Manual)]
public class CreateWallCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        // ドキュメントを取得
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        Document doc = uidoc.Document;

        // レベルを取得（最初のレベルを使用）
        Level? level = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .FirstOrDefault();

        if (level == null)
        {
            message = "レベルが見つかりません";
            return Result.Failed;
        }

        // 壁タイプを取得（最初の壁タイプを使用）
        WallType? wallType = new FilteredElementCollector(doc)
            .OfClass(typeof(WallType))
            .Cast<WallType>()
            .FirstOrDefault();

        if (wallType == null)
        {
            message = "壁タイプが見つかりません";
            return Result.Failed;
        }

        // 壁の始点と終点を定義（単位: feet）
        // 10フィート = 約3048mm
        XYZ start = new XYZ(0, 0, 0);
        XYZ end = new XYZ(10, 0, 0);

        // 線を作成
        Line line = Line.CreateBound(start, end);

        // 壁の高さ（10フィート = 約3048mm）
        double height = 10;

        // トランザクション内で壁を作成
        using (Transaction trans = new Transaction(doc, "Create Wall"))
        {
            trans.Start();

            Wall wall = Wall.Create(
                doc,
                line,
                wallType.Id,
                level.Id,
                height,
                0,      // オフセット
                false,  // 反転
                false   // 構造
            );

            trans.Commit();

            TaskDialog.Show("Create Wall", $"壁を作成しました\nID: {wall.Id}");
        }

        return Result.Succeeded;
    }
}
