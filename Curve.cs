using System;

namespace GenePotionSelector;

// It wraps EClass.curve.
// The first argument of EClass.curve has been changed from int to long.
// Use this wrapper to support both versions.
public class CurveWrapper
{
  public static int curve(int _a, int start, int step, int rate = 75)
  {
    var t = typeof(EClass);

    var method = t.GetMethod("curve", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) });
    if (method != null)
    {
      return method.Invoke(null, new object[] { _a, start, step, rate }) is int result ? result : 0;
    }

    method = t.GetMethod("curve", new Type[] { typeof(long), typeof(int), typeof(int), typeof(int) });
    if (method != null)
    {
      return method.Invoke(null, new object[] { (long)_a, start, step, rate }) is int result ? result : 0;
    }

    throw new MissingMethodException("EClass.curve method not found");
  }
}
