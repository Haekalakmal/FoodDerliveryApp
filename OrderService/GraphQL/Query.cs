using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OrderService.Models;


namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> GetOrdersByToken([Service] FoodDeliveryContext context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            if (user != null)
            {
                var orders = context.Orders.Where(o => o.UserId == user.Id).Include(o => o.OrderDetails);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Order> GetOrders([Service] FoodDeliveryContext context) =>
            context.Orders.Include(o => o.OrderDetails);

        //View Tracking Order By Buyer
        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> ViewTrackingOrderByToken([Service] FoodDeliveryContext context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            if (user != null)
            {
                var orders = context.Orders.Where(o => o.UserId == user.Id).Include(o => o.OrderDetails).OrderBy(o => o.Id).LastOrDefault();
                var latestOrder = context.Orders.Where(o => o.Id == orders.Id);
                return latestOrder.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

    }
}
