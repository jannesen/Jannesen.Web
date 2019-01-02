using System;
using System.Web;

namespace Jannesen.Web.Core
{
    public class ApplModule: IHttpModule
    {
        public      static              object      _lock = new object();
        public      static              int         _usage;

        public                          void        Dispose()
        {
            int     usage;

            lock(_lock)
            {
                usage = --_usage;
            }

            if (usage == 0)
                WebApplication.AppDomainUnload();
        }
        public                          void        Init(HttpApplication application)
        {
            int     usage;

            lock(_lock)
            {
                usage = _usage++;
            }

            if (usage==0)
                WebApplication.AppDomainInitialize();
        }
    }
}
