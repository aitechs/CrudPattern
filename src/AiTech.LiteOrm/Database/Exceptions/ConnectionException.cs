using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AiTech.LiteOrm.Database.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ConnectionException : SystemException
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        public ConnectionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
