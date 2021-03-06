﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreAttribureHttpHandler: WebCoreAttribureDynamicClass
    {
        public      override    string                          Type
        {
            get {
                return "http-handler";
            }
        }

        public                                                  WebCoreAttribureHttpHandler(string name): base(name)
        {
        }

        public      override    ConstructorInfo                 GetConstructor(Type classType)
        {
            return GetConstructorFor(classType, typeof(WebCoreHttpHandler), typeof(WebCoreConfigReader));
        }
    }
}
