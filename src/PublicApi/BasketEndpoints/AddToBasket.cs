using System.IO;
using System.Threading;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AddToBasket : BaseAsyncEndpoint<AddToBasketRequest, AddToBasketResponse>
    {
        private readonly IAsyncRepository<Basket> _basketRepository;
		private readonly IAsyncRepository<CatalogItem> _itemRepository;

        public AddToBasket(IAsyncRepository<Basket> basketRepository, IAsyncRepository<CatalogItem> itemRepository, IUriComposer uriComposer, IFileSystem webFileSystem)
        {
            _basketRepository = basketRepository;
			_itemRepository = itemRepository;
        }

        [HttpPost("api/basket")]
        [SwaggerOperation(
            Summary = "Adds a Catalog Item to Basket",
            Description = "Adds a Catalog Item to Basket",
            OperationId = "basket.add-catalog-item",
            Tags = new[] { "BasketEndpoints" })
        ]
        public override async Task<ActionResult<AddToBasketResponse>> HandleAsync(AddToBasketRequest request, CancellationToken cancellationToken)
        {
            var itemSpec = new CatalogItemsSpecification(request.CatalogItemId);
            var item = await _itemRepository.FirstOrDefaultAsync(itemSpec);
            if (item == null)
                return NotFound(new { request.CatalogItemId });

            var basketSpec = new BasketWithItemsSpecification(User.Identity.Name);
            var basket = (await _basketRepository.FirstOrDefaultAsync(basketSpec));

            if (basket == null)
            {
                basket = new Basket(User.Identity.Name);
                await _basketRepository.AddAsync(basket);
            }

            basket.AddItem(request.CatalogItemId, item.Price);
            await _basketRepository.UpdateAsync(basket);

            return Ok(new AddToBasketResponse());
        }
    }
}
