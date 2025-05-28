using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Broker
{
    public interface IBrokered { }
    public interface IBrokeredReturns<TOutput> { }
    public interface IBrokeredAsync { }
    public interface IBrokeredAsyncReturns<TOutput> { }

    /// <summary>
    /// Indicates the IBrokered and IBrokeredReturns (or async versions) questions aren't required to have a handler present.
    /// </summary>
    public interface IBrokeredHandlerIsOptional { }
}
