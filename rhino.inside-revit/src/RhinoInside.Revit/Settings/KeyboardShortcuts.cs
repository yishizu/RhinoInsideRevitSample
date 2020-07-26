using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace RhinoInside.Revit.Settings
{
  public static class KeyboardShortcuts
  {
    [Serializable()]
    public class ShortcutItem
    {
      [XmlAttribute()] public string CommandName { get; set; }
      [XmlAttribute()] public string CommandId { get; set; }
      [XmlAttribute()] public string Shortcuts { get; set; }
      [XmlAttribute()] public string Paths { get; set; }
    }

    [Serializable(), XmlRoot("Shortcuts", Namespace = "")]
    public class Shortcuts : List<ShortcutItem> { }

    internal static bool LoadFrom(string keyboardShortcutsPath, out Shortcuts shortcuts)
    {
      try
      {
        using (var ReadFileStream = new FileStream(keyboardShortcutsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          var serializer = new XmlSerializer(typeof(Shortcuts));
          shortcuts = serializer.Deserialize(ReadFileStream) as Shortcuts;
          return true;
        }
      }
      catch (FileNotFoundException)
      {
        shortcuts = null;
        return false;
      }
    }

    internal static void LoadFromResources(string keyboardShortcutsId, out Shortcuts shortcuts)
    {
      using (var ReadFileStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"RhinoInside.Revit.{keyboardShortcutsId}"))
      {
        var serializer = new XmlSerializer(typeof(Shortcuts));
        shortcuts = serializer.Deserialize(ReadFileStream) as Shortcuts;
      }
    }

    internal static bool SaveAs(Shortcuts shortcuts, string keyboardShortcutsPath)
    {
      try
      {
        using (var WriteFileStream = new StreamWriter(keyboardShortcutsPath))
        {
          var ns = new XmlSerializerNamespaces();
          ns.Add(string.Empty, string.Empty);

          var serializer = new XmlSerializer(typeof(Shortcuts));
          serializer.Serialize(WriteFileStream, shortcuts, ns);
          return true;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    public static bool RegisterDefaultShortcut(string tabName, string panelName, string commandId, string commandName, string commandShortcuts)
    {
      commandId = $"CustomCtrl_%CustomCtrl_%{tabName}%{panelName}%{commandId}";

      string keyboardShortcutsPath = Path.Combine(Revit.CurrentUsersDataFolderPath, "KeyboardShortcuts.xml");
      if (!File.Exists(keyboardShortcutsPath))
        keyboardShortcutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", $"RVT {Revit.ApplicationUI.ControlledApplication.VersionNumber}", "UserDataCache", "KeyboardShortcuts.xml");

      if (!LoadFrom(keyboardShortcutsPath, out var shortcuts))
        LoadFromResources($"Resources.RVT{Revit.ApplicationUI.ControlledApplication.VersionNumber}.KeyboardShortcuts.xml", out shortcuts);

#if DEBUG
      // Those lines generate the KeyboardShortcuts.xml template file when new Revit version is supported
      string keyboardShortcutsTemplatePath = Path.Combine(Addin.SourceCodePath, "Resources", $"RVT{Revit.ApplicationUI.ControlledApplication.VersionNumber}", "KeyboardShortcuts.xml");
      var info = new FileInfo(keyboardShortcutsTemplatePath);
      if (info.Length == 0)
      {
        var shortcutsSummary = new Shortcuts();
        foreach (var shortcutItem in shortcuts.OrderBy(x => x.CommandId))
        {
          if (!string.IsNullOrEmpty(shortcutItem.Shortcuts))
          {
            var shortcutDefinition = new ShortcutItem
            {
              CommandId = shortcutItem.CommandId,
              Shortcuts = shortcutItem.Shortcuts
            };
            shortcutsSummary.Add(shortcutDefinition);
          }
        }

        SaveAs(shortcutsSummary, keyboardShortcutsTemplatePath);
      }
#endif

      bool shortcutUpdated = false;
      try
      {
        var shortcutItem = shortcuts.Where(x => x.CommandId == commandId).First();
        if (shortcutItem.Shortcuts is null)
        {
          shortcutItem.Shortcuts = commandShortcuts;
          shortcutUpdated = true;
        }
      }
      catch (InvalidOperationException)
      {
        var shortcutItem = new ShortcutItem()
        {
          CommandName = commandName,
          CommandId = commandId,
          Shortcuts = commandShortcuts,
          Paths = $"{tabName}>{panelName}"
        };
        shortcuts.Add(shortcutItem);
        shortcutUpdated = true;
      }

      if (shortcutUpdated)
        SaveAs(shortcuts, Path.Combine(Revit.CurrentUsersDataFolderPath, "KeyboardShortcuts.xml"));

      return shortcutUpdated;
    }
 }
}
