using System;

namespace NetLadioWatcher
{
    public class NetLadioUpdateFailedException : Exception
    {
        internal NetLadioUpdateFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
