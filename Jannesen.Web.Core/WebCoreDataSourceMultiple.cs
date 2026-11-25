using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jannesen.Web.Core;

internal sealed class WebCoreDataSourceMultiple(IWebCoreDataSource[] list): IWebCoreDataSource
{
    public                      IEnumerable<IWebCoreProcessor>      GetRequiredProcessors()
    {
        return WebCoreLibrary.JoinDatasourcesArray(list);
    }
    public                      WebCoreDataValue                    GetValue(WebCoreCall httpCall)
    {
        var rtn = WebCoreDataValue.NoValue;

        for (var i = 0 ; i < list.Length ; ++i) {
            rtn = list[i].GetValue(httpCall);

            if (rtn.Type != WebCoreDataValueType.NoValue)
                break;
        }

        return rtn;
    }

    public      override        string                              ToString()
    {
        var rtn = new StringBuilder();

        foreach(var i in list) {
            if (rtn.Length > 0) rtn.Append('|');
            rtn.Append(i.ToString());
        }

        return rtn.ToString();
    }
}
