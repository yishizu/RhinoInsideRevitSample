using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.InteropExtension;

using Autodesk.Revit.Attributes;
using DB = Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#if REVIT_2018
using Autodesk.Revit.DB.Visual;
#else
using Autodesk.Revit.Utility;
#endif

using Rhino;
using Rhino.Geometry;
using Rhino.FileIO;
using Rhino.DocObjects;
using RhinoInside.Revit.Convert.Geometry;
using RhinoInside.Revit.Convert.System.Drawing;

namespace RhinoInside.Revit.UI
{
  [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
  public class CommandImport : RhinoCommand
  {
    public static void CreateUI(RibbonPanel ribbonPanel)
    {
      var buttonData = NewPushButtonData<CommandImport, NeedsActiveDocument<Availability>>("Import");

      if (ribbonPanel.AddItem(buttonData) is PushButton pushButton)
      {
        pushButton.ToolTip = "Imports geometry from 3dm file to a Revit model or family";
        pushButton.Image = ImageBuilder.LoadBitmapImage("Resources.Ribbon.Rhinoceros.Import-3DM.png", true);
        pushButton.LargeImage = ImageBuilder.LoadBitmapImage("Resources.Ribbon.Rhinoceros.Import-3DM.png");
        pushButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://github.com/mcneel/rhino.inside-revit/tree/master#sample-8"));
      }
    }

    #region Categories
    static Dictionary<string, DB.Category> GetCategoriesByName(DB.Document doc)
    {
      return doc.OwnerFamily.FamilyCategory.SubCategories.
        OfType<DB.Category>().
        ToDictionary(x => x.Name, x => x);
    }

    static DB.ElementId ImportLayer
    (
      DB.Document doc,
      File3dm model,
      Layer layer,
      Dictionary<string, DB.Category> categories,
      Dictionary<string, DB.Material> materials
    )
    {
      var id = DB.ElementId.InvalidElementId;

      if (layer.HasName)
      {
        var matName = layer.Name;
        if (categories.TryGetValue(matName, out var category)) id = category.Id;
        else
        {
          var familyCategory = doc.OwnerFamily.FamilyCategory;
          if (familyCategory.CanAddSubcategory)
          {
            if (doc.Settings.Categories.NewSubcategory(familyCategory, layer.Name) is DB.Category subCategory)
            {
              subCategory.LineColor = layer.Color.ToColor();

              var modelMaterial = layer.RenderMaterialIndex >= 0 ? model.AllMaterials.FindIndex(layer.RenderMaterialIndex) : default;
              if (modelMaterial is object)
              {
                if (doc.GetElement(ImportMaterial(doc, modelMaterial, materials)) is DB.Material material)
                  subCategory.Material = material;
              }

              categories.Add(subCategory.Name, subCategory);
              id = subCategory.Id;
            }
          }
        }
      }

      return id;
    }
    #endregion

    #region Materials
    static Dictionary<string, DB.Material> GetMaterialsByName(DB.Document doc)
    {
      var collector = new DB.FilteredElementCollector(doc);
      return collector.OfClass(typeof(DB.Material)).OfType<DB.Material>().
        GroupBy(x => x.Name).
        ToDictionary(x => x.Key, x => x.First());
    }

    static string GenericAssetName(Autodesk.Revit.ApplicationServices.LanguageType language)
    {
      switch (language)
      {
        case Autodesk.Revit.ApplicationServices.LanguageType.English_USA:           return "Generic";
        case Autodesk.Revit.ApplicationServices.LanguageType.German:                return "Generisch";
        case Autodesk.Revit.ApplicationServices.LanguageType.Spanish:               return "Genérico";
        case Autodesk.Revit.ApplicationServices.LanguageType.French:                return "Générique";
        case Autodesk.Revit.ApplicationServices.LanguageType.Italian:               return "Generico";
        case Autodesk.Revit.ApplicationServices.LanguageType.Dutch:                 return "Allgemeine";
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Simplified:    return "常规";
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Traditional:   return "常規";
        case Autodesk.Revit.ApplicationServices.LanguageType.Japanese:              return "一般";
        case Autodesk.Revit.ApplicationServices.LanguageType.Korean:                return "일반";
        case Autodesk.Revit.ApplicationServices.LanguageType.Russian:               return "общий";
        case Autodesk.Revit.ApplicationServices.LanguageType.Czech:                 return "Obecný";
        case Autodesk.Revit.ApplicationServices.LanguageType.Polish:                return "Rodzajowy";
        case Autodesk.Revit.ApplicationServices.LanguageType.Hungarian:             return "Generikus";
        case Autodesk.Revit.ApplicationServices.LanguageType.Brazilian_Portuguese:  return "Genérico";
        #if REVIT_2018
        case Autodesk.Revit.ApplicationServices.LanguageType.English_GB:            return "Generic";
        #endif
      }

      return null;
    }

    static string GenericAssetName() => GenericAssetName(Revit.ActiveUIApplication.Application.Language) ?? "Generic";

    static DB.AppearanceAssetElement GetGenericAppearanceAssetElement(DB.Document doc)
    {
      var applicationLanguage = Revit.ActiveUIApplication.Application.Language;
      var languages = Enumerable.Repeat(applicationLanguage, 1).
      Concat
      (
        Enum.GetValues(typeof(Autodesk.Revit.ApplicationServices.LanguageType)).
        Cast<Autodesk.Revit.ApplicationServices.LanguageType>().
        Where(lang => lang != applicationLanguage && lang != Autodesk.Revit.ApplicationServices.LanguageType.Unknown)
      );

      foreach (var lang in languages)
      {
        if (DB.AppearanceAssetElement.GetAppearanceAssetElementByName(doc, GenericAssetName(lang)) is DB.AppearanceAssetElement assetElement)
          return assetElement;
      }

      return null;
    }

    static DB.ElementId ImportMaterial
    (
      DB.Document doc,
      Rhino.Render.RenderMaterial mat
    )
    {
      string name = mat.Name;
      var appearanceAssetId = DB.ElementId.InvalidElementId;

#if REVIT_2019
      if (DB.AppearanceAssetElement.GetAppearanceAssetElementByName(doc, name) is DB.AppearanceAssetElement appearanceAssetElement)
        appearanceAssetId = appearanceAssetElement.Id;
      else
      {
        appearanceAssetElement = GetGenericAppearanceAssetElement(doc);
        if (appearanceAssetElement is null)
        {
          var assets = Revit.ActiveUIApplication.Application.GetAssets(AssetType.Appearance);
          foreach (var asset in assets)
          {
            if (asset.Name == GenericAssetName())
            {
              appearanceAssetElement = DB.AppearanceAssetElement.Create(doc, name, asset);
              appearanceAssetId = appearanceAssetElement.Id;
              break;
            }
          }
        }
        else
        {
          appearanceAssetElement = appearanceAssetElement.Duplicate(name);
          appearanceAssetId = appearanceAssetElement.Id;
        }

        if (appearanceAssetId != DB.ElementId.InvalidElementId)
        {
          using (var editScope = new AppearanceAssetEditScope(doc))
          {
            var editableAsset = editScope.Start(appearanceAssetId);

            //var category = editableAsset.FindByName("category") as AssetPropertyString;
            //category.Value = $":{mat.Category.FirstCharUpper()}";

            var description = editableAsset.FindByName("description") as AssetPropertyString;
            description.Value = mat.Notes ?? string.Empty;

            var keyword = editableAsset.FindByName("keyword") as AssetPropertyString;
            {
              string tags = string.Empty;
              foreach (var tag in (mat.Tags ?? string.Empty).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                tags += $":{tag.Replace(':', ';')}";
              keyword.Value = tags;
            }

            if (mat.SmellsLikeMetal || mat.SmellsLikeTexturedMetal)
            {
              var generic_self_illum_luminance = editableAsset.FindByName(Generic.GenericIsMetal) as AssetPropertyBoolean;
              generic_self_illum_luminance.Value = true;
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Diffuse, out Rhino.Display.Color4f diffuse))
            {
              var generic_diffuse = editableAsset.FindByName(Generic.GenericDiffuse) as AssetPropertyDoubleArray4d;
              generic_diffuse.SetValueAsDoubles(new double[] { diffuse.R, diffuse.G, diffuse.B, diffuse.A });
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Transparency, out double transparency))
            {
              var generic_transparency = editableAsset.FindByName(Generic.GenericTransparency) as AssetPropertyDouble;
              generic_transparency.Value = transparency;

              if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.TransparencyColor, out Rhino.Display.Color4f transparencyColor))
              {
                diffuse = diffuse.BlendTo((float) transparency, transparencyColor);

                var generic_diffuse = editableAsset.FindByName(Generic.GenericDiffuse) as AssetPropertyDoubleArray4d;
                generic_diffuse.SetValueAsDoubles(new double[] { diffuse.R, diffuse.G, diffuse.B, diffuse.A });
              }
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Ior, out double ior))
            {
              var generic_refraction_index = editableAsset.FindByName(Generic.GenericRefractionIndex) as AssetPropertyDouble;
              generic_refraction_index.Value = ior;
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Shine, out double shine))
            {
              if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Specular, out Rhino.Display.Color4f specularColor))
              {
                var generic_reflectivity_at_0deg = editableAsset.FindByName(Generic.GenericReflectivityAt0deg) as AssetPropertyDouble;
                generic_reflectivity_at_0deg.Value = shine * specularColor.L;
              }
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Reflectivity, out double reflectivity))
            {
              if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.ReflectivityColor, out Rhino.Display.Color4f reflectivityColor))
              {
                var generic_reflectivity_at_90deg = editableAsset.FindByName(Generic.GenericReflectivityAt90deg) as AssetPropertyDouble;
                generic_reflectivity_at_90deg.Value = reflectivity * reflectivityColor.L;

                if (mat.Fields.TryGetValue("fresnel-enabled", out bool fresnel_enabled) && !fresnel_enabled)
                {
                  diffuse = diffuse.BlendTo((float) reflectivity, reflectivityColor);
                  var generic_diffuse = editableAsset.FindByName(Generic.GenericDiffuse) as AssetPropertyDoubleArray4d;
                  generic_diffuse.SetValueAsDoubles(new double[] { diffuse.R, diffuse.G, diffuse.B, diffuse.A });
                }
              }
            }

            if (mat.Fields.TryGetValue("polish-amount", out double polish_amount))
            {
              var generic_glossiness = editableAsset.FindByName(Generic.GenericGlossiness) as AssetPropertyDouble;
              generic_glossiness.Value = polish_amount;
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.Emission, out Rhino.Display.Color4f emission))
            {
              var generic_self_illum_filter_map = editableAsset.FindByName(Generic.GenericSelfIllumFilterMap) as AssetPropertyDoubleArray4d;
              generic_self_illum_filter_map.SetValueAsDoubles(new double[] { emission.R, emission.G, emission.B, emission.A });
            }

            if (mat.Fields.TryGetValue(Rhino.Render.RenderMaterial.BasicMaterialParameterNames.DisableLighting, out bool self_illum))
            {
              var generic_self_illum_luminance = editableAsset.FindByName(Generic.GenericSelfIllumLuminance) as AssetPropertyDouble;
              generic_self_illum_luminance.Value = self_illum ? 200000 : 0.0;
            }

            editScope.Commit(false);
          }
        }
      }
