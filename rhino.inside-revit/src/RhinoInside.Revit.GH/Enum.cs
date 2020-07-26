using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace RhinoInside.Revit.GH.Types
{
  public abstract class GH_Enumerate : GH_Integer, IComparable
  {
    protected GH_Enumerate() { }
    protected GH_Enumerate(int value) : base(value) { }

    /// <summary>
    /// Gets the validity of this instance. Enums are valid if are defined.
    /// </summary>
    public override bool IsValid => Enum.IsDefined(UnderlyingEnumType, Value);

    /// <summary>
    /// Checks if this Enumerate value is the Empty value. Override this property to define an Empty value.
    /// </summary>
    public virtual bool IsEmpty => false;

    public abstract Type UnderlyingEnumType { get; }
    public override IGH_Goo Duplicate() => (IGH_Goo) MemberwiseClone();

    static Dictionary<Type, Tuple<Type, Type>> LookForEnums(Assembly assembly)
    {
      var result = new Dictionary<Type, Tuple<Type, Type>>();

      foreach (var type in assembly.ExportedTypes.Where(x => x.IsSubclassOf(typeof(GH_Enumerate))))
      {
        bool typeFound = false;
        var gooType = type;
        while (gooType != typeof(GH_Enumerate))
        {
          if (gooType.IsConstructedGenericType && gooType.GetGenericTypeDefinition() == typeof(GH_Enum<>))
          {
            var valueType = gooType.GetGenericArguments()[0];
            foreach (var param in assembly.ExportedTypes.Where(x => x.GetInterfaces().Contains(typeof(IGH_Param))))
            {
              if (!param.IsClass)
                continue;

              var paramType = param;
              while (paramType != typeof(GH_ActiveObject))
              {
                if (paramType.IsConstructedGenericType && paramType.GetGenericTypeDefinition() == typeof(Parameters.Param_Enum<>))
                {
                  if (paramType.GetGenericArguments()[0] == type)
                  {
                    result.Add(valueType, Tuple.Create(param, type));
                    typeFound = true;
                    break;
                  }
                }

                paramType = paramType.BaseType;
              }

              if (typeFound)
                break;
            }

            if (!typeFound)
            {
              result.Add(valueType, Tuple.Create(typeof(Parameters.Param_Enum<>).MakeGenericType(type), type));
              typeFound = true;
            }
          }

          if (typeFound)
            break;

          gooType = gooType.BaseType;
        }
      }

      // Register all the ParamsTypes as params in Grasshopper
      foreach(var entry in result)
      {
        if (entry.Value.Item1.IsGenericType)
        {
          var proxy = Activator.CreateInstance(entry.Value.Item1) as IGH_ObjectProxy;
          if (!Grasshopper.Instances.ComponentServer.IsObjectCached(proxy.Guid))
            Grasshopper.Instances.ComponentServer.AddProxy(proxy);
        }
      }
      
      return result;
    }

    static readonly Dictionary<Type, Tuple<Type, Type>> EnumTypes = LookForEnums(Assembly.GetCallingAssembly());
    public static bool TryGetParamTypes(Type type, out Tuple<Type, Type> paramTypes) =>
      EnumTypes.TryGetValue(type, out paramTypes);

    public override sealed string ToString() => $"{TypeName}: {Text}";
    public string Text
    {
      get
      {
        if (!IsValid) return "<invalid>";
        if (IsEmpty) return "<empty>";
        return Format(GetType(), this);
      }
    }

    public static ReadOnlyDictionary<int, string> GetNamedValues(Type enumType)
    {
      if (!enumType.IsSubclassOf(typeof(GH_Enumerate)))
        throw new ArgumentException($"{nameof(enumType)} must be a subclass of {typeof(GH_Enumerate).FullName}", nameof(enumType));

      var _NamedValues_ = enumType.GetProperty("NamedValues", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static, null, typeof(ReadOnlyDictionary<int, string>), new Type[0], null);
      return _NamedValues_.GetValue(null) as ReadOnlyDictionary<int, string>;
    }

    public static string Format<T>(GH_Enumerate value)
      where T : GH_Enumerate
    {
      if (value is null) return default;
      else if (value.IsEmpty) return string.Empty;
      else if (value.IsValid)
      {
        try { return GetNamedValues(typeof(T))[value.Value]; }
        catch (KeyNotFoundException) { return $"#{value}"; }
      }

      return default;
    }

    public static string Format(Type enumType, GH_Enumerate value)
    {
      if (enumType is null)
        throw new ArgumentNullException(nameof(enumType));

      if (!enumType.IsSubclassOf(typeof(GH_Enumerate)))
        throw new ArgumentException($"{nameof(enumType)} must be a subclass of {typeof(GH_Enumerate).FullName}", nameof(enumType));

      if (value is null) return default;
      else if (value.IsEmpty) return string.Empty;
      else if (value.IsValid)
      {
        try { return GetNamedValues(enumType)[value.Value]; }
        catch (KeyNotFoundException) { return $"#{value}"; }
      }

      return default;
    }

    public static GH_Enumerate Parse(Type enumType, string name)
    {
      if (enumType is null)
        throw new ArgumentNullException(nameof(enumType));

      if (!enumType.IsSubclassOf(typeof(GH_Enumerate)))
        throw new ArgumentException($"{nameof(enumType)} must be a subclass of {typeof(GH_Enumerate).FullName}", nameof(enumType));

      if (name is null)
      {
        return null;
      }
      else if (name == string.Empty)
      {
        var enumerate = Activator.CreateInstance(enumType) as GH_Enumerate;
        if(!enumerate.IsEmpty)
          throw new ArgumentException(nameof(name));

        return enumerate;
      }
      else if (name[0] == '#')
      {
        var enumerate = Activator.CreateInstance(enumType) as GH_Enumerate;
        enumerate.Value = (int) Enum.Parse(enumerate.UnderlyingEnumType, name.Substring(1));
        return enumerate;
      }
      else
      {
        var NamedValues = GetNamedValues(enumType);
        var inverse = NamedValues.ToDictionary(x => x.Value, x => x.Key);
        if (!inverse.TryGetValue(name, out var val))
          throw new ArgumentException($"{name} is not one of the named constants defined for the enumeration", nameof(name));

        var enumerate = Activator.CreateInstance(enumType) as GH_Enumerate;
        enumerate.Value = val;
        return enumerate;
      }
    }

    public static GH_Enumerate Parse(Type enumType, int value)
    {
      if (enumType is null)
        throw new ArgumentNullException(nameof(enumType));

      if (!enumType.IsSubclassOf(typeof(GH_Enumerate)))
        throw new ArgumentException($"{nameof(enumType)} must be a subclass of {typeof(GH_Enumerate).FullName}", nameof(enumType));

      var enumerate = Activator.CreateInstance(enumType) as GH_Enumerate;
      enumerate.Value = value;
      if(enumerate.IsValid)
        return enumerate;

      throw new ArgumentException(nameof(value));
    }

    public static GH_Enumerate Parse(Type enumType, double value)
    {
      if (enumType is null)
        throw new ArgumentNullException(nameof(enumType));

      if (!enumType.IsSubclassOf(typeof(GH_Enumerate)))
        throw new ArgumentException($"{nameof(enumType)} must be a subclass of {typeof(GH_Enumerate).FullName}", nameof(enumType));

      var enumerate = Activator.CreateInstance(enumType) as GH_Enumerate;
      if (double.IsNaN(value))
      {
        if (enumerate.IsEmpty)
          return enumerate;
      }
      else
      {
        try
        {
          enumerate.Value = System.Convert.ToInt32(value);
          if (enumerate.IsValid)
            return enumerate;
        }
        catch (OverflowException) { }
      }

      throw new ArgumentException(nameof(value));
    }

    public static string Format<T>(T value)
      where T : GH_Enumerate
    {
      return Format(typeof(T), value);
    }

    public static bool TryParse<T>(string name, out T value)
      where T : GH_Enumerate
    {
      try
      {
        value = (T) Parse(typeof(T), name);
        return true;
      }
      catch (ArgumentException)
      {
        value = default;
        return false;
      }
    }

    public override bool CastFrom(object source)
    {
      switch (source)
      {
        case GH_Integer integer: source = integer.Value; break;
        case GH_Number number:   source = number.Value;  break;
        case GH_String text:     source = text.Value;    break;
      }

      try
      {
        var enumerate = default(GH_Enumerate);
        switch (source)
        {
          case int intValue:        enumerate = Parse(GetType(), intValue);     break;
          case double doubleValue:  enumerate = Parse(GetType(), doubleValue);  break;
          case string stringValue:  enumerate = Parse(GetType(), stringValue);  break;
          default:
            return base.CastFrom(source);
        }

        Value = enumerate.Value;
        return true;
      }
      catch (ArgumentException)
      {
        return false;
      }
    }

    #region IComparable
    int IComparable.CompareTo(object obj)
    {
      if (obj is GH_Enumerate other)
        return Value - other.Value;

      throw new ArgumentException($"{nameof(obj)} is not a {nameof(GH_Enumerate)}", nameof(obj));
    }
    #endregion
  }

  public abstract class GH_Enum<T> : GH_Enumerate
    where T : struct, Enum
  {
    public GH_Enum() { }
    public GH_Enum(T value) => m_value = (int) (object) value;
    public new T Value { get => (T) (object) base.Value; set => base.Value = (int) (object) value; }

    public override string TypeName
    {
      get
      {
        var name = GetType().GetTypeInfo().GetCustomAttribute(typeof(RhinoInside.Revit.GH.Kernel.Attributes.NameAttribute)) as RhinoInside.Revit.GH.Kernel.Attributes.NameAttribute;
        return name?.Name ?? typeof(T).Name;
      }
    }
    public override string TypeDescription
    {
      get
      {
        var description = GetType().GetTypeInfo().GetCustomAttribute(typeof(System.ComponentModel.DescriptionAttribute)) as System.ComponentModel.DescriptionAttribute;
        return description?.Description ?? $"{typeof(T).Module.Name} {TypeName}";
      }
    }
    public override sealed Type UnderlyingEnumType => typeof(T);

    public override bool CastFrom(object source)
    {
      if (source is T value)
      {
        var enumerate = Activator.CreateInstance(GetType()) as GH_Enum<T>;
        enumerate.Value = value;
        if (!enumerate.IsValid)
          return false;

        Value = enumerate.Value;
        return true;
      }

      return base.CastFrom(source);
    }

    public override bool CastTo<Q>(ref Q target)
    {
      if (typeof(Q).IsAssignableFrom(typeof(GH_Integer)))
      {
        target = (Q) (object) new GH_Integer(base.Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Number)))
      {
        target = (Q) (object) new GH_Number(base.Value);
        return true;
      }

      if (typeof(Q) == typeof(T))
      {
        target = (Q) (object) Value;
        return true;
      }

      return base.CastTo<Q>(ref target);
    }

    public override object ScriptVariable() => Value;

    class Proxy : IGH_GooProxy
    {
      readonly GH_Enum<T> owner;
      IGH_Goo IGH_GooProxy.ProxyOwner => owner;
      bool IGH_GooProxy.IsParsable => true;
      string IGH_GooProxy.UserString { get; set; }

      public Proxy(GH_Enum<T> o) { owner = o; (this as IGH_GooProxy).UserString = FormatInstance(); }
      public void Construct() { }
      public string FormatInstance() => Enum.Format(typeof(T), owner.Value, "G");
      public bool FromString(string str) => Enum.TryParse(str, out owner.m_value);
      public string MutateString(string str) => str.Trim();
      public override string ToString() => Text;

      public bool Valid => owner.IsValid;
      public Type Type => typeof(T);
      public string Text => owner.Text;
    }

    public override IGH_GooProxy EmitProxy() => new Proxy(this);

    static ReadOnlyDictionary<int, string> namedValues;
    public static ReadOnlyDictionary<int, string> NamedValues
    {
      get
      {
        if (namedValues is null)
        {
          var names = Enum.GetNames(typeof(T));
          var values = Enum.GetValues(typeof(T)) as int[];
          var dictionary = new Dictionary<int, string>(values.Length);

          int index = 0;
          foreach (var value in values)
          {
            try { dictionary.Add(value, names[index++]); }
            catch (ArgumentException) { /* ignore duplicates */ }
          }

          namedValues = new ReadOnlyDictionary<int, string>(dictionary);
        }

        return namedValues;
      }
    }
  }
}

