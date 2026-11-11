using System;

namespace Jannesen.Web.Core.Impl;

public interface IFileLocation
{
    string          Filename        { get; }
    int             LineNumber      { get; }
}
