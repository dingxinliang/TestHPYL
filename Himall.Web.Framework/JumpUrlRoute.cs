using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Web.Framework
{
    public class JumpUrlRoute
    {
        // Properties
        public string Action { get; set; }

        public string Area { get; set; }

        public string Controller { get; set; }

        public bool IsSpecial { get; set; }

        public string PC { get; set; }

        public string WAP { get; set; }

        public string WX { get; set; }
    }
}
