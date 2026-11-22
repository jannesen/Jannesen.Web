using System;

namespace Jannesen.Web.Core;

public interface IWebCoreProcessorGetDatasource
{
    IWebCoreDataSource      GetDataSource(string name);
}
