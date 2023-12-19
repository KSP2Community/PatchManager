using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// A builtin math library to make dealing with stuff easier
/// </summary>
[SassyLibrary("builtin", "math"), PublicAPI]
public class MathBuiltins
{
    #region Basic Math
    
    [SassyMethod("square-root")]
    public static double SquareRoot(double value) => Math.Sqrt(value);

    [SassyMethod("cube-root")]
    public static double CubeRoot(double value) => Math.Pow(value, 0.333333333333333333333333333333333333333333333333d);

    [SassyMethod("pow")]
    public static double Pow(double x, double y) => Math.Pow(x, y);

    [SassyMethod("ln")]
    public static double Ln(double a) => Math.Log(a);

    [SassyMethod("log")]
    public static double Log(double a, double @base) => Math.Log(a, @base);

    [SassyMethod("log10")]
    public static double Log10(double a) => Math.Log10(a);

    [SassyMethod("ceiling")]
    public static double Ceiling(double a) => Math.Ceiling(a);

    [SassyMethod("floor")]
    public static double Floor(double a) => Math.Floor(a);

    [SassyMethod("abs")]
    public static double Abs(double a) => Math.Abs(a);

    [SassyMethod("max")]
    public static double Max([VarArgs] List<DataValue> values) => values.Select(x => x.To<double>()).Max();

    [SassyMethod("min")]
    public static double Min([VarArgs] List<DataValue> values) => values.Select(x => x.To<double>()).Min();

    [SassyMethod("list.max")]
    public static double Max(List<double> list) => list.Max();

    [SassyMethod("list.min")]
    public static double Min(List<double> list) => list.Min();

    [SassyMethod("round")]
    public static double Round(double x) => Math.Round(x, MidpointRounding.AwayFromZero);

    [SassyMethod("sign")]
    public static double Sign(double x) => Math.Sign(x);

    [SassyConstant("E")] public static double E => Math.E;

    #endregion

    #region Trigonometry

    [SassyMethod("sin")]
    public static double Sin(double a) => Math.Sin(a);

    [SassyMethod("cos")]
    public static double Cos(double a) => Math.Cos(a);

    [SassyMethod("tan")]
    public static double Tan(double a) => Math.Tan(a);

    [SassyMethod("sinh")]
    public static double Sinh(double a) => Math.Sinh(a);

    [SassyMethod("cosh")]
    public static double Cosh(double a) => Math.Cosh(a);

    [SassyMethod("tanh")]
    public static double Tanh(double a) => Math.Tanh(a);

    [SassyMethod("asin")]
    public static double Asin(double a) => Math.Asin(a);

    [SassyMethod("acos")]
    public static double Acos(double a) => Math.Acos(a);

    [SassyMethod("atan")]
    public static double Atan(double a) => Math.Atan(a);

    [SassyMethod("atan2")]
    public static double Atan2(double y, double x) => Math.Atan2(y, x);

    [SassyConstant("PI")] public static double Pi => Math.PI;
    [SassyConstant("TAU")] public static double Tau => 2 * Pi;

    #endregion

    #region Other Math

    [SassyMethod("interpolate")]
    public static double Lerp(double a, double b, double t) => (1 - t) * a + t * b;

    [SassyMethod("list.normalize")]
    public static List<double> Normalize(List<double> vector)
    {
        var magnitude = Math.Sqrt(vector.Select(x => x * x).Sum());
        return vector.Select(x => x / magnitude).ToList();
    }

    [SassyMethod("normalize")]
    public static List<double> Normalize([VarArgs] List<DataValue> args)
    {
        var list = (from x in args select x.To<double>()).ToList();
        return Normalize(list);
    }


    #endregion
}