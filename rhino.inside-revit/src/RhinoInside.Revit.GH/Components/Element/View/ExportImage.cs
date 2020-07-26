using System;
using System.IO;
using Grasshopper.Kernel;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  public class ViewExportImage : Component
  {
    public override Guid ComponentGuid => new Guid("4A962A0C-46A0-4A5F-B727-6747B715A975");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;
    protected override string IconTag => "IMG";

    public ViewExportImage() : base
    (
      "Export View Image", "ViewImage",
      "Exports a view into an image file",
      "Revit", "View"
    )
    { }

    protected override void RegisterInputParams(GH_InputParamManager manager)
    {
      manager.AddParameter(new Parameters.View(), "View", "V", string.Empty, GH_ParamAccess.item);

      var folderPath = new Grasshopper.Kernel.Parameters.Param_FilePath();
      manager[manager.AddParameter(folderPath, "Folder", "F", $"Default is {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\%ProjectTitle%", GH_ParamAccess.item)].Optional = true;

      manager.AddBooleanParameter("OverrideFile", "O", string.Empty, GH_ParamAccess.item, false);

      var fileType = new Parameters.Param_Enum<Types.ImageFileType>();
      fileType.PersistentData.Append(new Types.ImageFileType(DB.ImageFileType.PNG));
      manager.AddParameter(fileType, "FileType", "T", "The file type used for export", GH_ParamAccess.item);

      var imageResolution = new Parameters.Param_Enum<Types.ImageResolution>();
      imageResolution.PersistentData.Append(new Types.ImageResolution());
      manager.AddParameter(imageResolution, "Resolution", "R", "The image resolution in dots per inch", GH_ParamAccess.item);

      var fitDirectionType = new Parameters.Param_Enum<Types.FitDirectionType>();
      fitDirectionType.PersistentData.Append(new Types.FitDirectionType());
      manager.AddParameter(fitDirectionType, "FitDirection", "D", "The fit direction", GH_ParamAccess.item);

      manager.AddIntegerParameter("PixelSize", "S", "The pixel size of an image in specified direction", GH_ParamAccess.item, 2048);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager manager)
    {
      var imageFilePath = new Grasshopper.Kernel.Parameters.Param_FilePath();
      manager.AddParameter(imageFilePath, "Image File", "I", string.Empty, GH_ParamAccess.item);
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      var view = default(DB.View);
      if (!DA.GetData("View", ref view))
        return;

      var folder = default(string);
      DA.GetData("Folder", ref folder);

      if (string.IsNullOrEmpty(folder))
        folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), view.Document.Title);

      Directory.CreateDirectory(folder);

      var overrideFile = default(bool);
      if (!DA.GetData("OverrideFile", ref overrideFile))
        return;

      var fileType = default(DB.ImageFileType);
      if (!DA.GetData("FileType", ref fileType))
        return;

      var resolution = default(DB.ImageResolution);
      if (!DA.GetData("Resolution", ref resolution))
        return;

      var fitDirection = default(DB.FitDirectionType);
      if (!DA.GetData("FitDirection", ref fitDirection))
        return;

      var pixelSize = default(int);
      if (!DA.GetData("PixelSize", ref pixelSize))
        return;

      var viewName = DB.ImageExportOptions.GetFileName(view.Document, view.Id);
      var options = new DB.ImageExportOptions()
      {
        ZoomType = DB.ZoomFitType.FitToPage,
        FitDirection          = fitDirection, // DB.FitDirectionType.Horizontal,
        PixelSize             = pixelSize,    // 2048,
        ImageResolution       = resolution,   // DB.ImageResolution.DPI_72,
        ShadowViewsFileType   = fileType,     // DB.ImageFileType.PNG,
        HLRandWFViewsFileType = fileType,     // DB.ImageFileType.PNG,
        ExportRange = DB.ExportRange.SetOfViews,
        FilePath = folder + Path.DirectorySeparatorChar
      };

      options.SetViewsAndSheets(new DB.ElementId[] { view.Id });

      var filePath = Path.Combine(options.FilePath, viewName);
      switch (options.ShadowViewsFileType)
      {
        case DB.ImageFileType.BMP:          filePath += ".bmp"; break;
        case DB.ImageFileType.JPEGLossless: filePath += ".jpg"; break;
        case DB.ImageFileType.JPEGMedium:   filePath += ".jpg"; break;
        case DB.ImageFileType.JPEGSmallest: filePath += ".jpg"; break;
        case DB.ImageFileType.PNG:          filePath += ".png"; break;
        case DB.ImageFileType.TARGA:        filePath += ".tga"; break;
        case DB.ImageFileType.TIFF:         filePath += ".tif"; break;
      }

      if (!overrideFile && File.Exists(filePath))
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"File '{filePath}' already exists.");
      else
        view.Document.ExportImage(options);

      DA.SetData("Image File", filePath);
    }
  }
}
