using System;
using System.Linq.Expressions;
using Grasshopper.Kernel;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class ElementTypeSimilar : Component
  {
    public override Guid ComponentGuid => new Guid("BA9C72C5-EC88-450B-B736-BE6D827FA2F3");
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    protected override string IconTag => "S";

    public ElementTypeSimilar() : base
    (
      name: "Similar Types",
      nickname: "SimTypes",
      description: "Obtains a set of types that are similar to Type",
      category: "Revit",
      subCategory: "Type"
    )
    { }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(new Parameters.ElementType(), "Type", "T", "ElementType to query for its similar types", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.ElementType(), "Types", "T", string.Empty, GH_ParamAccess.list);
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      var elementType = default(DB.ElementType);
      if (!DA.GetData("Type", ref elementType))
        return;

      DA.SetDataList("Types", elementType?.GetSimilarTypes());
    }
  }
}