namespace RhinoInside.Revit.GH.Parameters
{
  using Grasshopper.Kernel.Extensions;
  using Kernel.Attributes;

  public class Param_Enum<T> : Grasshopper.Kernel.GH_PersistentParam<T>, IGH_ObjectProxy
    where T : Types.GH_Enumerate
  {
    protected Param_Enum(string name, string abbreviation, string description, string category, string subcategory) :
      base(name, abbreviation, description, category, subcategory)
    { }

    static readonly Guid GenericDataParamComponentGuid = new Guid("{8EC86459-BF01-4409-BAEE-174D0D2B13D0}");
    protected override Bitmap Icon => (Bitmap) Properties.Resources.ResourceManager.GetObject(typeof(T).Name) ??                    // try type name first
                                      (Bitmap) Properties.Resources.ResourceManager.GetObject(typeof(T).Name + "_ValueList") ??     // try with _ValueList e.g. WallFunction_ValueList
                                      Grasshopper.Instances.ComponentServer.EmitObjectIcon(GenericDataParamComponentGuid);          // default to GH icon

    public Param_Enum() :
    base
    (
      typeof(T).Name,
      typeof(T).Name,
      string.Empty,
      string.Empty,
      string.Empty
    )
    {
      ProxyExposure = Exposure;

      if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(NameAttribute)) is NameAttribute name)
        Name = name.Name;

