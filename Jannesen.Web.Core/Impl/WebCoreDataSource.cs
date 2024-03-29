﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreDataSource
    {
        private readonly            string                  _name;

        public                      string                  Name
        {
            get {
                return _name;
            }
        }

        protected                                           WebCoreDataSource(string name)
        {
            _name = name;
        }

        public      abstract        WebCoreDataValue        GetValue(WebCoreCall call);
    }
}
