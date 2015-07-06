using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace BeerBubbleWeb
{
    public interface ITable<T> where T : BaseEntity
    {
    }
}
