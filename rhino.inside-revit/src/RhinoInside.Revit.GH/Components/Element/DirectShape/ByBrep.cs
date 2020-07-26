using System;
using System.Linq;
using Grasshopper.Kernel;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class DirectShapeByBrep : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("5ADE9AE3-588C-4285-ABC5-09DEB92C6660");
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    public DirectShapeByBrep() : base
    (
      "Add Brep DirectShape", "BrpDShape",
      "Given a Brep, it adds a Brep shape to the active Revit document",
      "Revit", "DirectShape"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.GraphicalElement(), "Brep", "B", "New BrepShape", GH_ParamAccess.item);
    }

    void ReconstructDirectShapeByBrep
    (
      DB.Document doc,
      ref DB.DirectShape element,

      Rhino.Geometry.Brep brep
    )
    {
      ThrowIfNotValid(nameof(brep), brep);

      if (element is DB.DirectShape ds) { }
      else ds = DB.DirectShape.CreateElement(doc, new DB.ElementId(DB.BuiltInCategory.OST_GenericModel));

      var shape = brep.ToShape();
      ds.SetShape(shape);

      ReplaceElement(ref element, ds);
    }
  }
}
