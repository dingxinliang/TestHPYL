using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.SellerAdmin.Models
{
    public class WdgjApiModel
    {
        // Properties
        public long Id { get; set; }

        [Required(ErrorMessage = "接入码必须填写")]
        public string uCode { get; set; }

        [Required(ErrorMessage = "效验码必须填写")]
        public string uSign { get; set; }

    }
}