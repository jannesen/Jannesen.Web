using System;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessResultBasicAutorization(string? username, string? passwd)
{
    public          string?         UserName        => username;
    public          string?         Passwd          => passwd;

}
