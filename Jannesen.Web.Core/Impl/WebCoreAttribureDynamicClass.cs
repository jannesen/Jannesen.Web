using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class WebCoreAttribureDynamicClass: Attribute
    {
        private readonly        string                          _name;

        public      abstract    string                          Type            { get ; }
        public                  string                          Name
        {
            get {
                return _name;
            }
        }

        protected                                               WebCoreAttribureDynamicClass(string name)
        {
            _name  = name;
        }

        public      abstract    ConstructorInfo                 GetConstructor(Type classType);

        protected   static      ConstructorInfo                 GetConstructorFor(Type type, Type baseClass, params Type[] argTypes)
        {
            if (baseClass != null && !type.IsSubclassOf(baseClass))
                throw new InternalErrorException("Internal error, " + type.FullName + " is not a subclass of " + baseClass.FullName + ".");

            ConstructorInfo     constructorInfo = type.GetConstructor(argTypes);

            if (constructorInfo == null) {
                string  msg = "Internal error, missing constructor " + type.FullName + "(";

                for (int i = 0 ; i < argTypes.Length ; ++i) {
                    if (i > 0)
                        msg += ",";

                    msg += argTypes[i].Name;
                }

                msg += ").";

                throw new InternalErrorException(msg);
            }

            return constructorInfo;
        }

        public      override    int                             GetHashCode()
        {
            return Type.GetHashCode() ^ _name.GetHashCode();
        }
        public      override    bool                            Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType()) {
                if (((WebCoreAttribureDynamicClass)obj)._name == _name)
                    return true;
            }

            return false;
        }
        public      override    string                          ToString()
        {
            return Type + ": " + Name;
        }
    }
}
