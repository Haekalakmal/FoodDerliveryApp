using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using System;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
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
                if (user != null)
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
                    context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    //input.Id = order.Id;
                    //input.Code = order.Code;
                }
                else
                    throw new Exception("user was not found");
            }
            catch (Exception error)
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
    }


}