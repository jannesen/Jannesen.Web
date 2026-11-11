using System;

namespace Jannesen.Web.Core;

public interface IFileLocation
{
    string          Filename        { get; }
    int             LineNumber      { get; }
}
