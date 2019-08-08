using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Jannesen.Web.MSSql
{
    [Serializable]
    public class NoDataException: Exception
    {
        public                              NoDataException(string message): base(message)
        {
        }
        protected                           NoDataException(SerializationInfo info,  StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.MSSql";
            }
        }
    }
}
