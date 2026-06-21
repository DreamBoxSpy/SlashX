using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Exceptions
{

    [Serializable]
    public class NuGetVersionConflictException : InvalidOperationException
    {
        public NuGetVersionConflictException() { }
        public NuGetVersionConflictException(string message) : base(message) { }
        public NuGetVersionConflictException(string message, Exception inner) : base(message, inner) { }

        [Obsolete]
        protected NuGetVersionConflictException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
