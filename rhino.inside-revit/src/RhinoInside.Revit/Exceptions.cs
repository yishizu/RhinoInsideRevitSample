using System;

namespace RhinoInside.Revit.Exceptions
{
  /// <summary>
  /// The exception that is thrown when a non-fatal error occurs.
  /// The current operation failed, if you catch this exception changes must be undone
  /// </summary>
  public class FailException : Exception
  {
    public FailException() : base(string.Empty) { }
    public FailException(string message) : base(message) { }
    public FailException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  /// <summary>
  /// The exception that is thrown when a non-fatal warning occurs.
  /// The current operation was cancelled but what is already done is valid.
  /// </summary>
  public class CancelException : Exception
  {
    public CancelException() : base (string.Empty) { }
    public CancelException(string message) : base(message) { }
    public CancelException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}
