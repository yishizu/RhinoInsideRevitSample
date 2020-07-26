using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using DB = Autodesk.Revit.DB;


namespace RhinoInside.Revit.GH.Parameters
{
  public class WallFunction_ValueList : GH_ValueList
  {
    public override Guid ComponentGuid => new Guid("C84653DD-3B44-4059-820E-127761665305");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    public WallFunction_ValueList()
    {
      Category = "Revit";
      SubCategory = "Input";
      Name = "Wall Function";
      NickName = "WF";
      Description = "Picker for builtin predefined Wall functions";

      ListItems.Clear();
      ListItems.Add(
        new GH_ValueListItem("Interior", ((int) DB.WallFunction.Interior).ToString())
        );
      ListItems.Add(
        new GH_ValueListItem("Exterior", ((int) DB.WallFunction.Exterior).ToString())
        );
      ListItems.Add(
        new GH_ValueListItem("Foundation", ((int) DB.WallFunction.Foundation).ToString())
        );
      ListItems.Add(
        new GH_ValueListItem("Retaining", ((int) DB.WallFunction.Retaining).ToString())
        );
      ListItems.Add(
        new GH_ValueListItem("Soffit", ((int) DB.WallFunction.Soffit).ToString())
        );
      ListItems.Add(
        new GH_ValueListItem("Core-Shaft", ((int) DB.WallFunction.Coreshaft).ToString())
        );
    }
  }
}