      if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(NickNameAttribute)) is NickNameAttribute nickname)
        NickName = nickname.NickName;

      if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute description)
        Description = description.Description;

      if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(CategoryAttribute)) is CategoryAttribute category)
      {
        Category = category.Category;
        SubCategory = category.SubCategory;
      }
    }

    public override Guid ComponentGuid
    {
      get
      {
        if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(ComponentGuidAttribute)) is ComponentGuidAttribute componentGuid)
          return componentGuid.Value;

        throw new NotImplementedException($"{typeof(T).FullName} has no {nameof(ComponentGuid)}, please use {typeof(ComponentGuidAttribute).FullName}");
      }
    }

    public override GH_Exposure Exposure
    {
      get
      {
        if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(ExposureAttribute)) is ExposureAttribute exposure)
          return exposure.Value;

        return GH_Exposure.hidden;
      }
    }
    protected override void Menu_AppendPromptOne(ToolStripDropDown menu) { }
    protected override void Menu_AppendPromptMore(ToolStripDropDown menu) { }

    protected override GH_GetterResult Prompt_Plural(ref List<T> values) => GH_GetterResult.cancel;
    protected override GH_GetterResult Prompt_Singular(ref T value) => GH_GetterResult.cancel;

    public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
    {
      if (Kind > GH_ParamKind.input || DataType == GH_ParamData.remote)
      {
        base.AppendAdditionalMenuItems(menu);
        return;
      }

      Menu_AppendWireDisplay(menu);
      Menu_AppendDisconnectWires(menu);

      Menu_AppendPrincipalParameter(menu);
      Menu_AppendReverseParameter(menu);
      Menu_AppendFlattenParameter(menu);
      Menu_AppendGraftParameter(menu);
      Menu_AppendSimplifyParameter(menu);

      var current = InstantiateT();
      if (SourceCount == 0 && PersistentDataCount == 1)
      {
        if(PersistentData.get_FirstItem(true) is T firstValue)
          current.Value = firstValue.Value;
      }

      var values = Types.GH_Enumerate.GetNamedValues(typeof(T));
      if (values.Count < 7)
      {
        Menu_AppendSeparator(menu);
        foreach (var e in values)
        {
          var tag = InstantiateT(); tag.Value = e.Key;
          var item = Menu_AppendItem(menu, tag.Text, Menu_NamedValueClicked, SourceCount == 0, e.Key == current.Value);
          item.Tag = tag;
        }
        Menu_AppendSeparator(menu);
      }
      else
      {
        var listBox = new ListBox();
        foreach (var e in values)
        {
          var tag = InstantiateT(); tag.Value = e.Key;
          int index = listBox.Items.Add(tag);
          if (e.Key == current.Value)
            listBox.SelectedIndex = index;
        }

        listBox.DisplayMember = "Text";
        listBox.BorderStyle = BorderStyle.FixedSingle;

        listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

        listBox.Width = (int) (200 * GH_GraphicsUtil.UiScale);
        listBox.Height = (int) (100 * GH_GraphicsUtil.UiScale);
        Menu_AppendCustomItem(menu, listBox);
      }

      Menu_AppendDestroyPersistent(menu);
      Menu_AppendInternaliseData(menu);

      if(Exposure != GH_Exposure.hidden)
        Menu_AppendExtractParameter(menu);

      Menu_AppendItem(menu, "Expose picker", Menu_ExposePicker, SourceCount == 0);
    }

    private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (sender is ListBox listBox)
      {
        if (listBox.SelectedIndex != -1)
        {
          if (listBox.Items[listBox.SelectedIndex] is T value)
          {
            RecordUndoEvent($"Set: {value}");
            PersistentData.Clear();
            PersistentData.Append(value.Duplicate() as T);
          }
        }

        ExpireSolution(true);
      }
    }

    private void Menu_NamedValueClicked(object sender, EventArgs e)
    {
      if (sender is ToolStripMenuItem item)
      {
        if (item.Tag is T value)
        {
          RecordUndoEvent($"Set: {value}");
          PersistentData.Clear();
          PersistentData.Append(value.Duplicate() as T);

          ExpireSolution(true);
        }
      }
    }

    protected void Menu_ExposePicker(object sender, EventArgs e)
    {
      if (sender is ToolStripMenuItem)
      {
        var list = new Grasshopper.Kernel.Special.GH_ValueList();
        if (list is null)
          return;

        list.Category = string.Empty;
        list.SubCategory = string.Empty;

        if (typeof(T).GetTypeInfo().GetCustomAttribute(typeof(NameAttribute)) is NameAttribute name)
          list.Name = name.Name;
        else
          list.Name = typeof(T).Name;

        list.NickName = string.Empty;
        list.Description = $"A {TypeName} picker";

        list.ListItems.Clear();

        foreach (var value in Types.GH_Enumerate.GetNamedValues(typeof(T)))
          list.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(value.Value, value.Key.ToString()));

        if(this.ConnectNewObject(list))
          list.ExpireSolution(true);
      }
    }

    #region IGH_ObjectProxy
    string IGH_ObjectProxy.Location => GetType().Assembly.Location;
    Guid IGH_ObjectProxy.LibraryGuid => Guid.Empty;
    bool IGH_ObjectProxy.SDKCompliant => SDKCompliancy(Rhino.RhinoApp.ExeVersion, Rhino.RhinoApp.ExeServiceRelease);
    bool IGH_ObjectProxy.Obsolete => Obsolete;
    Type IGH_ObjectProxy.Type => GetType();
    GH_ObjectType IGH_ObjectProxy.Kind => GH_ObjectType.CompiledObject;
    Guid IGH_ObjectProxy.Guid => ComponentGuid;
    Bitmap IGH_ObjectProxy.Icon => Icon;
    IGH_InstanceDescription IGH_ObjectProxy.Desc => this;

    GH_Exposure ProxyExposure;
    GH_Exposure IGH_ObjectProxy.Exposure { get => ProxyExposure; set => ProxyExposure = value; }

    IGH_DocumentObject IGH_ObjectProxy.CreateInstance() => new Param_Enum<T>();
    IGH_ObjectProxy IGH_ObjectProxy.DuplicateProxy() => (IGH_ObjectProxy) MemberwiseClone();
    #endregion
  }
}
