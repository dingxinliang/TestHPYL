using Himall.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Web.Framework
{
    public class CurrentUrlHelper
    {
        // Methods
        public static string CurrentUrl()
        {
            string port = WebHelper.GetPort();
            port = string.IsNullOrEmpty(port) ? string.Empty : (":" + port);
            return (WebHelper.GetScheme() + "://" + WebHelper.GetHost() + port);
        }

        public static string CurrentUrlNoPort()
        {
            return (WebHelper.GetScheme() + "://" + WebHelper.GetHost());
        }

        public static string GetScheme()
        {
            return WebHelper.GetScheme();
        }

    }
}
