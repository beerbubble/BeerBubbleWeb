using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BeerBubbleWeb
{
    public class LoginAttribute : FilterAttribute, IExceptionFilter
    {

        public virtual void OnException(ExceptionContext filterContext)
        { }

    }
}
