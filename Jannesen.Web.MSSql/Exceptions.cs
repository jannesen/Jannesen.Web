using System;

namespace Jannesen.Web.MSSql
{
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
