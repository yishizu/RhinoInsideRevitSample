using System;
using Grasshopper.Kernel;

using DB = Autodesk.Revit.DB;
using DBX = RhinoInside.Revit.External.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class AnalyzeCurtainGridMullionType : AnalysisComponent
  {
    public override Guid ComponentGuid => new Guid("66A9F189-D2BD-4E47-8C97-A469E3DD861B");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;
    protected override string IconTag => "ACGMT";

    public AnalyzeCurtainGridMullionType() : base(
      name: "Analyze Mullion Type",
      nickname: "A-MT",
      description: "Analyze given mullion type",
      category: "Revit",
      subCategory: "Wall"
    )
    {
    }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(
        param: new Parameters.ElementType(),
        name: "Mullion Type",
        nickname: "CGMT",
        description: "Curtain Grid Mullion Type",
        access: GH_ParamAccess.item
        );
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      manager.AddParameter(
        param: new Parameters.Param_Enum<Types.CurtainMullionSystemFamily>(),
        name: "Mullion System Family",
        nickname: "MSF",
        description: "Mullion System Family",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Angle",
        nickname: "A",
        description: "Mullion type angle",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Offset",
        nickname: "O",
        description: "Mullion type offset",
        access: GH_ParamAccess.item
        );
      manager.AddParameter(
        param: new Parameters.ElementType(),
        name: "Profile",
        nickname: "PRF",
        description: "Mullion type profile",
        access: GH_ParamAccess.item
        );
      manager.AddParameter(
        param: new Parameters.ElementType(),
        name: "Position",
        nickname: "POS",
        description: "Mullion type position",
        access: GH_ParamAccess.item
        );
      manager.AddBooleanParameter(
        name: "Corner Mullion",
        nickname: "CM",
        description: "Whether mullion type is a corner mullion",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Thickness",
        nickname: "T",
        description: "Mullion thickness for rectangular mullions. Calculated based on mullion system family",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Depth 1",
        nickname: "D1",
        description: "Mullion depth 1 for rectangular mullions. Calculated based on mullion system family",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Depth 2",
        nickname: "D2",
        description: "Mullion depth 2 for rectangular mullions. Calculated based on mullion system family",
        access: GH_ParamAccess.item
        );
      manager.AddNumberParameter(
        name: "Radius",
        nickname: "R",
        description: "Mullion radius for circular mullions. Calculated based on mullion system family",
        access: GH_ParamAccess.item
        );
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      // get input
      DB.MullionType mullionType = default;
      if (!DA.GetData("Mullion Type", ref mullionType))
        return;

      // determine the mullion type by looking at parameters
      // this chart shows param availability on various mullion types
      // almost all types have unique param types except for L and V mullions
      // L and V mullions are differentiated by testing the FamilyName of the mullion
      // L and V mullion FamilyName always start with "L " or "V " in any language
      //
      //                  | Circ | Rect | Quad | Trapiz | L | V
      // --------------------------------------------------------
      // Radius           |  X   |  -   |  -   |  -     | - | -
      // Leg 1            |  -   |  -   |  -   |  -     | X | X
      // Leg 2            |  -   |  -   |  -   |  -     | X | X
      // Thickness        |  -   |  X   |  -   |  -     | X | X
      // Is Corner        |  0   |  0   |  1   |  1     | 1 | 1
      // Depth 1          |  -   |  -   |  X   |  -     | - | -
      // Depth 2          |  -   |  -   |  X   |  -     | - | -
      // Width Side 1     |  -   |  X   |  -   |  -     | - | -
      // Width Side 2     |  -   |  X   |  -   |  -     | - | -
      // Profile          |  X   |  X   |  -   |  -     | - | -
      // Depth            |  -   |  -   |  -   |  X     | - | -
      // Center Width     |  -   |  -   |  -   |  X     | - | -
      //
      // Mullion System Families (Values are defined here not in the API)
      //    Rectangular      = 0
      //    Circular         = 1
      //    L Corner         = 2
      //    Trapezoid Corner = 3
      //    Quad Corner      = 4
      //    V Corner         = 5
      var hasRadius = mullionType.get_Parameter(DB.BuiltInParameter.CIRC_MULLION_RADIUS) != null;
      var hasRectWidthside1 = mullionType.get_Parameter(DB.BuiltInParameter.RECT_MULLION_WIDTH1) != null;
      var hasCustWidthside1 = mullionType.get_Parameter(DB.BuiltInParameter.CUST_MULLION_WIDTH1) != null;
      var hasDepth1 = mullionType.get_Parameter(DB.BuiltInParameter.MULLION_DEPTH1) != null;
      var hasCenterWidth = mullionType.get_Parameter(DB.BuiltInParameter.TRAP_MULL_WIDTH) != null;
      var hasLeg1 = mullionType.get_Parameter(DB.BuiltInParameter.LV_MULLION_LEG1) != null;

      var mullionSystemFamily = DBX.CurtainMullionSystemFamily.Unknown;
      // rectangular
      if (hasRectWidthside1 || hasCustWidthside1)
        mullionSystemFamily = DBX.CurtainMullionSystemFamily.Rectangular;
      // cicular
      else if (hasRadius)
        mullionSystemFamily = DBX.CurtainMullionSystemFamily.Circular;
      // quad
      else if (hasDepth1)
        mullionSystemFamily = DBX.CurtainMullionSystemFamily.QuadCorner;
      // trapezoid
      else if (hasCenterWidth)
        mullionSystemFamily = DBX.CurtainMullionSystemFamily.TrapezoidCorner;
      // corner L or V
      else if (hasLeg1)
      {
        // confirmed that the corner mullion system family name in other languages also starts with L or V
        if (mullionType.FamilyName.StartsWith("L "))
          mullionSystemFamily = DBX.CurtainMullionSystemFamily.LCorner;
        else if (mullionType.FamilyName.StartsWith("V "))
          mullionSystemFamily = DBX.CurtainMullionSystemFamily.VCorner;
      }

      DA.SetData("Mullion System Family", mullionSystemFamily);

      PipeHostParameter(DA, mullionType, DB.BuiltInParameter.MULLION_ANGLE, "Angle");
      PipeHostParameter(DA, mullionType, DB.BuiltInParameter.MULLION_OFFSET, "Offset");

      var profile = new Types.MullionProfile(mullionType.Document, mullionType.get_Parameter(DB.BuiltInParameter.MULLION_PROFILE).AsElementId());
      DA.SetData("Profile", profile);

      var position = new Types.MullionPosition(mullionType.Document, mullionType.get_Parameter(DB.BuiltInParameter.MULLION_POSITION).AsElementId());
      DA.SetData("Position", position);

      PipeHostParameter(DA, mullionType, DB.BuiltInParameter.MULLION_CORNER_TYPE, "Corner Mullion");

      // output params are reused for various mullion types
      //
      //                  | Circ | Rect | Quad | Trapiz | L  | V
      // ----------------------------------------------------------
      // Radius           |  R   |  -   |  -   |  -     | -  | -
      // Leg 1            |  -   |  -   |  -   |  -     | D1 | D1
      // Leg 2            |  -   |  -   |  -   |  -     | D2 | D2
      // Thickness        |  -   |  T   |  -   |  -     | T  | T
      // Depth 1          |  -   |  -   |  D1  |  -     | -  | -
      // Depth 2          |  -   |  -   |  D2  |  -     | -  | -
      // Width Side 1     |  -   |  D1  |  -   |  -     | -  | -
      // Width Side 2     |  -   |  D2  |  -   |  -     | -  | -
      // Depth            |  -   |  -   |  -   |  D1    | -  | -
      // Center Width     |  -   |  -   |  -   |  T     | -  | -
      // Diameter         |  T   <= custom calculated for circular mullions
      var thicknessParam =
        mullionType.get_Parameter(DB.BuiltInParameter.RECT_MULLION_THICK) ??
        mullionType.get_Parameter(DB.BuiltInParameter.CUST_MULLION_THICK) ??
        mullionType.get_Parameter(DB.BuiltInParameter.TRAP_MULL_WIDTH);
      DA.SetData("Thickness", thicknessParam?.AsDoubleInRhinoUnits());

      var depth1Param =
        mullionType.get_Parameter(DB.BuiltInParameter.RECT_MULLION_WIDTH1) ??
        mullionType.get_Parameter(DB.BuiltInParameter.CUST_MULLION_WIDTH1) ??
        mullionType.get_Parameter(DB.BuiltInParameter.LV_MULLION_LEG1) ??
        mullionType.get_Parameter(DB.BuiltInParameter.MULLION_DEPTH1) ??
        mullionType.get_Parameter(DB.BuiltInParameter.MULLION_DEPTH);
      DA.SetData("Depth 1", depth1Param?.AsDoubleInRhinoUnits());

      var depth2Param =
        mullionType.get_Parameter(DB.BuiltInParameter.RECT_MULLION_WIDTH2) ??
        mullionType.get_Parameter(DB.BuiltInParameter.CUST_MULLION_WIDTH2) ??
        mullionType.get_Parameter(DB.BuiltInParameter.LV_MULLION_LEG2) ??
        mullionType.get_Parameter(DB.BuiltInParameter.MULLION_DEPTH2);
      DA.SetData("Depth 2", depth2Param?.AsDoubleInRhinoUnits());

      var radiusParam = mullionType.get_Parameter(DB.BuiltInParameter.CIRC_MULLION_RADIUS);
      if (radiusParam != null)
      {
        DA.SetData("Radius", radiusParam?.AsDoubleInRhinoUnits());
        DA.SetData("Thickness", radiusParam?.AsDoubleInRhinoUnits() * 2);
      }
    }
  }
}
