using AzureKeyVaultExample.Data;
using Microsoft.AspNetCore.Mvc;

namespace AzureKeyVaultExample.Routes;

public class ProductGetAll
{
    public static string Template => "/products";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;
    public static IResult Action([FromServices] ApplicationDbContext context)
    {
        return Results.Ok(context.Products.ToList());
    }
}
