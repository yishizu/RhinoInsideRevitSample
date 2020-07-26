using System;
using Rhino;
using Rhino.Geometry;
using DB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.Convert.Geometry
{
  using global::System.Diagnostics;

  public static class UnitConverter
  {
    #region Scaling factors
    /// <summary>
    /// Factor to do a direct conversion without any unit scaling.
    /// </summary>
    public const double NoScale = 1.0;

    /// <summary>
    /// Factor for converting a value from Revit internal units to Rhino model units.
    /// </summary>
    public static double ToRhinoUnits => RhinoMath.UnitScale(UnitSystem.Feet, RhinoDoc.ActiveDoc?.ModelUnitSystem ?? UnitSystem.Meters);

    /// <summary>
    /// Factor for converting a value from Rhino model units to Revit internal units.
    /// </summary>
    public static double ToHostUnits => RhinoMath.UnitScale(RhinoDoc.ActiveDoc?.ModelUnitSystem ?? UnitSystem.Meters, UnitSystem.Feet);
    #endregion

    #region Scale
    public static void Scale(ref Point2f value, double factor)
    {
      value.X *= (float) factor;
      value.Y *= (float) factor;
    }
    public static void Scale(ref Point2d value, double factor)
    {
      value.X *= factor;
      value.Y *= factor;
    }
    public static void Scale(ref Vector2d value, double factor)
    {
      value.X *= factor;
      value.Y *= factor;
    }
    public static void Scale(ref Vector2f value, double factor)
    {
      value.X *= (float) factor;
      value.Y *= (float) factor;
    }

    public static void Scale(ref Point3f value, double factor)
    {
      value.X *= (float) factor;
      value.Y *= (float) factor;
      value.Z *= (float) factor;
    }
    public static void Scale(ref Point3d value, double factor)
    {
      value.X *= factor;
      value.Y *= factor;
      value.Z *= factor;
    }
    public static void Scale(ref Vector3d value, double factor)
    {
      value.X *= factor;
      value.Y *= factor;
      value.Z *= factor;
    }
    public static void Scale(ref Vector3f value, double factor)
    {
      value.X *= (float) factor;
      value.Y *= (float) factor;
      value.Z *= (float) factor;
    }

    public static void Scale(ref Transform value, double scaleFactor)
    {
      value.M03 *= scaleFactor;
      value.M13 *= scaleFactor;
      value.M23 *= scaleFactor;
    }

    public static void Scale(ref BoundingBox value, double scaleFactor)
    {
      value.Min *= scaleFactor;
      value.Max *= scaleFactor;
    }

    public static void Scale(ref Plane value, double scaleFactor)
    {
      value.Origin *= scaleFactor;
    }

    public static void Scale(ref Line value, double scaleFactor)
    {
      value.From *= scaleFactor;
      value.To   *= scaleFactor;
    }

    public static void Scale(ref Arc value, double scaleFactor)
    {
      var plane = value.Plane;
      plane.Origin *= scaleFactor;
      value.Plane = plane;
      value.Radius *= scaleFactor;
    }

    public static void Scale(ref Circle value, double scaleFactor)
    {
      var plane = value.Plane;
      plane.Origin *= scaleFactor;
      value.Plane = plane;
      value.Radius *= scaleFactor;
    }

    public static void Scale(ref Ellipse value, double scaleFactor)
    {
      var plane = value.Plane;
      plane.Origin *= scaleFactor;
      value.Plane = plane;
      value.Radius1 *= scaleFactor;
      value.Radius2 *= scaleFactor;
    }

    /// <summary>
    /// Scales <paramref name="value"/> instance by <paramref name="factor"/> in place.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="factor"></param>
    /// <seealso cref="InOtherUnits{G}(G, double)"/>
    public static void Scale<G>(G value, double factor) where G : GeometryBase
    {
      if (factor != 1.0 && value?.Scale(factor) == false)
        throw new InvalidOperationException($"Failed to Change {value} basis");
    }
    #endregion

    #region InOtherUnits
    public static Point3f InOtherUnits(this Point3f value, double factor)
    { Scale(ref value, factor); return value; }

    public static Point3d InOtherUnits(this Point3d value, double factor)
    { Scale(ref value, factor); return value; }

    public static Vector3d InOtherUnits(this Vector3d value, double factor)
    { Scale(ref value, factor); return value; }

    public static Vector3f InOtherUnits(this Vector3f value, double factor)
    { Scale(ref value, factor); return value; }

    public static Transform InOtherUnits(this Transform value, double factor)
    { Scale(ref value, factor); return value; }

    public static BoundingBox InOtherUnits(this BoundingBox value, double factor)
    { Scale(ref value, factor); return value; }

    public static Plane InOtherUnits(this Plane value, double factor)
    { Scale(ref value, factor); return value; }

    public static Line InOtherUnits(this Line value, double factor)
    { Scale(ref value, factor); return value; }

    public static Arc InOtherUnits(this Arc value, double factor)
    { Scale(ref value, factor); return value; }

    public static Circle InOtherUnits(this Circle value, double factor)
    { Scale(ref value, factor); return value; }

    public static Ellipse InOtherUnits(this Ellipse value, double factor)
    { Scale(ref value, factor); return value; }

    /// <summary>
    /// Duplicates and scales <paramref name="value"/> to be stored in other units.
    /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="factor"></param>
    /// <returns>Returns a scaled duplicate of the input <paramref name="value"/> in other units.</returns>
    public static G InOtherUnits<G>(this G value, double factor) where G : GeometryBase
    { value = (G) value.DuplicateShallow(); if(factor != 1.0) Scale(value, factor); return value; }

    static double InOtherUnits(double value, DB.ParameterType type, UnitSystem from, UnitSystem to)
    {
      switch (type)
      {
        #region Length

        case DB.ParameterType.Length:
        case DB.ParameterType.ForceLengthPerAngle:
        case DB.ParameterType.LinearForceLengthPerAngle:
        case DB.ParameterType.ReinforcementLength:

        case DB.ParameterType.AreaForcePerLength:
        case DB.ParameterType.ReinforcementAreaPerUnitLength:

          return value * RhinoMath.UnitScale(from, to);

        case DB.ParameterType.ForcePerLength:
        case DB.ParameterType.LinearForcePerLength:
        case DB.ParameterType.MassPerUnitLength:
        case DB.ParameterType.WeightPerUnitLength:
        case DB.ParameterType.PipeMassPerUnitLength:

          return value / RhinoMath.UnitScale(from, to);

        #endregion

        #region Area

        case DB.ParameterType.Area:
        case DB.ParameterType.AreaForce:
        case DB.ParameterType.HVACAreaDividedByCoolingLoad:
        case DB.ParameterType.HVACAreaDividedByHeatingLoad:
        case DB.ParameterType.SurfaceArea:
        case DB.ParameterType.ReinforcementArea:
        case DB.ParameterType.SectionArea:

          return value * Math.Pow(RhinoMath.UnitScale(from, to), 2.0);

        case DB.ParameterType.HVACCoolingLoadDividedByArea:
        case DB.ParameterType.HVACHeatingLoadDividedByArea:
        case DB.ParameterType.MassPerUnitArea:

          return value / Math.Pow(RhinoMath.UnitScale(from, to), 2.0);

        #endregion

        #region Volume

        case DB.ParameterType.Volume:
        case DB.ParameterType.PipingVolume:
        case DB.ParameterType.ReinforcementVolume:

          return value * Math.Pow(RhinoMath.UnitScale(from, to), 3.0);

        case DB.ParameterType.HVACCoolingLoadDividedByVolume:
        case DB.ParameterType.HVACHeatingLoadDividedByVolume:
        case DB.ParameterType.HVACAirflowDividedByVolume:

          return value * Math.Pow(RhinoMath.UnitScale(from, to), 3.0);

        #endregion

        default:
          Debug.WriteLine($"{nameof(InOtherUnits)} do not implement conversion for {type}");
          break;
      }

      return value;
    }
    #endregion

    #region InRhinoUnits
    public static Point3f InRhinoUnits(this Point3f value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Point3d InRhinoUnits(this Point3d value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Vector3d InRhinoUnits(this Vector3d value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Vector3f InRhinoUnits(this Vector3f value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Transform InRhinoUnits(this Transform value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static BoundingBox InRhinoUnits(this BoundingBox value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Plane InRhinoUnits(this Plane value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Line InRhinoUnits(this Line value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Arc InRhinoUnits(this Arc value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Circle InRhinoUnits(this Circle value)
    { Scale(ref value, ToRhinoUnits); return value; }

    public static Ellipse InRhinoUnits(this Ellipse value)
    { Scale(ref value, ToRhinoUnits); return value; }

    /// <summary>
    /// Duplicates and scales <paramref name="value"/> to be stored in Acitve Rhino document units.
    /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Returns a scaled duplicate of the input <paramref name="value"/> in Active Rhino document units.</returns>
    public static G InRhinoUnits<G>(this G value) where G : GeometryBase
    { Scale(value = (G) value.DuplicateShallow(), ToRhinoUnits); return value; }

    public static double InRhinoUnits(double value, DB.ParameterType type) =>
      InRhinoUnits(value, type, RhinoDoc.ActiveDoc);
    static double InRhinoUnits(double value, DB.ParameterType type, RhinoDoc rhinoDoc)
    {
      if (rhinoDoc is null)
        return double.NaN;

      return InOtherUnits(value, type, UnitSystem.Feet, rhinoDoc.ModelUnitSystem);
    }
    #endregion

    #region InHostUnits
    public static Point3f InHostUnits(this Point3f value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Point3d InHostUnits(this Point3d value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Vector3d InHostUnits(this Vector3d value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Vector3f InHostUnits(this Vector3f value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Transform InHostUnits(this Transform value)
    { Scale(ref value, ToHostUnits); return value; }

    public static BoundingBox InHostUnits(this BoundingBox value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Plane InHostUnits(this Plane value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Line InHostUnits(this Line value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Arc InHostUnits(this Arc value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Circle InHostUnits(this Circle value)
    { Scale(ref value, ToHostUnits); return value; }

    public static Ellipse InHostUnits(this Ellipse value)
    { Scale(ref value, ToHostUnits); return value; }

    /// <summary>
    /// Duplicates and scales <paramref name="value"/> to be stored Revit internal units.
    /// <para>See <see cref="Scale{G}(G, double)"/> for in place scaling.</para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Returns a duplicate of <paramref name="value"/> in Revit internal units.</returns>
    public static G InHostUnits<G>(this G value) where G : GeometryBase
    { Scale(value = (G) value.DuplicateShallow(), ToHostUnits); return value; }

    public static double InHostUnits(double value, DB.ParameterType type) =>
      InHostUnits(value, type, RhinoDoc.ActiveDoc);
    static double InHostUnits(double value, DB.ParameterType type, RhinoDoc rhinoDoc)
    {
      if (rhinoDoc is null)
        return double.NaN;

      return InOtherUnits(value, type, rhinoDoc.ModelUnitSystem, UnitSystem.Feet);
    }
    #endregion
  }
}
