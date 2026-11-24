using System;
using System.Linq;
using System.Collections.Generic;

namespace Jannesen.Web.Core;

public static class WebCoreLibrary
{
    public  static      IEnumerable<IWebCoreProcessor<object>>          JoinDatasources(params IWebCoreDataSource[] datasources)
    {
        return JoinDatasourcesArray(datasources);
    }
    public  static      IEnumerable<IWebCoreProcessor<object>>          JoinDatasourcesArray(IWebCoreDataSource[] datasources)
    {
        ArgumentNullException.ThrowIfNull(datasources);

        var rtn = new List<IWebCoreProcessor<object>>();

        foreach(var datasource in datasources) {
            foreach(var p in datasource.GetRequiredProcessors()) {
                if (!rtn.Any(x => object.ReferenceEquals(x, p))) {
                    rtn.Add(p);
                }
            }
        }

        return rtn;
    }
}
