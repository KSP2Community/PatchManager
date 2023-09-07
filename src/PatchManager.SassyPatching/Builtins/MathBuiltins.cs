using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// A builtin math library to make dealing with stuff easier
/// </summary>
[SassyLibrary("builtin","math"),PublicAPI]
public class MathBuiltins
{
    [SassyMethod("square-root")]
    public static double SquareRoot(double value) => Math.Sqrt(value);

    [SassyMethod("cube-root")]
    public static double CubeRoot(double value) => Math.Pow(value, 0.333333333333333333333333333333333333333333333333d);

    [SassyMethod("pow")]
    public static double Pow(double x, double y) => Math.Pow(x, y);

    [SassyConstant("PI")] public static double Pi => Math.PI;
    [SassyConstant("E")] public static double E => Math.E;
}