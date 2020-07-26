using System;
using System.Linq;
using DB = Autodesk.Revit.DB;
using DBX = RhinoInside.Revit.External.DB;

namespace RhinoInside.Revit.GH.Types
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using Kernel.Attributes;

  [
    ComponentGuid("83088978-8B44-4154-ABC9-A7CA53CA65E5"),
    Name("Parameter Class"),
    Description("Represents a Revit Parameter class."),
  ]
  public class ParameterClass : GH_Enum<DBX.ParameterClass> { }

  [
    ComponentGuid("2A5D36DD-CD94-4306-963B-D9312DAEB0F9"),
    Name("Parameter Binding"),
    Description("Represents a Revit parameter binding type."),
  ]
  public class ParameterBinding : GH_Enum<DBX.ParameterBinding> { }

  [
    ComponentGuid("A3621A84-190A-48C2-9B0C-F5784B78089C"),
    Name("Storage Type"),
    Description("Represents a Revit storage type."),
  ]
  public class StorageType : GH_Enum<DB.StorageType> { }

  [
    ComponentGuid("A5EA05A9-C17E-48F4-AC4C-34F169AE4F9A"),
    Name("Parameter Type"),
    Description("Represents a Revit parameter type."),
  ]
  public class ParameterType : GH_Enum<DB.ParameterType> { }

  [
    ComponentGuid("38E9E729-9D9F-461F-A1D7-798CDFA2CD4C"),
    Name("Unit Type"),
    Description("Represents a Revit unit type."),
  ]
  public class UnitType : GH_Enum<DB.UnitType>
  {
    public UnitType() : base(DB.UnitType.UT_Undefined) { }
    public UnitType(DB.UnitType value) : base(value) { }

    public override bool IsEmpty => Value == DB.UnitType.UT_Undefined;

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      Enum.GetValues(typeof(DB.UnitType)).Cast<int>().
      Where(x => x != (int) DB.UnitType.UT_Custom && x != (int) DB.UnitType.UT_Undefined).
      ToDictionary(x => x, x => DB.LabelUtils.GetLabelFor((DB.UnitType) x))
    );
  }

  [
    ComponentGuid("ABE3F6CB-CE2D-4DBE-AB81-A6CB884D7DE1"),
    Name("Unit System"),
    Description("Represents a Revit unit system."),
  ]
  public class UnitSystem : GH_Enum<DB.UnitSystem> { }

  [
    ComponentGuid("3D9979B4-65C8-447F-BCEA-3705249DF3B6"),
    Name("Parameter Group"),
    Description("Represents a Revit parameter group."),
  ]
  public class BuiltInParameterGroup : GH_Enum<DB.BuiltInParameterGroup>
  {
    public BuiltInParameterGroup() : base(DB.BuiltInParameterGroup.INVALID) { }

    public override bool IsEmpty => Value == DB.BuiltInParameterGroup.INVALID;

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      Enum.GetValues(typeof(DB.BuiltInParameterGroup)).Cast<int>().
      OrderBy(x => DB.LabelUtils.GetLabelFor((DB.BuiltInParameterGroup) x)).
      ToDictionary(x => x, x => DB.LabelUtils.GetLabelFor((DB.BuiltInParameterGroup) x))
    );
  }

  [
    ComponentGuid("195B9D7E-D4B0-4335-A442-3C2FA40794A2"),
    Name("Category Type"),
    Description("Represents a Revit parameter category type."),
  ]
  public class CategoryType : GH_Enum<DB.CategoryType>
  {
    public CategoryType() : base(DB.CategoryType.Invalid) { }
    public CategoryType(DB.CategoryType value) : base(value) { }

    public override bool IsEmpty => Value == DB.CategoryType.Invalid;
    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.CategoryType.Model,            "Model"       },
        { (int) DB.CategoryType.Annotation,       "Annotation"  },
        { (int) DB.CategoryType.Internal,         "Internal"    },
        { (int) DB.CategoryType.AnalyticalModel,  "Analytical"  },
      }
    );
  }

  [
    ComponentGuid("1AF2E8BF-5FAF-41AD-9A2F-EB96A706587C"),
    Name("Graphics Style Type"),
    Description("Represents a graphics style type."),
  ]
  public class GraphicsStyleType : GH_Enum<DB.GraphicsStyleType>
  {
    public GraphicsStyleType() : base(DB.GraphicsStyleType.Projection) { }
    public GraphicsStyleType(DB.GraphicsStyleType value) : base(value) { }
  }

  [
    ComponentGuid("F992A251-4085-4525-A514-298F3155DF8A"),
    Name("Detail Level"),
    Description("Represents a view detail level."),
  ]
  public class ViewDetailLevel : GH_Enum<DB.ViewDetailLevel>
  {
    public override bool IsEmpty => Value == DB.ViewDetailLevel.Undefined;
  }

  [
    ComponentGuid("83380EFC-D2E2-3A9E-A1D7-939EC71852DD"),
    Name("View Discipline"),
    Description("Represents a Revit view discipline."),
  ]
  public class ViewDiscipline : GH_Enum<DB.ViewDiscipline>
  {
    public override bool IsEmpty => Value == 0;
  }

  [
    ComponentGuid("485C3278-0D1A-445D-B3DA-75FB8CD38CF9"),
    Name("View Family"),
    Description("Represents a Revit view family."),
  ]
  public class ViewFamily : GH_Enum<DB.ViewFamily>
  {
    public ViewFamily() : base(DB.ViewFamily.Invalid) { }
    public ViewFamily(DB.ViewFamily value) : base(value) { }

    public override bool IsEmpty => Value == DB.ViewFamily.Invalid;

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.ViewFamily.ThreeDimensional,         "3D View"                   },
        { (int) DB.ViewFamily.FloorPlan,                "Floor Plan"                },
        { (int) DB.ViewFamily.CeilingPlan,              "Ceiling Plan"              },
        { (int) DB.ViewFamily.StructuralPlan,           "Structural Plan"           },
        { (int) DB.ViewFamily.AreaPlan,                 "Area Plan"                 },
        { (int) DB.ViewFamily.Elevation,                "Elevation"                 },
        { (int) DB.ViewFamily.Section,                  "Section"                   },
        { (int) DB.ViewFamily.Detail,                   "Detail View"               },
        { (int) DB.ViewFamily.Drafting,                 "Drafting View"             },
        { (int) DB.ViewFamily.ImageView,                "Rendering"                 },
        { (int) DB.ViewFamily.Walkthrough,              "Walkthrough"               },
        { (int) DB.ViewFamily.Legend,                   "Legend"                    },
        { (int) DB.ViewFamily.Sheet,                    "Sheet"                     },
        { (int) DB.ViewFamily.Schedule,                 "Schedule"                  },
        { (int) DB.ViewFamily.GraphicalColumnSchedule,  "Graphical Column Schedule" },
        { (int) DB.ViewFamily.PanelSchedule,            "Panel Schedule"            },
        { (int) DB.ViewFamily.CostReport,               "Cost Report"               },
        { (int) DB.ViewFamily.LoadsReport,              "Loads Report"              },
        { (int) DB.ViewFamily.PressureLossReport,       "Pressure Loss Report"      },
      }
    );
  }

  [
    ComponentGuid("BF051011-660D-39E7-86ED-20EEE3A68DB0"),
    Name("View Type"),
    Description("Represents a Revit view type."),
  ]
  public class ViewType : GH_Enum<DB.ViewType>
  {
    public override bool IsEmpty => Value == DB.ViewType.Undefined;
  }

  [
    ComponentGuid("2FDE857C-EDAB-4999-B6AE-DC531DD2AD18"),
    Name("Image Fit direction type"),
    Description("Represents a Revit fit direction type."),
  ]
  public class FitDirectionType : GH_Enum<DB.FitDirectionType>
  {
    public FitDirectionType() : base(DB.FitDirectionType.Horizontal) { }
    public FitDirectionType(DB.FitDirectionType value) : base(value) { }
  }

  [
    ComponentGuid("C6132D3E-1BA4-4BF5-B40C-D08F81A79AB1"),
    Name("Image Resolution"),
    Description("Represents a Revit image resolution."),
  ]
  public class ImageResolution : GH_Enum<DB.ImageResolution>
  {
    public ImageResolution() : base(DB.ImageResolution.DPI_72) { }
    public ImageResolution(DB.ImageResolution value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.ImageResolution.DPI_72,   "72 DPI" },
        { (int) DB.ImageResolution.DPI_150, "150 DPI" },
        { (int) DB.ImageResolution.DPI_300, "300 DPI" },
        { (int) DB.ImageResolution.DPI_600, "600 DPI" },
      }
    );
  }

  [
    ComponentGuid("F6BABEFF-C4AD-49D0-81D6-9C3CD021DD45"),
    Name("Image FileType"),
    Description("Represents a Revit image file type."),
  ]
  public class ImageFileType : GH_Enum<DB.ImageFileType>
  {
    public ImageFileType() : base(DB.ImageFileType.BMP) { }
    public ImageFileType(DB.ImageFileType value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.ImageFileType.BMP,           "BMP" },
        { (int) DB.ImageFileType.JPEGLossless,  "JPEG-Lossless" },
        { (int) DB.ImageFileType.JPEGMedium,    "JPEG-Medium" },
        { (int) DB.ImageFileType.JPEGSmallest,  "JPEG-Smallest" },
        { (int) DB.ImageFileType.PNG,           "PNG" },
        { (int) DB.ImageFileType.TARGA,         "TARGA" },
        { (int) DB.ImageFileType.TIFF,          "TIFF" },
      }
    );
  }

  [
    ComponentGuid("2A3E4872-EF41-442A-B886-8B7DBA73DFE2"),
    Name("Wall Location Line"),
    Description("Represents a Revit wall location line."),
  ]
  public class WallLocationLine : GH_Enum<DB.WallLocationLine>
  {
    public WallLocationLine() : base() { }
    public WallLocationLine(DB.WallLocationLine value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.WallLocationLine.WallCenterline,      "Wall Centerline"       },
        { (int) DB.WallLocationLine.CoreCenterline,      "Core Centerline"       },
        { (int) DB.WallLocationLine.FinishFaceExterior,  "Finish Face: Exterior" },
        { (int) DB.WallLocationLine.FinishFaceInterior,  "Finish Face: Interior" },
        { (int) DB.WallLocationLine.CoreExterior,        "Core Face: Exterior"   },
        { (int) DB.WallLocationLine.CoreInterior,        "Core Face: Interior"   },
      }
    );
  }

  [
    ComponentGuid("2FEFFADD-BD29-4B19-9682-4CC5947DF11C"),
    Name("Wall System Family"),
    Description("Represents a Revit wall system family"),
  ]
  public class WallSystemFamily : GH_Enum<DB.WallKind>
  {
    public WallSystemFamily() : base(DB.WallKind.Unknown) { }
    public WallSystemFamily(DB.WallKind value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.WallKind.Basic,      "Basic Wall"    },
        { (int) DB.WallKind.Curtain,    "Curtain Wall"  },
        { (int) DB.WallKind.Stacked,    "Stacked Wall"  },
      }
    );
  }

  [
    ComponentGuid("F069304B-4066-4D23-9542-7AC54CED3C92"),
    Name("Wall Function"),
    Description("Represents a Revit wall function"),
  ]
  public class WallFunction : GH_Enum<DB.WallFunction> {
    public WallFunction() : base() { }
    public WallFunction(DB.WallFunction value) : base(value) { }
  }

  [
    ComponentGuid("7A71E012-6E92-493D-960C-83BE3C50ECAE"),
    Name("Wall Wrapping"),
    Description("Represents a Revit wall wrapping option"),
  ]
  public class WallWrapping : GH_Enum<DBX.WallWrapping>
  {
    public WallWrapping() : base() { }
    public WallWrapping(DBX.WallWrapping value) : base(value) { }
  }

  [
    ComponentGuid("2F1CE55B-FD85-4EC5-8638-8DA06932DE0E"),
    Name("Structural Wall Usage"),
    Description("Represents a Revit structural wall usage."),
  ]
  public class StructuralWallUsage : GH_Enum<DB.Structure.StructuralWallUsage> {
    public StructuralWallUsage() : base() { }
    public StructuralWallUsage(DB.Structure.StructuralWallUsage value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DB.Structure.StructuralWallUsage.NonBearing,  "Non-Bearing"         },
        { (int) DB.Structure.StructuralWallUsage.Bearing,     "Bearing"             },
        { (int) DB.Structure.StructuralWallUsage.Shear,       "Shear"               },
        { (int) DB.Structure.StructuralWallUsage.Combined,    "Structural combined" },
      }
    );
  }

  [
    ComponentGuid("A8122936-6A69-4D78-B1F5-13FD8F2144A5"),
    Name("End Cap Condition"),
    Description("Represents end cap condition of a wall compound structure"),
  ]
  public class EndCapCondition : GH_Enum<DB.EndCapCondition>
  {
    public EndCapCondition() : base() { }
    public EndCapCondition(DB.EndCapCondition value) : base(value) { }
  }

  [
    ComponentGuid("68D22DE2-CDD5-4441-9745-462E28030A03"),
    Name("Deck Embedding Type"),
    Description("Represents deck embedding type of a wall compound structure layer"),
  ]
  public class DeckEmbeddingType : GH_Enum<DB.StructDeckEmbeddingType>
  {
    public DeckEmbeddingType() : base() { }
    public DeckEmbeddingType(DB.StructDeckEmbeddingType value) : base(value) { }
  }

  [
    ComponentGuid("4220F183-C273-4342-9885-3DEB13531731"),
    Name("Layer Function"),
    Description("Represents layer function of a wall compound structure layer"),
  ]
  public class LayerFunction : GH_Enum<DB.MaterialFunctionAssignment>
  {
    public LayerFunction() : base() { }
    public LayerFunction(DB.MaterialFunctionAssignment value) : base(value) { }
  }

  [
    ComponentGuid("BF8B68B5-4E24-4602-8065-7EE90536B90E"),
    Name("Opening Wrapping Condition"),
    Description("Represents compound structure layers wrapping at openings setting"),
  ]
  public class OpeningWrappingCondition : GH_Enum<DB.OpeningWrappingCondition>
  {
    public OpeningWrappingCondition() : base() { }
    public OpeningWrappingCondition(DB.OpeningWrappingCondition value) : base(value) { }
  }

  [
    ComponentGuid("621785D8-363C-46EF-A920-B8CF0026B4CF"),
    Name("Curtain Grid Align Type"),
    Description("Represents alignment type for curtain grids at either direction"),
  ]
  public class CurtainGridAlignType : GH_Enum<DB.CurtainGridAlignType>
  {
    public CurtainGridAlignType() : base() { }
    public CurtainGridAlignType(DB.CurtainGridAlignType value) : base(value) { }
  }

  [
    ComponentGuid("A734FF65-D9E6-4C8C-A413-B5EACD6E3062"),
    Name("Curtain Grid Layout"),
    Description("Represents layout for curtain grids at either direction"),
  ]
  public class CurtainGridLayout : GH_Enum<DBX.CurtainGridLayout>
  {
    public CurtainGridLayout() : base() { }
    public CurtainGridLayout(DBX.CurtainGridLayout value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DBX.CurtainGridLayout.None,            "None"             },
        { (int) DBX.CurtainGridLayout.FixedDistance,   "Fixed Distance"   },
        { (int) DBX.CurtainGridLayout.FixedNumber,     "Fixed Number"     },
        { (int) DBX.CurtainGridLayout.MaximumSpacing,  "Maximum Spacing"  },
        { (int) DBX.CurtainGridLayout.MinimumSpacing,  "Minimum Spacing"  },
      }
    );
  }

  [
    ComponentGuid("371E482B-BB95-4D9D-962F-00867E01AB35"),
    Name("Curtain Grid Join Condition"),
    Description("Represents join condition for curtain grids at either direction"),
  ]
  public class CurtainGridJoinCondition : GH_Enum<DBX.CurtainGridJoinCondition>
  {
    public CurtainGridJoinCondition() : base() { }
    public CurtainGridJoinCondition(DBX.CurtainGridJoinCondition value) : base(value) { }

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DBX.CurtainGridJoinCondition.NotDefined,                        "Not Defined" },
        { (int) DBX.CurtainGridJoinCondition.VerticalGridContinuous,            "Vertical Grid Continuous" },
        { (int) DBX.CurtainGridJoinCondition.HorizontalGridContinuous,          "Horizontal Grid Continuous" },
        { (int) DBX.CurtainGridJoinCondition.BorderAndVerticalGridContinuous,   "Border & Vertical Grid Continuous" },
        { (int) DBX.CurtainGridJoinCondition.BorderAndHorizontalGridContinuous, "Border & Horizontal Grid Continuous" },
      }
    );
  }

  [
    ComponentGuid("C61AA1B8-4CB2-44A0-9217-091E151D1D0A"),
    Name("Curtain Mullion System Family"),
    Description("Represents builtin curtain mullion system families"),
  ]
  public class CurtainMullionSystemFamily : GH_Enum<DBX.CurtainMullionSystemFamily>
  {
    public CurtainMullionSystemFamily() : base(DBX.CurtainMullionSystemFamily.Unknown) { }
    public CurtainMullionSystemFamily(DBX.CurtainMullionSystemFamily value) : base(value) { }

    public override bool IsEmpty => Value == DBX.CurtainMullionSystemFamily.Unknown;

    public static new ReadOnlyDictionary<int, string> NamedValues { get; } = new ReadOnlyDictionary<int, string>
    (
      new Dictionary<int, string>
      {
        { (int) DBX.CurtainMullionSystemFamily.Unknown,         "Unknown"         },
        { (int) DBX.CurtainMullionSystemFamily.Rectangular,     "Rectangular"     },
        { (int) DBX.CurtainMullionSystemFamily.Circular,        "Circular"        },
        { (int) DBX.CurtainMullionSystemFamily.LCorner,         "L Corner"        },
        { (int) DBX.CurtainMullionSystemFamily.TrapezoidCorner, "Trapezoid Corner"},
        { (int) DBX.CurtainMullionSystemFamily.QuadCorner,      "Quad Corner"     },
        { (int) DBX.CurtainMullionSystemFamily.VCorner,         "V Corner"        },
      }
    );
  }

  [
    ComponentGuid("9F9D90FC-06FF-4908-B67E-ED63B089937E"),
    Name("Curtain Panel System Family"),
    Description("Represents builtin curtain panel system families"),
  ]
  public class CurtainPanelSystemFamily : GH_Enum<DBX.CurtainPanelSystemFamily>
  {
    public CurtainPanelSystemFamily() : base() { }
    public CurtainPanelSystemFamily(DBX.CurtainPanelSystemFamily value) : base(value) { }
  }

  [
    ComponentGuid("CF3ACC14-D9F3-4169-985B-C207260250DA"),
    Name("Floor Function"),
    Description("Represents builtin floor function"),
  ]
  public class FloorFunction : GH_Enum<DBX.FloorFunction>
  {
  }
}
