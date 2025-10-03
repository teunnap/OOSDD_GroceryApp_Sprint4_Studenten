
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class BoughtProductsService : IBoughtProductsService
    {
        private readonly IGroceryListItemsRepository _groceryListItemsRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IGroceryListRepository _groceryListRepository;
        public BoughtProductsService(IGroceryListItemsRepository groceryListItemsRepository, IGroceryListRepository groceryListRepository, IClientRepository clientRepository, IProductRepository productRepository)
        {
            _groceryListItemsRepository=groceryListItemsRepository;
            _groceryListRepository=groceryListRepository;
            _clientRepository=clientRepository;
            _productRepository=productRepository;
        }
        public List<BoughtProducts> Get(int? productId)
        {
            List<BoughtProducts> result = new();

            // If no specific productId is provided, return empty list
            if (productId is null)
            {
                return result;
            }

            // Fetch all lists and items once
            List<GroceryList> lists = _groceryListRepository.GetAll();
            List<GroceryListItem> items = _groceryListItemsRepository.GetAll();
            List<Product> products = _productRepository.GetAll();
            List<Client> clients = _clientRepository.GetAll();

            // Filter items for the chosen product
            var query = items.Where(i => i.ProductId == productId)
                .Join(lists,
                    i => i.GroceryListId,
                    l => l.Id,
                    (i, l) => new { Item = i, List = l })
                .Join(clients,
                    il => il.List.ClientId,
                    c => c.Id,
                    (il, c) => new { il.Item, il.List, Client = c })
                .Join(products,
                    ilc => ilc.Item.ProductId,
                    p => p.Id,
                    (ilc, p) => new { ilc.Client, ilc.List, Product = p });

            foreach (var row in query)
            {
                result.Add(new BoughtProducts(row.Client, row.List, row.Product));
            }

            return result;
        }
    }
}
