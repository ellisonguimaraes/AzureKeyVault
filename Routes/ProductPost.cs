using AzureKeyVaultExample.Data;
using AzureKeyVaultExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureKeyVaultExample.Routes;

public class ProductPost
{
    public static string Template => "/products";
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
    public static Delegate Handle => Action;
    public static IResult Action(
        [FromBody] ProductRequest productRequest,
        [FromServices] ApplicationDbContext context)
    {
        var product = new Product(productRequest.Name, productRequest.Price);
        context.Products.Add(product);
        context.SaveChanges();

        return Results.Created($"{Template}/{product.Id}", product.Id);
    }
}

