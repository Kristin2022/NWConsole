
using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
/*
Add new records to the Categories table  ????is that #2 
Edit a specified record from the Categories table - done
Display all Categories in the Categories table (CategoryName and Description) done
Display all Categories and their related active (not discontinued) product data (CategoryName, ProductName)
Display a specific Category and its related active product data (CategoryName, ProductName)
*/


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
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        System.Console.WriteLine("5) Add a Product");
        System.Console.WriteLine("6) Edit a Product");
        System.Console.WriteLine("7) Select what you want to display of products (all, discontinued, or active)");
        System.Console.WriteLine("8) Delete a product");
        System.Console.WriteLine("9) Delete a category");
        System.Console.WriteLine("10) Select a product to view all of its information");
        System.Console.WriteLine("11) Edit category");
        System.Console.WriteLine("12) Display Categories and Category descirption");
        System.Console.WriteLine("13) Display all active Categories and their product(s)");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        System.Console.WriteLine("");
        Console.Clear();
        logger.Info($"Option {choice} selected");

        //Display Categories 
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
        //Add Category
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
                    db.Add(category);
                    db.SaveChanges();
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

        //Display Category and related products
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
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
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        // Add new records to the Products table
        else if (choice == "5")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);
            Product product = new Product();
            Console.WriteLine("Select the category whose products you want to add");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;

            product.CategoryId = int.Parse(Console.ReadLine());
            Console.Clear();

            Console.WriteLine("Enter Product Name:");
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
        // Edit a Product
        else if (choice == "6")
        {
            var query = db.Products.OrderBy(p => p.ProductId);
            Console.WriteLine("Select the product id you want to edit");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            // Retrieve the product from the database
            Product product = db.Products.Find(id);

            Console.WriteLine("Edit Product Name:");
            string newName = Console.ReadLine();
            product.ProductName = newName; // Set the new name here

            ValidationContext context = new ValidationContext(product, null, null); // Validate the updated object
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                // save product to db
                db.Update(product);
                db.SaveChanges();
                logger.Info("Validation passed");
            }
            else if (db.Products.Any(c => c.ProductName == product.ProductName))
            {
                // generate validation error
                isValid = false;
                results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
            }
        }

        //Select what you want to display of products 
        else if (choice == "7")
        {
            System.Console.WriteLine("7) Select what you want to display of products");
            System.Console.WriteLine("1) All products");
            System.Console.WriteLine("2) Discontinued products");
            System.Console.WriteLine("3) Active products");
            string productChoice = Console.ReadLine();
            DisplayProducts(db, productChoice);
        }
        // Delete a product  
        else if (choice == "8")
        {
            var query = db.Products.OrderBy(p => p.ProductId);

            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }

            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            Console.WriteLine("Enter the ID of the product you want to delete:");
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
        // Delete a category 
        else if (choice == "9")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId} {item.CategoryName}");
            }
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            Console.WriteLine("Enter the id of the category you want to delete:");
            string name = Console.ReadLine();
            var category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category != null)
            {
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
        //Display a specific Product (all product fields should be displayed)
        else if (choice == "10")
        {
            var query = db.Products.OrderBy(p => p.ProductId);

            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}, {item.ProductName}");
            }

            Console.WriteLine("Enter the Product Id you would like to view");
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            var product = db.Products.Find(id);
            if (product != null)
            {
                Console.WriteLine($"Product Id: {product.ProductId}, Product Name {product.ProductName}, Supplier Id: {product.SupplierId}\n Category Id: {product.CategoryId}\n Quantity per unit: {product.QuantityPerUnit}\n Unit price: {product.UnitPrice}\n Units in stock: {product.UnitsInStock}\n Units on order {product.UnitsOnOrder}\n Reorder level: {product.ReorderLevel}\n Discontinued: {product.Discontinued}\n");
            }
            else
            {
                Console.WriteLine("Product not found");
            }
        }

        // Edit a Category (will not display updated/editted category in display category choice "1")
        else if (choice == "11")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);
            Console.WriteLine("Select the category id you want to edit\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId} {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            // Retrieve the category from the database
            Category category = db.Categories.Find(id);

            Console.WriteLine("Edit Category Name:");
            string newName = Console.ReadLine();

            // Check for unique name
            if (db.Categories.Any(c => c.CategoryName == newName))
            {
                Console.WriteLine("Name exists");
            }
            else
            {
                // Update the category name
                category.CategoryName = newName;

                // Mark the entity as modified
                db.Entry(category).State = EntityState.Modified;

                // Save changes
                db.SaveChanges();
                Console.WriteLine("Category updated successfully");
            }
        }

        // Edit a Category 
        else if (choice == "11")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);
            Console.WriteLine("Select the category id you want to edit\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId} {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            // Retrieve the category from the database
            Category category = db.Categories.Find(id);

            Console.WriteLine("Edit Category Name:");
            string newName = Console.ReadLine();
            category.CategoryName = newName;

            ValidationContext context = new ValidationContext(category, null, null); // Validate the updated object
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                // save category to db
                db.Update(category);
                db.EditCategory(category);
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "Name" }));
                }
                else
                {
                    db.SaveChanges();
                    logger.Info("Validation passed");
                }
            }
        }
        // else if (choice == "12")
        if (choice == "12")
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Blue;
            foreach (var category in query)
            {
                Console.WriteLine($"{category.CategoryName} - {category.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        //Display all Categories and their related active (not discontinued) product data (CategoryName, ProductName)
        else if (choice == "13")
        {

        }
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
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

    int count = query.Count();
    Console.WriteLine($"{count} records returned");

    foreach (var product in query)
    {
        Console.WriteLine($"Product Name: {product.ProductName}");
    }
}
logger.Info("Program ended");