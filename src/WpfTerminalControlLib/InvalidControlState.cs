using System;
using System.Runtime.Serialization;

namespace Terminal1
{
    public class InvalidControlState : Exception
    {
        /// <inheritdoc />
        public InvalidControlState()
        {
        }

        /// <inheritdoc />
        protected InvalidControlState(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public InvalidControlState(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public InvalidControlState(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}