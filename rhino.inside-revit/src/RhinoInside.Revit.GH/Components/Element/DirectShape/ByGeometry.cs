using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.External.DB.Extensions;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  using Kernel.Attributes;

  public class DirectShapeByGeometry : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("0bfbda45-49cc-4ac6-8d6d-ecd2cfed062a");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    public DirectShapeByGeometry() : base
    (
      "Add Geometry DirectShape", "GeoDShape",
      "Given its Geometry, it adds a DirectShape element to the active Revit document",
      "Revit", "DirectShape"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.GraphicalElement(), "DirectShape", "DS", "New DirectShape", GH_ParamAccess.item);
    }

    void ReconstructDirectShapeByGeometry
    (
      DB.Document doc,
      ref DB.Element element,

      [Optional] string name,
      Optional<DB.Category> category,
      IList<IGH_GeometricGoo> geometry,
      [Optional] IList<DB.Material> material
    )
    {
      SolveOptionalCategory(ref category, doc, DB.BuiltInCategory.OST_GenericModel, nameof(geometry));

      if (element is DB.DirectShape ds && ds.Category.Id == category.Value.Id) { }
      else ds = DB.DirectShape.CreateElement(doc, category.Value.Id);

      using (var ga = GeometryEncoder.Context.Push())
      {
        var materialIndex = 0;
        var materialCount = material?.Count ?? 0;

        var shape = geometry.
                    Select(x => AsGeometryBase(x)).
                    Where(x => ThrowIfNotValid(nameof(geometry), x)).
                    SelectMany(x =>
                    {
                      if (materialCount > 0)
                      {
                        ga.MaterialId =
                        (
                          materialIndex < materialCount ?
                          material[materialIndex++]?.Id :
                          material[materialCount - 1]?.Id
                        ) ??
                        DB.ElementId.InvalidElementId;
                      }

                      return x.ToShape();
                    }).
                    ToList();

        ds.SetShape(shape);
      }

      ds.Name = name ?? string.Empty;

      ReplaceElement(ref element, ds);
    }

    Rhino.Geometry.GeometryBase AsGeometryBase(IGH_GeometricGoo obj)
    {
      if (obj is GH_Curve curve)
        return curve.Value;

      if (obj is GH_Brep brep)
        return brep.Value;

      if (obj is GH_SubD subD)
        return subD.Value;

      if (obj is GH_Mesh mesh)
        return mesh.Value;

      var scriptVariable = obj.ScriptVariable();
      switch (scriptVariable)
      {
        case Rhino.Geometry.Point3d point: return new Rhino.Geometry.Point(point);
        case Rhino.Geometry.Line line: return new Rhino.Geometry.LineCurve(line);
        case Rhino.Geometry.Rectangle3d rect: return rect.ToNurbsCurve();
        case Rhino.Geometry.Arc arc: return new Rhino.Geometry.ArcCurve(arc);
        case Rhino.Geometry.Circle circle: return new Rhino.Geometry.ArcCurve(circle);
        case Rhino.Geometry.Ellipse ellipse: return ellipse.ToNurbsCurve();
        case Rhino.Geometry.Box box: return box.ToBrep();
      }

      return scriptVariable as Rhino.Geometry.GeometryBase;
    }
  }

  public class DirectShapeTypeByGeometry : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("25DCFE8E-5BE9-460C-80E8-51B7041D8FED");
    public override GH_Exposure Exposure => GH_Exposure.primary;

    public DirectShapeTypeByGeometry() : base
    (
      "Add DirectShapeType", "DShapeTyp",
      "Given its Geometry, it reconstructs a DirectShapeType to the active Revit document",
      "Revit", "DirectShape"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.ElementType(), "Type", "T", "New DirectShapeType", GH_ParamAccess.item);
    }

    void ReconstructDirectShapeTypeByGeometry
    (
      DB.Document doc,
      ref DB.ElementType elementType,

      string name,
      Optional<DB.Category> category,
      IList<IGH_GeometricGoo> geometry,
      [Optional] IList<DB.Material> material
    )
    {
      SolveOptionalCategory(ref category, doc, DB.BuiltInCategory.OST_GenericModel, nameof(geometry));

      if (elementType is DB.DirectShapeType directShapeType && directShapeType.Category.Id == category.Value.Id) { }
      else directShapeType = DB.DirectShapeType.Create(doc, name, category.Value.Id);

      using (var ga = GeometryEncoder.Context.Push())
      {
        var materialIndex = 0;
        var materialCount = material?.Count ?? 0;

        var shape = geometry.
                    Select(x => AsGeometryBase(x)).
                    Where(x => ThrowIfNotValid(nameof(geometry), x)).
                    SelectMany(x =>
                    {
                      if (materialCount > 0)
                      {
                        ga.MaterialId = (
                                         materialIndex < materialCount ?
                                         material[materialIndex++]?.Id :
                                         material[materialCount - 1]?.Id
                                        ) ??
                                        DB.ElementId.InvalidElementId;
                      }

                      return x.ToShape();
                    }).
                    ToList();

        directShapeType.SetShape(shape);
      }

      directShapeType.Name = name;

      ReplaceElement(ref elementType, directShapeType);
    }

    Rhino.Geometry.GeometryBase AsGeometryBase(IGH_GeometricGoo obj)
    {
      var scriptVariable = obj.ScriptVariable();
      switch (scriptVariable)
      {
        case Rhino.Geometry.Point3d point: return new Rhino.Geometry.Point(point);
        case Rhino.Geometry.Line line: return new Rhino.Geometry.LineCurve(line);
        case Rhino.Geometry.Rectangle3d rect: return rect.ToNurbsCurve();
        case Rhino.Geometry.Arc arc: return new Rhino.Geometry.ArcCurve(arc);
        case Rhino.Geometry.Circle circle: return new Rhino.Geometry.ArcCurve(circle);
        case Rhino.Geometry.Ellipse ellipse: return ellipse.ToNurbsCurve();
        case Rhino.Geometry.Box box: return box.ToBrep();
      }

      return scriptVariable as Rhino.Geometry.GeometryBase;
    }
  }

  public class DirectShapeByLocation : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("A811EFA4-8DE2-46F3-9F88-3D4F13FE40BE");
    public override GH_Exposure Exposure => GH_Exposure.primary;

    public DirectShapeByLocation() : base
    (
      "Add DirectShape", "DShape",
      "Given its location, it reconstructs a DirectShape into the active Revit document",
      "Revit", "DirectShape"
    )
    { }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(new Parameters.GraphicalElement(), "DirectShape", "DS", "New DirectShape", GH_ParamAccess.item);
    }

    void ReconstructDirectShapeByLocation
    (
      DB.Document doc,
      ref DB.Element element,

      [Description("Location where to place the element. Point or plane is accepted.")]
      Rhino.Geometry.Plane location,
      DB.DirectShapeType type
    )
    {
      if (element is DB.DirectShape ds && ds.Category.Id == type.Category.Id) { }
      else ds = DB.DirectShape.CreateElement(doc, type.Category.Id);

      if(ds.TypeId != type.Id)
        ds.SetTypeId(type.Id);

      using (var library = DB.DirectShapeLibrary.GetDirectShapeLibrary(doc))
      {
        if (!library.ContainsType(type.UniqueId))
          library.AddDefinitionType(type.UniqueId, type.Id);
      }

      using (var transform = Rhino.Geometry.Transform.PlaneToPlane(Rhino.Geometry.Plane.WorldXY, location).ToTransform())
      {
        ds.SetShape(DB.DirectShape.CreateGeometryInstance(doc, type.UniqueId, transform));
      }

      var parametersMask = new DB.BuiltInParameter[]
      {
        DB.BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
        DB.BuiltInParameter.ELEM_FAMILY_PARAM,
        DB.BuiltInParameter.ELEM_TYPE_PARAM
      };

      ReplaceElement(ref element, ds, parametersMask);
    }
  }
}
