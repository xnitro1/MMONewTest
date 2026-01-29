using System.Reflection;

namespace NightBlade
{
    public class FieldAttributeInfo<T> where T : Attribute
    {
        public FieldInfo Field { get; set; }
        public object Source { get; set; }
        public T AttributeInstance { get; set; }
    }
}







