using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints
{
	public class ListBasketResponse : BaseResponse
    {
        public int Id { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
        public string BuyerId { get; set; }

        public decimal Total()
        {
            return Math.Round(Items.Sum(x => x.UnitPrice * x.Quantity), 2);
        }


        public class BasketItem
        {
            public int Id { get; set; }
            public int CatalogItemId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; } // the current unit price of the catalog item
            public decimal OldUnitPrice { get; set; } // the unit price when the item was first added to basket
            public int Quantity { get; set; }
            public string PictureUrl { get; set; }
        }
    }
}