using System;

namespace Jannesen.Web.MSSql
{
    public class NoDataException: Exception
    {
        public                              NoDataException(string message): base(message)
        {
        }

        public  override    string          Source          => "Jannesen.Web.MSSql";
    }
}
