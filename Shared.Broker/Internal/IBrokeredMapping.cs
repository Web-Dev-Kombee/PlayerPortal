using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Broker.Internal
{
    public interface IBrokeredSingleMapping<in TData> { }
    public interface IBrokeredManyMapping<in TData> { }
}
