using System;

namespace Jannesen.Web.Core;

public abstract class WebCoreDataSource: IWebCoreDataSource
{
    private readonly            string                  _name;

    public                      string                  Name        => _name;

    protected                                           WebCoreDataSource(string name)
    {
        _name = name;
    }

    public      abstract        WebCoreDataValue        GetValue(WebCoreCall call);
}
