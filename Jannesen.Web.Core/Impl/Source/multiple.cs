using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    class multiple: WebCoreDataSource
    {
        public      const           char                SplitChar = '|';

        private                     WebCoreDataSource[] _list;

        public                                          multiple(string source, string name_args): base(source)
        {
            string[]    sources = source.Split(SplitChar);

            _list = new WebCoreDataSource[sources.Length];

            for (int i = 0 ; i < sources.Length ; ++i)
                _list[i] = WebApplication.GetDataSource(sources[i], name_args);
        }

        public      override        WebCoreDataValue    GetValue(WebCoreCall httpCall)
        {
            WebCoreDataValue        rtn = WebCoreDataValue.NoValue;

            for (int i = 0 ; i < _list.Length ; ++i) {
                rtn = _list[i].GetValue(httpCall);

                if (rtn.Type != WebCoreDataValueType.NoValue)
                    break;
            }

            return rtn;
        }

        public      override        string              ToString()
        {
            return "multiple:" + Name;
        }
    }
}
