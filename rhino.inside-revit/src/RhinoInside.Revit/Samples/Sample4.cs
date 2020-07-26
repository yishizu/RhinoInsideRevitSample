using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Input;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using Rhino.Geometry;
using Rhino.PlugIns;
using GH_IO.Serialization;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;

using RhinoInside.Revit.UI;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.Convert.System.Collections.Generic;

namespace RhinoInside.Revit.Samples
{
  public abstract class Sample4 : GrasshopperCommand
  {
    public static void CreateUI(RibbonPanel ribbonPanel)
    {
      mruPullDownButton = ribbonPanel.AddItem(new PulldownButtonData("cmdRhinoInside.GrasshopperPlayer", "Sample 4")) as Autodesk.Revit.UI.PulldownButton;
      if (mruPullDownButton != null)
      {
        mruPullDownButton.ToolTip = "Loads and evals a Grasshopper definition";
        mruPullDownButton.Image = ImageBuilder.BuildImage("4");
        mruPullDownButton.LargeImage = ImageBuilder.BuildLargeImage("4");
        mruPullDownButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://github.com/mcneel/rhino.inside-revit/tree/master#sample-4"));

        AddPushButton<Browse, NeedsActiveDocument<Availability>> (mruPullDownButton, "Browse…", "Browse for a Grasshopper definition to evaluate");
      }
    }

    static PulldownButton mruPullDownButton = null;
    static PushButton[] mruPushPuttons = null;

    bool AddFileToMru(string filePath)
    {
      if (!File.Exists(filePath))
        return false;

      if(mruPushPuttons is null)
      {
        mruPullDownButton.AddSeparator();
        mruPushPuttons = new PushButton[]
        {
          AddPushButton<Mru0, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
          AddPushButton<Mru1, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
          AddPushButton<Mru2, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
          AddPushButton<Mru3, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
          AddPushButton<Mru4, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
          AddPushButton<Mru5, NeedsActiveDocument<Availability>>(mruPullDownButton, null, null),
        };

        foreach (var mru in mruPushPuttons)
        {
          mru.Visible = false;
          mru.Enabled = false;
        }
      }

      int lastItemToMove = 0;
      for (int m = 0; m < mruPushPuttons.Length; ++m)
      {
        lastItemToMove++;

        if (mruPushPuttons[m].ToolTip == filePath)
          break;
      }

      int itemsToMove = lastItemToMove - 1;
      if (itemsToMove > 0)
      {
        for (int m = lastItemToMove - 1; m > 0; --m)
        {
          mruPushPuttons[m].Visible = mruPushPuttons[m - 1].Visible;
          mruPushPuttons[m].Enabled = mruPushPuttons[m - 1].Enabled;
          mruPushPuttons[m].ToolTipImage = mruPushPuttons[m - 1].ToolTipImage;
          mruPushPuttons[m].LongDescription = mruPushPuttons[m - 1].LongDescription;
          mruPushPuttons[m].ToolTip = mruPushPuttons[m - 1].ToolTip;
          mruPushPuttons[m].ItemText = mruPushPuttons[m - 1].ItemText;
          mruPushPuttons[m].Image = mruPushPuttons[m - 1].Image;
          mruPushPuttons[m].LargeImage = mruPushPuttons[m - 1].LargeImage;
        }

        mruPushPuttons[0].Visible = true;
        mruPushPuttons[0].Enabled = true;
        mruPushPuttons[0].ToolTipImage = null;
        mruPushPuttons[0].LongDescription = string.Empty;
        mruPushPuttons[0].ToolTip = filePath;
        mruPushPuttons[0].ItemText = Path.GetFileNameWithoutExtension(filePath);
        mruPushPuttons[0].Image = ImageBuilder.BuildImage(Path.GetExtension(filePath).Substring(1));
        mruPushPuttons[0].LargeImage = ImageBuilder.BuildLargeImage(Path.GetExtension(filePath).Substring(1));
      }

      return true;
    }

