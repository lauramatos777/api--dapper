using api__dapper.domain.models;
using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.http.middlewares;

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;

namespace api__dapper.http.routes;

public static class SellRoutes
{
  public static void MapSellEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/sells").WithTags("Sales");

    // Admin: Ver todas as vendas
    group.MapGet("/", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (ISellService service) =>
        Results.Ok(await service.GetAllSells()));

    // Admin: Ver venda específica por ID
    group.MapGet("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, ISellService service) =>
    {
      var sell = await service.GetSellById(id);
      return sell is null ? Results.NotFound() : Results.Ok(sell);
    });

    // User: Ver suas próprias vendas
    group.MapGet("/my-purchases", [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)] async (ISellService service, ClaimsPrincipal user) =>
    {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

      var sells = await service.GetSellsByUserId(userId);
      return Results.Ok(sells);
    });

    // User: Criar uma nova compra
    group.MapPost("/", [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)] async (CreateSell sellDto, ISellService service, ClaimsPrincipal user) =>
    {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

      var sell = await service.CreateSell(userId, sellDto);
      return Results.Created($"/api/sells/{sell.Id}", sell);
    });

    // Admin: Atualizar status da venda
    group.MapPut("/{id}/status", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, UpdateSellStatus statusDto, ISellService service) =>
    {
      var updatedSell = await service.UpdateSellStatus(id, statusDto);
      return updatedSell is null ? Results.NotFound() : Results.Ok(updatedSell);
    });

    // Admin: Deletar venda
    group.MapDelete("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, ISellService service) =>
    {
      var result = await service.DeleteSell(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}
