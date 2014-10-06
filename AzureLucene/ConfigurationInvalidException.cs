using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Mindscape.Lightspeed.Extensions
{
    public class ConfigurationValueEmptyException : ArgumentNullException
    {
        public ConfigurationValueEmptyException(){}
        
        public ConfigurationValueEmptyException(string message)
            : base(message){}

        public ConfigurationValueEmptyException(string message, Exception inner)
            : base(message, inner){}

        public ConfigurationValueEmptyException(String format, params object[] args)
            : base(String.Format(format, args)) { }

        protected ConfigurationValueEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context){}
    }



    public class ConfigurationInvalidException : ArgumentException
    {
        public ConfigurationInvalidException(){}

        public ConfigurationInvalidException(string message)
            : base(message){}

        public ConfigurationInvalidException(string message, Exception inner)
            : base(message, inner){}

        public ConfigurationInvalidException(String format, params object[] args) 
            : base(String.Format(format, args)) { }

        protected ConfigurationInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context){}
    }
}
