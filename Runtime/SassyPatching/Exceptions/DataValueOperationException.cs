using System;

namespace PatchManager.SassyPatching.Exceptions
{
    public class DataValueOperationException : Exception
    {
        public DataValueOperationException(string message) : base(message)
        {
        }
    }
}