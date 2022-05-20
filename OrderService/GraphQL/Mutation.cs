using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using OrderService.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderData> AddOrderAsync(
             OrderData input,
             ClaimsPrincipal claimsPrincipal,
             [Service] FoodDeliveryContext context)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
                var kurir = context.Couriers.Where(o => o.Id == input.CourierId).FirstOrDefault();
                if (user != null)
                {
                    if (kurir.Availibility == true)
                    {
                        // EF
                        var order = new Order
                        {
                            Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                            UserId = user.Id,
                            CourierId = input.CourierId
                        };
                        foreach (var item in input.Details)
                        {
                            var detail = new OrderDetail
                            {
                                OrderId = order.Id,
                                FoodId = item.FoodId,
                                Quantity = item.Quantity
                            };
                            order.OrderDetails.Add(detail);
                        }

                        context.Orders.Add(order);
                        kurir.Availibility = false;
                        context.Couriers.Update(kurir);

                        context.SaveChanges();
                        await transaction.CommitAsync();
                    }

                    //input.Id = order.Id;
                    //input.Code = order.Code;
                }
                else
                    throw new Exception("user was not found");
            }
            catch (Exception err)
            {
                transaction.Rollback();
            }
            return input;
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<OrderData> UpdateOrderAsync(
            OrderData input,
            [Service] FoodDeliveryContext context)
        {
            var user = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.Code = Guid.NewGuid().ToString();
                user.UserId = input.UserId;
                user.CourierId = input.CourierId;


                context.Orders.Update(user);
                await context.SaveChangesAsync();
            }
            return input;
        }
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Order> DeleteOrderByIdAsync(
            int id,
            [Service] FoodDeliveryContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).Include(o => o.OrderDetails).FirstOrDefault();
            if (order != null)
            {
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(order);
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<TrackingOrder> AddTrackingAsync(
            TrackingOrder input,
            [Service] FoodDeliveryContext context)
        {
            var order = context.Orders.Where(o => o.Id == input.Id).FirstOrDefault();
            if (order != null)
            {
                order.Id = input.Id;
                order.Longitude = input.Longitude;
                order.Latitude = input.Latitude;

                context.Orders.Update(order);
                context.SaveChanges();
            }
            return input;
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<Order> CompleteOrderAsync(
            int id,
            [Service] FoodDeliveryContext context)
        {
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();
            var kurir = context.Couriers.Where(o => o.Id == order.CourierId).FirstOrDefault();
            if (order != null)
            {
                // EF
                order.Id = id;
                kurir.Availibility = true;
                context.Couriers.Update(kurir);
                context.SaveChanges();
            }
            return await Task.FromResult(order);
        }
    }


}