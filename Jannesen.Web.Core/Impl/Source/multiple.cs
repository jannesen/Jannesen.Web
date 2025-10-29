using System;

namespace Jannesen.Web.Core.Impl.Source
{
    sealed class multiple: WebCoreDataSource
    {
        public      const           char                SplitChar = '|';

        private readonly            WebCoreDataSource[] _list;

        public                                          multiple(string source, string name_args): base(source)
        {
            var sources = source.Split(SplitChar);

            _list = new WebCoreDataSource[sources.Length];

            for (var i = 0 ; i < sources.Length ; ++i) {
                _list[i] = WebLoader.Instance.GetDataSource(sources[i], name_args);
            }
        }

        public      override        WebCoreDataValue    GetValue(WebCoreCall httpCall)
        {
            var rtn = WebCoreDataValue.NoValue;

            for (var i = 0 ; i < _list.Length ; ++i) {
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
