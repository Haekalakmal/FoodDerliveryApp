using HotChocolate.AspNetCore.Authorization;
using FoodService.Models;



namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER", "BUYER" })]
        public IQueryable<Food> GetFoods([Service] FoodDeliveryContext context) =>
            context.Foods;
    }




}