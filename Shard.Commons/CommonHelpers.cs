using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Commons
{
    public static class CommonHelpers
    {
        public static int ConvertPageintoOffset(int pageNumber, int pageSize)
        {
            return (Math.Max(pageNumber, 1) - 1) * pageSize;
        }
    }

}