#endif

      return appearanceAssetId;
    }

    static DB.ElementId ImportMaterial
    (
      DB.Document doc,
      Material mat,
      Dictionary<string, DB.Material> materials
    )
    {
      var id = DB.ElementId.InvalidElementId;

      if(mat.HasName)
      {
        var matName = mat.Name;
        if (materials.TryGetValue(matName, out var material)) id = material.Id;
        else
        {
          id = DB.Material.Create(doc, matName);
          var newMaterial = doc.GetElement(id) as DB.Material;

          newMaterial.Color         = mat.PreviewColor.ToColor();
          newMaterial.Shininess     = (int) Math.Round(mat.Shine / Material.MaxShine * 128.0);
          newMaterial.Smoothness    = (int) Math.Round(mat.Reflectivity * 100.0);
          newMaterial.Transparency  = (int) Math.Round(mat.Transparency * 100.0);
          newMaterial.AppearanceAssetId = ImportMaterial(doc, mat.RenderMaterial);

          materials.Add(matName, newMaterial);
        }
      }

      return id;
    }
    #endregion

    #region Project
    static IList<DB.GeometryObject> ImportObject
    (
      DB.Document doc,
      File3dm model,
      GeometryBase geometry,
      ObjectAttributes attributes,
      Dictionary<string, DB.Material> materials,
      double scaleFactor
    )
    {
      var layer = model.AllLayers.FindIndex(attributes.LayerIndex);
      if (layer?.IsVisible ?? false)
      {
        using (var ctx = GeometryEncoder.Context.Push())
        {
          switch (attributes.MaterialSource)
          {
            case ObjectMaterialSource.MaterialFromObject:
              {
                var modelMaterial = attributes.MaterialIndex < 0 ? Material.DefaultMaterial : model.AllMaterials.FindIndex(attributes.MaterialIndex);
                ctx.MaterialId = ImportMaterial(doc, modelMaterial, materials);
                break;
              }
            case ObjectMaterialSource.MaterialFromLayer:
              {
                var modelLayer = model.AllLayers.FindIndex(attributes.LayerIndex);
                var modelMaterial = modelLayer.RenderMaterialIndex < 0 ? Material.DefaultMaterial : model.AllMaterials.FindIndex(modelLayer.RenderMaterialIndex);
                ctx.MaterialId = ImportMaterial(doc, modelMaterial, materials);
                break;
              }
          }

          if (geometry is InstanceReferenceGeometry instance)
          {
            if (model.AllInstanceDefinitions.FindId(instance.ParentIdefId) is InstanceDefinitionGeometry definition)
            {
              var definitionId = definition.Id.ToString();
              var library = DB.DirectShapeLibrary.GetDirectShapeLibrary(doc);
              if (!library.Contains(definitionId))
              {
                var objectIds = definition.GetObjectIds();
                var GNodes = objectIds.
                  Select(x => model.Objects.FindId(x)).
                  Cast<File3dmObject>().
                  SelectMany(x => ImportObject(doc, model, x.Geometry, x.Attributes, materials, scaleFactor));

                library.AddDefinition(definitionId, GNodes.ToArray());
              }

              return DB.DirectShape.CreateGeometryInstance(doc, definitionId, instance.Xform.ToTransform(scaleFactor));
            }
          }
          else return geometry.ToShape(scaleFactor);
        }
      }

      return new DB.GeometryObject[0];
    }

    static Result Import3DMFileToProject
    (
      DB.Document doc,
      string filePath,
      DB.BuiltInCategory builtInCategory
    )
    {
      try
      {
        DB.DirectShapeLibrary.GetDirectShapeLibrary(doc).Reset();

        using (var model = File3dm.Read(filePath))
        {
          var scaleFactor = RhinoMath.UnitScale(model.Settings.ModelUnitSystem, Revit.ModelUnitSystem);

          using (var trans = new DB.Transaction(doc, "Import 3D Model"))
          {
            if (trans.Start() == DB.TransactionStatus.Started)
            {
              var categoryId = new DB.ElementId(builtInCategory);
              var materials = GetMaterialsByName(doc);

              var type = DB.DirectShapeType.Create(doc, Path.GetFileName(filePath), categoryId);

              foreach (var obj in model.Objects.Where(x => !x.Attributes.IsInstanceDefinitionObject && x.Attributes.Space == ActiveSpace.ModelSpace))
              {
                if (!obj.Attributes.Visible)
                  continue;

                var geometryList = ImportObject(doc, model, obj.Geometry, obj.Attributes, materials, scaleFactor).ToArray();
                if (geometryList?.Length > 0)
                {
                  try { type.AppendShape(geometryList); }
                  catch (Autodesk.Revit.Exceptions.ArgumentException) { }
                }
              }

              var ds = DB.DirectShape.CreateElement(doc, type.Category.Id);
              ds.SetTypeId(type.Id);

              var library = DB.DirectShapeLibrary.GetDirectShapeLibrary(doc);
              if (!library.ContainsType(type.UniqueId))
                library.AddDefinitionType(type.UniqueId, type.Id);

              ds.SetShape(DB.DirectShape.CreateGeometryInstance(doc, type.UniqueId, DB.Transform.Identity));

              if (trans.Commit() == DB.TransactionStatus.Committed)
              {
                var elements = new DB.ElementId[] { ds.Id };
                Revit.ActiveUIDocument.Selection.SetElementIds(elements);
                Revit.ActiveUIDocument.ShowElements(elements);

                return Result.Succeeded;
              }
            }
          }
        }
      }
      finally
      {
        DB.DirectShapeLibrary.GetDirectShapeLibrary(doc).Reset();
      }

      return Result.Failed;
    }
    #endregion

    #region Family
    static Result Import3DMFileToFamily
    (
      DB.Document doc,
      string filePath
    )
    {
      using (var model = File3dm.Read(filePath))
      {
        var scaleFactor = RhinoMath.UnitScale(model.Settings.ModelUnitSystem, Revit.ModelUnitSystem);

        using (var trans = new DB.Transaction(doc, "Import 3D Model"))
        {
          if (trans.Start() == DB.TransactionStatus.Started)
          {
            var materials = GetMaterialsByName(doc);
            var categories = GetCategoriesByName(doc);
            var elements = new List<DB.ElementId>();

            var view3D = default(DB.View);
            using (var collector = new DB.FilteredElementCollector(doc))
            {
              var elementCollector = collector.OfClass(typeof(DB.View3D));
              view3D = elementCollector.Cast<DB.View3D>().Where(x => x.Name == "{3D}").FirstOrDefault();
            }

            if (view3D is object)
            {
              foreach (var cplane in model.AllNamedConstructionPlanes)
              {
                var plane = cplane.Plane;
                var bubbleEnd = plane.Origin.ToXYZ(scaleFactor);
                var freeEnd = (plane.Origin + plane.XAxis).ToXYZ(scaleFactor);
                var cutVec = plane.YAxis.ToXYZ();

                var refrencePlane = doc.FamilyCreate.NewReferencePlane(bubbleEnd, freeEnd, cutVec, view3D);
                refrencePlane.Name = cplane.Name;
                refrencePlane.Maximize3DExtents();
              }
            }

            foreach (var obj in model.Objects.Where(x => !x.Attributes.IsInstanceDefinitionObject && x.Attributes.Space == ActiveSpace.ModelSpace))
            {
              if (!obj.Attributes.Visible)
                continue;

              var layer = model.AllLayers.FindIndex(obj.Attributes.LayerIndex);
              if (layer?.IsVisible != true)
                continue;

              var geometry = obj.Geometry;
              if (geometry is Extrusion extrusion) geometry = extrusion.ToBrep();
              else if (geometry is SubD subD) geometry = subD.ToBrep();

              try
              {
                switch (geometry)
                {
                  case Point point:
                    if (doc.OwnerFamily.IsConceptualMassFamily)
                    {
                      var referncePoint = doc.FamilyCreate.NewReferencePoint(point.Location.ToXYZ(scaleFactor));
                      elements.Add(referncePoint.Id);
                    }
                    break;
                  case Curve curve:
                    if (curve.TryGetPlane(out var plane, Revit.VertexTolerance))
                    {
                      if (curve.ToCurve(scaleFactor) is DB.Curve crv)
                      {
                        var sketchPlane = DB.SketchPlane.Create(doc, plane.ToPlane(scaleFactor));
                        var modelCurve = doc.FamilyCreate.NewModelCurve(crv, sketchPlane);

                        elements.Add(modelCurve.Id);

                        {
                          var subCategoryId = ImportLayer(doc, model, layer, categories, materials);
                          if (DB.Category.GetCategory(doc, subCategoryId) is DB.Category subCategory)
                          {
                            var familyGraphicsStyle = subCategory?.GetGraphicsStyle(DB.GraphicsStyleType.Projection);

                            if (familyGraphicsStyle is object)
                              modelCurve.Subcategory = familyGraphicsStyle;
                          }
                        }
                      }
                    }
                    else if (DB.DirectShape.IsSupportedDocument(doc) && DB.DirectShape.IsValidCategoryId(doc.OwnerFamily.FamilyCategory.Id, doc))
                    {
                      var subCategoryId = ImportLayer(doc, model, layer, categories, materials);
                      var shape = curve.ToShape();
                      if (shape.Length > 0)
                      {
                        var ds = DB.DirectShape.CreateElement(doc, doc.OwnerFamily.FamilyCategory.Id);
                        ds.SetShape(shape);
                        elements.Add(ds.Id);
                      }
                    }
                    break;
                  case Brep brep:
                    if (brep.ToSolid(scaleFactor) is DB.Solid solid)
                    {
                      if (DB.FreeFormElement.Create(doc, solid) is DB.FreeFormElement freeForm)
                      {
                        elements.Add(freeForm.Id);

                        {
                          var categoryId = ImportLayer(doc, model, layer, categories, materials);
                          if (categoryId != DB.ElementId.InvalidElementId)
                            freeForm.get_Parameter(DB.BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).Set(categoryId);
                        }

                        if (obj.Attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject)
                        {
                          if (model.AllMaterials.FindIndex(obj.Attributes.MaterialIndex) is Material material)
                          {
                            var categoryId = ImportMaterial(doc, material, materials);
                            if (categoryId != DB.ElementId.InvalidElementId)
                              freeForm.get_Parameter(DB.BuiltInParameter.MATERIAL_ID_PARAM).Set(categoryId);
                          }
                        }
                      }
                    }
                    break;
                }
              }
              catch (Autodesk.Revit.Exceptions.ArgumentException) { }
            }

            if (trans.Commit() == DB.TransactionStatus.Committed)
            {
              Revit.ActiveUIDocument.Selection.SetElementIds(elements);
              Revit.ActiveUIDocument.ShowElements(elements);

              return Result.Succeeded;
            }
          }
        }
      }

      return Result.Failed;
    }
    #endregion

    public override Result Execute(ExternalCommandData data, ref string message, DB.ElementSet elements)
    {
      var doc = data.Application.ActiveUIDocument.Document;
      if(!doc.IsFamilyDocument && !DB.DirectShape.IsSupportedDocument(doc))
      {
        message = "Active document dont't support DirectShape functionality.";
        return Result.Failed;
      }

      using
      (
        var openFileDialog = new OpenFileDialog()
        {
          Filter = "Rhino 3D models (*.3dm)|*.3dm",
          FilterIndex = 1,
          RestoreDirectory = true
        }
      )
      {

        switch (openFileDialog.ShowDialog(Revit.MainWindowHandle))
        {
          case DialogResult.OK:
            if (doc.IsFamilyDocument)
            {
              return Import3DMFileToFamily
              (
                doc,
                openFileDialog.FileName
              );
            }
            else
            {
              return Import3DMFileToProject
              (
                doc,
                openFileDialog.FileName,
                CommandGrasshopperBake.ActiveBuiltInCategory
              );
            }
          case DialogResult.Cancel: return Result.Cancelled;
        }
      }

      return Result.Failed;
    }
  }
}
