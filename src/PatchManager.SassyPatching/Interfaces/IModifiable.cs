namespace PatchManager.SassyPatching.Interfaces;

/// <summary>
/// Represents a modifiable value
/// </summary>
public interface IModifiable
{
    /// <summary>
    /// Gets the value of a field
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <returns>The value of the field</returns>
    public DataValue GetFieldValue(string fieldName);
    /// <summary>
    /// Sets the value of a field
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="dataValue">The value to set it to</param>
    public void SetFieldValue(string fieldName, DataValue dataValue);

    /// <summary>
    /// Set this object to another value
    /// </summary>
    /// <param name="dataValue">The value to set it to</param>
    public void Set(DataValue dataValue);

    /// <summary>
    /// Get the value of this object
    /// </summary>
    /// <returns>The value of the object</returns>
    public DataValue Get();
    
    /// <summary>
    /// Finalize modification of this object, and close its modification
    /// </summary>
    public void Complete();
}