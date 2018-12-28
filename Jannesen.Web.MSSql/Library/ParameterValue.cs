using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using Jannesen.Web.Core;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.MSSql.Library
{
    public class ParameterValue: Parameter
    {
        private readonly        ValueConvertor      _type;
        private readonly        WebCoreDataSource   _source;
        private readonly        bool                _optional;

        public                                      ParameterValue(string type, WebCoreConfigReader configReader): base(configReader)
        {
            string  source   = configReader.GetValueString  ("source");
            _optional        = configReader.GetValueBool    ("optional", false);

            try {
                _type = ValueConvertor.GetType(type);
            }
            catch(Exception err) {
                throw new WebConfigException("Invalid type '" + type + "'.", err, configReader);
            }

            try {
                _source = WebApplication.GetDataSource(source, Name);
            }
            catch(Exception err) {
                throw new WebConfigException("Invalid source '" + source + "'.", err, configReader);
            }

            configReader.NoChildElements();
        }

        public  override        void                AddToCommand(SqlCommand sqlCommand, WebCoreCall httpCall)
        {
            WebCoreDataValue    value = _source.GetValue(httpCall);

            switch (value.Type)
            {
            case WebCoreDataValueType.NoValue:
                if (!_optional)
                    throw new WebRequestException("Missing parameter '" + _source.ToString() + "' in request.");
                break;

            case WebCoreDataValueType.ClrValue:
            case WebCoreDataValueType.StringValue:
                try {
                    sqlCommand.Parameters.Add(Name, _type.DBType).Value = _type.ConvertToSqlParameterValue(value) ?? DBNull.Value;
                }
                catch(Exception err) {
                    throw new WebRequestException("Parameter '" + Name + "' is incorrect.", err);
                }
                break;
            }
        }

    }
}
