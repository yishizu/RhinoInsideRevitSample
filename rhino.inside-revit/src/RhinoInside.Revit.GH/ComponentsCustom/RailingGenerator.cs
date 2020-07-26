using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using RhinoInside.Revit;
using RhinoInside.Revit.Convert.Geometry;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.ComponentsCustom
{
  public class RailingGenerator : GH_Component
  {
    /// <summary>
    /// Initializes a new instance of the RailingGenerator class.
    /// </summary>
    public RailingGenerator()
      : base("RailingGenerator", "Nickname",
          "Description",
          "GEL", "Revit")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddCurveParameter("Curve", "C", "Curve for railing path", GH_ParamAccess.item);
      pManager.AddParameter(new Parameters.ElementType());
      pManager.AddParameter(new Parameters.Level());
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
      pManager.AddParameter(new Parameters.Element(), "Railing", "Rail", "This is the railing that get created by this node", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      Curve railingCurve = null;
      if (!DA.GetData(0, ref railingCurve))
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Your railing curve is not set");
        return;
      }

      DB.CurveLoop myCurveLoop = railingCurve.ToCurveLoop();
      DB.ElementType railingType = null;
      if (!DA.GetData(1, ref railingType))
      {
        return;
      }

      DB.Level level = null;
      if (!DA.GetData(2, ref level)) return;

      DB.Architecture.Railing railing = RhinoInside.Revit.Rhinoceros.InvokeInHostContext(() =>
        CreateRailing(Revit.ActiveDBDocument, myCurveLoop, railingType, level));

      DA.SetData(0, railing);
    }

    private DB.Architecture.Railing CreateRailing(DB.Document doc, DB.CurveLoop curveLoop, DB.ElementType type,
      DB.Level level)
    {
      DB.Architecture.Railing result = null;
      using (var transaction = new DB.Transaction(doc, this.Name))
      {
        transaction.Start();
        result = DB.Architecture.Railing.Create(doc, curveLoop, type.Id, level.Id);

        transaction.Commit();
      }

      return result;
    }
    /// <summary>
    /// Provides an Icon for the component.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
    {
      get
      {
        //You can add image files to your project resources and access them like this:
        // return Resources.IconForThisComponent;
        return null;
      }
    }

    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("03ef08af-0a1e-45fd-9133-4928d564891e"); }
    }
  }
}
