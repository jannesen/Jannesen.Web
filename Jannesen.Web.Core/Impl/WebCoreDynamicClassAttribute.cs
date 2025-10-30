using System;
using System.Reflection;

namespace Jannesen.Web.Core.Impl
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class WebCoreDynamicClassAttribute: Attribute
    {
        private readonly        string                          _name;

        public      abstract    string                          Type            { get ; }
        public                  string                          Name            => _name;

        protected                                               WebCoreDynamicClassAttribute(string name)
        {
            _name  = name;
        }

        public      abstract    ConstructorInfo                 GetConstructor(Type classType);

        protected   static      ConstructorInfo                 GetConstructorFor(Type type, Type baseClass, params Type[] argTypes)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(baseClass);
            ArgumentNullException.ThrowIfNull(argTypes);

            if (baseClass != null && !type.IsSubclassOf(baseClass))
                throw new InternalErrorException("Internal error, " + type.FullName + " is not a subclass of " + baseClass.FullName + ".");

            var constructorInfo = type.GetConstructor(argTypes);

            if (constructorInfo == null) {
                var msg = "Internal error, missing constructor " + type.FullName + "(";

                for (var i = 0 ; i < argTypes.Length ; ++i) {
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
            return Type.GetHashCode(StringComparison.Ordinal) ^ _name.GetHashCode(StringComparison.Ordinal);
        }
        public      override    bool                            Equals(object? obj)
        {
            if (obj != null && obj.GetType() == this.GetType()) {
                if (((WebCoreDynamicClassAttribute)obj)._name == _name)
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
