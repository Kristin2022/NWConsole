
using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
/*

Edit a specified record from the Products table

Display a specific Product (all product fields should be displayed)
Use NLog to track user functions*/


// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{

    var db = new NWContext();
    string choice;
    do
    {
        //display works
        Console.WriteLine("1) Display Categories");
        //add category works 
        Console.WriteLine("2) Add Category");
        //display category & related products works
        Console.WriteLine("3) Display Category and related products");
        //all categories display 
        //TODO related added products do not display
        Console.WriteLine("4) Display ALL Categories and their related products");
        System.Console.WriteLine("5) Add a Product");
        System.Console.WriteLine("6) Edit a Product");
        System.Console.WriteLine("7) Select what you want to display of products (all, discontinued, or active)");
        System.Console.WriteLine("8) Delete a product");
        System.Console.WriteLine("9) Delete a category");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        System.Console.WriteLine("");
        Console.Clear();
        logger.Info($"Option {choice} selected");
        //Display Categories works
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        // Add Category works
        else if (choice == "2")
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                logger.Info("Category added - {category}", category.CategoryName);
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    // save category to db
                    db.AddCategory(category);
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }

        }
        // Display Category and related products works
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            int id = int.Parse(Console.ReadLine());
            //Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }

        }
        //Display all Categories and their related products
        else if (choice == "4")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        // Add new records to the Products table works
        else if (choice == "5")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to add");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;

            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            Product product = new Product();

            Console.WriteLine("Enter Product Name:");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter the Product Description:");
            product.ProductName = Console.ReadLine();
            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                // save product to db
                db.AddProducts(product);
                // check for unique name
                if (db.Products.Any(c => c.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                }
            }

        }
        // TODO Edit a Product
        else if (choice == "6")
        {
            var query = db.Products.OrderBy(p => p.ProductId);

            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }
            Console.WriteLine("Enter product ID you would like to edit: ");

            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            Product product = new Product();

            db.Products.Find(id);
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Product Getproduct = new Product();
            if (product != null)
            {
                System.Console.WriteLine("Enter updated name of product");
                product.ProductName = Console.ReadLine();
                System.Console.WriteLine("Enter the new product description: ");
                Console.ReadLine();

                db.SaveChanges();
                logger.Info($"Product {id} and {product}");
            }
            else
            {
                logger.Info($"Product {id} and {product}");
            }

        }
        //Select what you want to display of products works
        System.Console.WriteLine("7) Select what you want to display of products");
        if (choice == "7")
        {
            System.Console.WriteLine("1) All products");
            System.Console.WriteLine("2) Discontinued products");
            System.Console.WriteLine("3) Active products");
            string productChoice = Console.ReadLine();
            DisplayProducts(db, productChoice);
        }
        // Delete a product works 
        else if (choice == "8")
        {
            Console.WriteLine("Enter the ID of the product you want to delete:");
            int id = int.Parse(Console.ReadLine());
            var product = db.Products.Find(id);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
            else
            {
                Console.WriteLine("Product not found");
            }
        }
        // Delete a category works
        else if (choice == "9")
        {
            Console.WriteLine("Enter the name of the category you want to delete:");
            string name = Console.ReadLine();
            var category = db.Categories.FirstOrDefault(c => c.CategoryName == name);
            if (category != null)
            {
                // Check if the category has any related products
                if (category.Products.Any())
                {
                    Console.WriteLine("This category has related products. Please delete or reassign these products before deleting the category.");
                }
                else
                {
                    db.Categories.Remove(category);
                    db.SaveChanges();
                    if (db.Categories.Any(c => c.CategoryName == name))
                    {
                        Console.WriteLine("Failed to delete the category.");
                    }
                    else
                    {
                        Console.WriteLine("Category deleted successfully.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Category not found");
            }
        }

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}


static Product GetProduct(NWContext db, Logger logger)
{
    var products = db.Products.OrderBy(p => p.ProductId);
    foreach (Product p in products)
    {
        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
    }
    if (int.TryParse(Console.ReadLine(), out int ProductId))
    {
        Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
        if (product != null)
        {
            return product;
        }
    }
    return null;
}
static void DisplayProducts(NWContext db, string productChoice)
{
    IQueryable<Product> query;

    switch (productChoice)
    {
        case "1": // All products
            query = db.Products;
            break;
        case "2": // Discontinued products
            query = db.Products.Where(p => p.Discontinued);
            break;
        case "3": // Active products
            query = db.Products.Where(p => !p.Discontinued);
            break;
        default:
            Console.WriteLine("Invalid choice. Please select 1, 2, or 3.");
            return;
    }

    foreach (var product in query)
    {
        Console.WriteLine($"Product ID: {product.ProductId}, Product Name: {product.ProductName}");
    }
}

logger.Info("Program ended");