using System;
using Grasshopper.Kernel;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class ViewActive : DocumentComponent
  {
    public override Guid ComponentGuid => new Guid("7CCF350C-80CC-42D0-85BA-78544FD59F4A");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override string IconTag => "A";

    public ViewActive() : base
    (
      name: "Active Graphical View",
      nickname: "AGraphView",
      description: "Gets the active graphical view",
      category: "Revit",
      subCategory: "View"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      ParamDefinition.FromParam(CreateDocumentParam(), ParamVisibility.Voluntary),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      ParamDefinition.Create<Parameters.View>("Active View", "V", "Active graphical view", GH_ParamAccess.item)
    };

    protected override void TrySolveInstance(IGH_DataAccess DA, DB.Document doc)
    {
      DA.SetData("Active View", doc?.GetActiveGraphicalView());
    }
  }
}
