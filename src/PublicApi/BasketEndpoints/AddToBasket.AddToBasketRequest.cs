namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints
{
	public class AddToBasketRequest : BaseRequest
    {
        public int CatalogItemId { get; set; }
    }
}