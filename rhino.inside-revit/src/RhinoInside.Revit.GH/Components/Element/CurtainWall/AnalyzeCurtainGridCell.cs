using System;
using Grasshopper.Kernel;
using RhinoInside.Revit.Convert.Geometry;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class AnalyzeCurtainGridCell : AnalysisComponent
  {
    public override Guid ComponentGuid => new Guid("FC7D5729-7D27-453A-A4A2-0E150C749083");
    public override GH_Exposure Exposure => GH_Exposure.septenary;
    protected override string IconTag => "ACC";

    public AnalyzeCurtainGridCell() : base(
      name: "Analyze Curtain Grid Cell",
      nickname: "A-CC",
      description: "Analyze given curtain grid cell",
      category: "Revit",
      subCategory: "Wall"
    )
    {
    }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(
        param: new Parameters.DataObject<DB.CurtainCell>(),
        name: "Curtain Grid Cell",
        nickname: "CGC",
        description: "Curtain Grid Cell",
        access: GH_ParamAccess.item
        );
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddCurveParameter(
        name: "Curves",
        nickname: "C",
        description: "Boundary curves of the given grid cell",
        access: GH_ParamAccess.item
        );
      manager.AddCurveParameter(
        name: "Planarized Curves",
        nickname: "PC",
        description: "Boundary curves of the flat surface fitted inside a curved grid cell",
        access: GH_ParamAccess.item
        );
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      // get input
      Types.DataObject<DB.CurtainCell> dataObj = default;
      if (!DA.GetData("Curtain Grid Cell", ref dataObj))
        return;

      DB.CurtainCell cell = dataObj.Value;

      // Revit API throws errors with no message when .CurveLoops is accessed
      // Autodesk.Revit.Exceptions.InvalidOperationException at Autodesk.Revit.DB.CurtainCell.get_CurveLoops()
      // but .CurveLoops actually returns data
      // same might happen with .PlanarizedCurveLoops but not fully tested
      try
      {
        DA.SetDataList("Curves", cell.CurveLoops?.ToPolyCurves());
        DA.SetDataList("Planarized Curves", cell.PlanarizedCurveLoops?.ToPolyCurves());
      }
      // silence the empty exception
      catch { }
    }
  }
}
