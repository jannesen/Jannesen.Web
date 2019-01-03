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

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.MSSql";
            }
        }
    }
}
