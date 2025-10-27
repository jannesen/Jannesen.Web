using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library
{
    public abstract class Parameter
    {
        public  readonly            string           Name;

        protected                                    Parameter(WebCoreConfigReader configReader)
        {
            Name = configReader.GetValueString("name");
        }
        public  abstract            void             AddToCommand(SqlCommand sqlCommand, WebCoreCall httpCall);
    }

    public class ParameterList: List<Parameter>
    {
        public                  void                AddParametersToCommand(SqlCommand sqlCommand, WebCoreCall httpCall)
        {
            foreach(Parameter parameter in this)
                parameter.AddToCommand(sqlCommand, httpCall);
        }
    }
}
