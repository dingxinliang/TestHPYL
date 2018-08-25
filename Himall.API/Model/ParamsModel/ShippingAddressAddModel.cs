﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    public class ShippingAddressAddModel
    {
        public int regionId { get; set; }

        public string address { get; set; }

        public string addressDetail { get; set; }

        public string phone { get; set; }

        public string shipTo { get; set; }

        public float longitude { get; set; }

        public float latitude { get; set; }

        public long? shopbranchid { get; set; }
    }
}
