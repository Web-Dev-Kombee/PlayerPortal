using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Broker.Internal
{
    public class ProvidedIntent
    {
        public ProvidedIntent(bool res, object data) { Result = res; Data = data; }
        public bool Result { get; private set; }
        public object Data { get; private set; }
    }
}
