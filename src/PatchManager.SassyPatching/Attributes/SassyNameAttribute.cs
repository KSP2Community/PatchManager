namespace PatchManager.SassyPatching.Attributes;

[AttributeUsage(AttributeTargets.Parameter,AllowMultiple = false)]
public class SassyNameAttribute : Attribute
{
   public string ArgumentName;
   public SassyNameAttribute(string argumentName)
   {
      ArgumentName = argumentName;
   }
}