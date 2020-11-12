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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class List : BaseAsyncEndpoint<ListBasketResponse>
    {
        private readonly IAsyncRepository<Basket> _basketRepository;
		private readonly IAsyncRepository<CatalogItem> _itemRepository;
		private readonly IUriComposer _uriComposer;
        private readonly IFileSystem _webFileSystem;

        public List(IAsyncRepository<Basket> basketRepository, IAsyncRepository<CatalogItem> itemRepository, IUriComposer uriComposer, IFileSystem webFileSystem)
        {
            _basketRepository = basketRepository;
			_itemRepository = itemRepository;
			_uriComposer = uriComposer;
            _webFileSystem = webFileSystem;
        }

        [HttpGet("api/basket")]
        [SwaggerOperation(
            Summary = "Adds a Catalog Item to Basket",
            Description = "Adds a Catalog Item to Basket",
            OperationId = "basket.list",
            Tags = new[] { "BasketEndpoints" })
        ]
        public override async Task<ActionResult<ListBasketResponse>> HandleAsync(CancellationToken cancellationToken)
        {
            return Ok(GetOrCreateBasketForUser(User.Identity.Name));
        }

        private async Task<ListBasketResponse> GetOrCreateBasketForUser(string userName)
        {
            var basketSpec = new BasketWithItemsSpecification(userName);
            var basket = (await _basketRepository.FirstOrDefaultAsync(basketSpec));

            if (basket == null)
            {
                return await CreateBasketForUser(userName);
            }
            return await CreateViewModelFromBasket(basket);
        }

        private async Task<ListBasketResponse> CreateViewModelFromBasket(Basket basket)
        {
            var viewModel = new ListBasketResponse
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = await GetBasketItems(basket.Items)
            };

            return viewModel;
        }

        private async Task<ListBasketResponse> CreateBasketForUser(string userId)
        {
            var basket = new Basket(userId);
            await _basketRepository.AddAsync(basket);

            return new ListBasketResponse()
            {
                BuyerId = basket.BuyerId,
                Id = basket.Id,
            };
        }

        private async Task<List<ListBasketResponse.BasketItem>> GetBasketItems(IReadOnlyCollection<BasketItem> basketItems)
        {
            var catalogItemsSpecification = new CatalogItemsSpecification(basketItems.Select(b => b.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basketItems.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);

                var basketItemViewModel = new ListBasketResponse.BasketItem
                {
                    Id = basketItem.Id,
                    OldUnitPrice = basketItem.UnitPrice,
                    UnitPrice = catalogItem.Price,
                    Quantity = basketItem.Quantity,
                    CatalogItemId = basketItem.CatalogItemId,
                    PictureUrl = _uriComposer.ComposePicUri(catalogItem.PictureUri),
                    ProductName = catalogItem.Name
                };
                return basketItemViewModel;
            }).ToList();

            return items;
        }
    }
}
