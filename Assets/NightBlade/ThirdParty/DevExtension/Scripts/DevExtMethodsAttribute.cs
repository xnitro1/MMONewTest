using System;

namespace NightBlade.DevExtension
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DevExtMethodsAttribute : Attribute
    {
        public string BaseMethodName { get; private set; }
        public DevExtMethodsAttribute(string baseMethodName)
        {
            BaseMethodName = baseMethodName;
        }
    }
}







