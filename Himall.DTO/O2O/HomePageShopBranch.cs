using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Himall.DTO.Product;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class HomePageShopBranch
    {
        public ShopActiveList ShopAllActives { get; set; }

        public ShopBranch ShopBranch { get; set; }

        public List<Product.Product> Products { get; set; }
    }
}