    Result Execute(ExternalCommandData data, ref string message, ElementSet elements, string filePath)
    {
      if (!AddFileToMru(filePath))
        return Result.Failed;

      var application = data.Application;
      var transactionName = string.Empty;
      var outputs = new List<KeyValuePair<string, List<GeometryBase>>>();

      var CurrentCulture = Thread.CurrentThread.CurrentCulture;
      try
      {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var archive = new GH_Archive();
        if (!archive.ReadFromFile(filePath))
          return Result.Failed;

        using (var definition = new GH_Document())
        {
          if (!archive.ExtractObject(definition, "Definition"))
            return Result.Failed;

          // Update Most recent used item extended ToolTip information
          {
            mruPushPuttons[0].LongDescription = definition.Properties.Description;

            if (archive.GetRootNode.FindChunk("Thumbnail")?.GetDrawingBitmap("Thumbnail") is System.Drawing.Bitmap bitmap)
              mruPushPuttons[0].ToolTipImage = bitmap.ToBitmapImage(Math.Min(bitmap.Width, 355), Math.Min(bitmap.Height, 355));
          }

          transactionName = Path.GetFileNameWithoutExtension(definition.Properties.ProjectFileName);

          var inputs = new List<IGH_Param>();

          // Collect input params
          foreach (var obj in definition.Objects)
          {
            if (!(obj is IGH_Param param))
              continue;

            if (param.Sources.Count != 0 || param.Recipients.Count == 0)
              continue;

            if (param.VolatileDataCount > 0)
              continue;

            if (param.Locked)
              continue;

            inputs.Add(param);
          }

          // Prompt for input values
          Thread.CurrentThread.CurrentCulture = CurrentCulture;
          var values = new Dictionary<IGH_Param, IEnumerable<IGH_Goo>>();
          foreach (var input in inputs.OrderBy((x) => x.Attributes.Pivot.Y))
          {
            switch (input)
            {
              case Param_Box box:
                var boxes = CommandGrasshopperPlayer.PromptBox(application.ActiveUIDocument, input.NickName);
                if (boxes == null)
                  return Result.Cancelled;
                values.Add(input, boxes);
                break;
              case Param_Point point:
                var points = CommandGrasshopperPlayer.PromptPoint(application.ActiveUIDocument, input.NickName);
                if (points == null)
                  return Result.Cancelled;
                values.Add(input, points);
                break;
              case Param_Curve curve:
                var curves = CommandGrasshopperPlayer.PromptEdge(application.ActiveUIDocument, input.NickName);
                if (curves == null)
                  return Result.Cancelled;
                values.Add(input, curves);
                break;
              case Param_Surface surface:
                var surfaces = CommandGrasshopperPlayer.PromptSurface(application.ActiveUIDocument, input.NickName);
                if (surfaces == null)
                  return Result.Cancelled;
                values.Add(input, surfaces);
                break;
              case Param_Brep brep:
                var breps = CommandGrasshopperPlayer.PromptBrep(application.ActiveUIDocument, input.NickName);
                if (breps == null)
                  return Result.Cancelled;
                values.Add(input, breps);
                break;
            }
          }

          Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
          try
          {
            Cursor.Current = Cursors.WaitCursor;

            // Update input volatile data values
            foreach (var value in values)
              value.Key.AddVolatileDataList(new Grasshopper.Kernel.Data.GH_Path(0), value.Value);

            // Collect output values
            foreach (var obj in definition.Objects)
            {
              if (!(obj is IGH_Param param))
                continue;

              if (param.Sources.Count == 0 || param.Recipients.Count != 0)
                continue;

              if (param.Locked)
                continue;

              try
              {
                param.CollectData();
                param.ComputeData();
              }
              catch (Exception e)
              {
                Debug.Fail(e.Source, e.Message);
                param.Phase = GH_SolutionPhase.Failed;
              }

              if (param.Phase == GH_SolutionPhase.Failed)
                return Result.Failed;

              var output = new List<GeometryBase>();
              var volatileData = param.VolatileData;
              if (volatileData.PathCount > 0)
              {
                foreach (var value in param.VolatileData.AllData(true).Select(x => x.ScriptVariable()))
                {
                  switch (value)
                  {
                    case Point3d point:          output.Add(new Rhino.Geometry.Point(point)); break;
                    case GeometryBase geometry:  output.Add(geometry); break;
                  }
                }
              }

              if (output.Count > 0)
                outputs.Add(new KeyValuePair<string, List<GeometryBase>>(param.NickName, output));
            }
          }
          finally
          {
            Cursor.Current = Cursors.Default;
          }
        }
      }
      catch (Exception)
      {
        return Result.Failed;
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = CurrentCulture;
      }

      // Bake output geometry
      if (outputs.Count > 0)
      {
        var doc = application.ActiveUIDocument.Document;

        using (var trans = new Transaction(doc, transactionName))
        {
          if (trans.Start(MethodBase.GetCurrentMethod().DeclaringType.Name) == TransactionStatus.Started)
          {
            var categoryId = new ElementId(CommandGrasshopperBake.ActiveBuiltInCategory);

            foreach (var output in outputs)
            {
              var ds = DirectShape.CreateElement(doc, categoryId);
              ds.Name = output.Key;

              foreach (var shape in output.Value.ConvertAll(ShapeEncoder.ToShape))
              {
                if (shape.Length > 0)
                  ds.AppendShape(shape);
              }
            }

            trans.Commit();
          }
        }
      }

      return Result.Succeeded;
    }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Browse : Sample4
    {
      public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
      {
        var rc = CommandGrasshopperPlayer.BrowseForFile(out var filePath);
        if (rc != Result.Succeeded)
          return rc;

        return Execute(data, ref message, elements, filePath);
      }
    }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru0 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[0].ToolTip); }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru1 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[1].ToolTip); }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru2 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[2].ToolTip); }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru3 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[3].ToolTip); }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru4 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[4].ToolTip); }

    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    public class Mru5 : Sample4 { public override Result Execute(ExternalCommandData data, ref string message, ElementSet elements) => Execute(data, ref message, elements, mruPushPuttons[5].ToolTip); }
  }
}
