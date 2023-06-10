namespace PatchManager.SassyPatching.Interfaces;

/// <summary>
/// Represents a modifiable value
/// </summary>
public interface IModifiable
{
    /// <summary>
    /// Gets a a field indexed by number
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="index">the index</param>
    /// <returns>The value of the field</returns>
    public Value GetFieldByNumber(string fieldName, ulong index);
    /// <summary>
    /// Gets a field indexed by element type
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="elementName">The element type</param>
    /// <returns>The value of the field</returns>
    public Value GetFieldByElement(string fieldName, string elementName);
    
    /// <summary>
    /// Gets a field indexed by class
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="className">The class</param>
    /// <returns>The value of the field</returns>
    public Value GetFieldByClass(string fieldName, string className);
    
    /// <summary>
    /// Sets a a field indexed by number
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="index">the index</param>
    /// <param name="value">The value of the field</param>
    public void SetFieldByNumber(string fieldName, ulong index, Value value);
    /// <summary>
    /// Sets a field indexed by element type
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="elementName">The element type</param>
    /// <param name="value">The value of the field</param>
    public void SetFieldByElement(string fieldName, string elementName, Value value);
    
    /// <summary>
    /// Sets a field indexed by class
    /// </summary>
    /// <param name="fieldName">the name of the field</param>
    /// <param name="className">The class</param>
    /// <param name="value">The value of the field</param>
    public void SetFieldByClass(string fieldName, string className, Value value);
    /// <summary>
    /// Gets the value of a field
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <returns>The value of the field</returns>
    public Value GetFieldValue(string fieldName);
    /// <summary>
    /// Sets the value of a field
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="value">The value to set it to</param>
    public void SetFieldValue(string fieldName, Value value);

    /// <summary>
    /// Set this object to another value
    /// </summary>
    /// <param name="value">The value to set it to</param>
    public void Set(Value value);

    /// <summary>
    /// Get the value of this object
    /// </summary>
    /// <returns>The value of the object</returns>
    public Value Get();
    
    /// <summary>
    /// Finalize modification of this object, and close its modification
    /// </summary>
    public void Complete();
}