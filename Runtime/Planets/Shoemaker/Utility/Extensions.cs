namespace Shoemaker.Utility
{
    public static class Extensions
    {
        public static void Apply<T>(this T? optional, ref T value) where T : struct
        {
            if (optional.HasValue) value = optional.Value;
        }
    }
}