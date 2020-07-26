using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Rhino.Geometry;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.Convert.System.Collections.Generic;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class TopographyByPoints : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("E8D8D05A-8703-4F75-B106-12B40EC9DF7B");
    public override GH_Exposure Exposure => GH_Exposure.primary;

    public TopographyByPoints() : base
    (
      "Add Topography (Points)", "Topography",
      "Given a set of Points, it adds a Topography surface to the active Revit document",
      "Revit", "Site"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.GraphicalElement(), "Topography", "T", "New Topography", GH_ParamAccess.item);
    }

    void ReconstructTopographyByPoints
    (
      DB.Document doc,
      ref DB.Architecture.TopographySurface element,

      IList<Point3d> points,
      [Optional] IList<Curve> regions
    )
    {
      var xyz = points.ConvertAll(GeometryEncoder.ToXYZ);

      //if (element is DB.Architecture.TopographySurface topography)
      //{
      //  using (var editScope = new DB.Architecture.TopographyEditScope(doc, GetType().Name))
      //  {
      //    editScope.Start(element.Id);
      //    topography.DeletePoints(topography.GetPoints());
      //    topography.AddPoints(xyz);

      //    foreach (var subRegionId in topography.GetHostedSubRegionIds())
      //      doc.Delete(subRegionId);

      //    editScope.Commit(this);
      //  }
      //}
      //else
      {
        ReplaceElement(ref element, DB.Architecture.TopographySurface.Create(doc, xyz));
      }

      if (element is object && regions?.Count > 0)
      {
        var curveLoops = regions.Select(region => region.ToCurveLoop());
        DB.Architecture.SiteSubRegion.Create(doc, curveLoops.ToList(), element.Id);
      }
    }
  }
}
