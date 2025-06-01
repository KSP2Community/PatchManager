namespace Shoemaker.Overrides
{
    public interface IOverride<in T> where T : class
    {
        public void ApplyTo(T obj);
    }
}