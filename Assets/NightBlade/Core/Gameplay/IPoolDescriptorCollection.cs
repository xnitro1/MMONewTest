using System.Collections.Generic;

namespace NightBlade
{
    public interface IPoolDescriptorCollection
    {
        IEnumerable<IPoolDescriptor> PoolDescriptors { get; }
    }
}







