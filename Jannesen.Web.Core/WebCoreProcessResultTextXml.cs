using System;
using System.Xml;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessResultTextXml(XmlDocument? document)
{
    public          XmlDocument?    Document => document;

    public          string?         GetStringValue(string xpath)
    {
        if (document == null)
            throw new WebRequestException("Empty body.");

        return document.SelectSingleNode(xpath)?.Value;
    }
}

