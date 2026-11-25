using System;
using System.Linq;
using System.Collections.Generic;

namespace Jannesen.Web.Core;

public static class WebCoreLibrary
{
    public  static      IEnumerable<IWebCoreProcessor>          JoinDatasources(params IWebCoreDataSource[] datasources)
    {
        return JoinDatasourcesArray(datasources);
    }
    public  static      IEnumerable<IWebCoreProcessor>          JoinDatasourcesArray(IWebCoreDataSource[] datasources)
    {
        ArgumentNullException.ThrowIfNull(datasources);

        var rtn = new List<IWebCoreProcessor>();

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
