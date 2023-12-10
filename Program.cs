using NLog;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
/*
Final Project -  Using the Northwind database

Add new records to the Products table -5)
Edit a specified record from the Products table - 6)
Display all records in the Products table (ProductName only) - user decides if they want to see all products, discontinued products, or active (not discontinued) products. Discontinued products should be distinguished from active products. -7)
Display a specific Product (all product fields should be displayed) -8)
Use NLog to track user functions - bin log_file.txt



Add new records to the Categories table - 2)
Edit a specified record from the Categories table -11)
Display all Categories in the Categories table (CategoryName and Description) -12
Display all Categories and their related active (not discontinued) product data (CategoryName, ProductName) -13
Display a specific Category and its related active product data (CategoryName, ProductName) -14



Delete a specified existing record from the Products table (account for Orphans in related tables) -10
Delete a specified existing record from the Categories table (account for Orphans in related tables) -9
Use data annotations and handle ALL user errors gracefully & log all errors using NLog

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
        System.Console.WriteLine("8) Select a specific product to view all of its information");
        System.Console.WriteLine("9) Delete a category");
        System.Console.WriteLine("10) Delete a specified existing record from the Products table (account for Orphans in related tables)");
        System.Console.WriteLine("11) Edit category");
        System.Console.WriteLine("12)Display all Categories in the Categories table (CategoryName and Description)");
        System.Console.WriteLine("13) Display all Categories and their related active (not discontinued) product data (CategoryName, ProductName)");
        System.Console.WriteLine("14) Display a specific Category and its related active product data (CategoryName, ProductName)");
        System.Console.WriteLine("15) Edit all of the records for a specific product records");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        System.Console.WriteLine("");
        Console.Clear();
        logger.Info($"Option {choice} selected");
        //logger.Warn($"Option {product} sel");

        //Display Categories ("1) Display Categories");
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
        //Add Category ("2) Add Category");
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

        //Display Category and related products ("3) Display Category and related products");
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}\n");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        //Display all Categories and their related products "4) Display all Categories and their related products");
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
        // Add new records to the Products table "5) Add a Product");
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
                logger.Info("Product added - {product}", product.ProductName);
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
        // Edit a specified record from the Products table "6) Edit a Product");
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
            product.ProductName = newName;

            ValidationContext context = new ValidationContext(product, null, null);
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
                logger.Warn("Warning unable to edit product");
                results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                logger.Warn("Warning: unable to edit");
            }
        }

        //Display all records in the Products table (ProductName only) - user decides if they want to see all products, discontinued products, or active (not discontinued) products. Discontinued products should be distinguished from active products. 
        //"7) Select what you want to display of products (all, discontinued, or active)");
        else if (choice == "7")
        {
            System.Console.WriteLine("7) Select what you want to display of products");
            System.Console.WriteLine("1) All products");
            System.Console.WriteLine("2) Discontinued products");
            System.Console.WriteLine("3) Active products");
            string productChoice = Console.ReadLine();
            DisplayProducts(db, productChoice);
        }
        //Display a specific Product (all product fields should be displayed) "8) Select a specific product to view all of its information");
        else if (choice == "8")
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
                Console.WriteLine($"Product Id: {product.ProductId}\n Product Name {product.ProductName}\n Supplier Id: {product.SupplierId}\n Category Id: {product.CategoryId}\n Quantity per unit: {product.QuantityPerUnit}\n Unit price: {product.UnitPrice}\n Units in stock: {product.UnitsInStock}\n Units on order {product.UnitsOnOrder}\n Reorder level: {product.ReorderLevel}\n Discontinued: {product.Discontinued}\n");
            }
            else
            {
                Console.WriteLine("Product not found");
                logger.Error("Product not found.");
            }
        }

        // Delete a specified existing record from the Categories table (account for Orphans in related tables) "9) Delete a category");
        if (choice == "9")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId} {item.CategoryName}");
            }
            Console.WriteLine("Enter the id of the category you want to delete:");
            string input = Console.ReadLine();
            if (!int.TryParse(input, out int id))
            {
                logger.Error("Invalid input for category id.");
                return;
            }
            var category = db.Categories.Include(c => c.Products).SingleOrDefault(c => c.CategoryId == id);
            if (category != null)
            {
                if (category.Products.Any())
                {
                    Console.WriteLine("This category has related products. Do you want to delete these products? (y/n)");
                    string response = Console.ReadLine();
                    if (response.ToLower() == "y")
                    {
                        db.Products.RemoveRange(category.Products);
                        db.Categories.Remove(category);
                        db.SaveChanges();
                        logger.Warn("Category and related products deleted successfully.");
                    }
                    else
                    {
                        logger.Info("Category not deleted.");
                    }
                }
                else
                {
                    db.Categories.Remove(category);
                    db.SaveChanges();
                    logger.Info("Category deleted successfully.");
                }
            }
            else
            {
                logger.Error("Category not found");
            }
        }

        // Delete product  10) Delete a product");
        else if (choice == "10")
        {
            var query = db.Products.OrderBy(p => p.ProductId);

            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}) {item.ProductName}");
            }
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            Console.WriteLine("Enter the ID of the product you want to delete:");

            var product = db.Products.Include(p => p.OrderDetails).SingleOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                db.OrderDetails.RemoveRange(product.OrderDetails);
                db.Products.Remove(product);
                db.SaveChanges();
                logger.Info("Product deleted successfully.");
            }
            else
            {
                Console.WriteLine("Product not found");
                logger.Error("Product invalid.");
            }
        }
        // Edit a specified record from the Categories table 
        else if (choice == "11")
        {
            var query = db.Categories.OrderBy(c => c.CategoryId);
            Console.WriteLine("Select the category id you want to edit");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            // Retrieve the product from the database
            Category category = db.Categories.Find(id);

            Console.WriteLine("Edit Category Name:");
            string newName = Console.ReadLine();
            category.CategoryName = newName;

            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                logger.Info("Validation passed");
                // save category to db
                db.Update(category);
                db.SaveChanges();
                logger.Info("Validation passed");
            }
            else if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                // generate validation error
                isValid = false;
                logger.Warn("Warning unable to edit category");
                results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                logger.Warn("Warning: unable to edit");
            }
        }
        //Display all Categories in the Categories table (CategoryName and Description)Display Categories and Category discription
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
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryName).ToList();
            int totalActiveProducts = 0;

            foreach (var item in query)
            {
                int activeProductCount = item.Products.Count(p => !p.Discontinued);
                totalActiveProducts += activeProductCount;
                Console.WriteLine($"Category Name: {item.CategoryName}\nActive products: {activeProductCount}");

                foreach (Product p in item.Products.Where(p => !p.Discontinued))
                {
                    Console.WriteLine($"\tProduct Name: {p.ProductName}");
                }
            }
            Console.WriteLine($"Total active products: {totalActiveProducts}");
        }


        //Display a specific Category and its related active product data (CategoryName, ProductName)
        else if (choice == "14")
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}, {item.CategoryName}");
            }
            Console.WriteLine("Enter the Id you would like to view");
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            var category = db.Categories.Include(c => c.Products).SingleOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                Console.WriteLine($"{category.CategoryName}");
                foreach (Product p in category.Products.Where(p => !p.Discontinued))
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
            else
            {
                Console.WriteLine("Category not found");
            }
        }
        //"15) Edit all of the records for each specific product record
        else if (choice == "15")
        {
            var query = db.Products.OrderBy(p => p.ProductId);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            foreach (var item in query)
            {
                Console.WriteLine($"{item.ProductId}, {item.ProductName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();

            Product product = db.Products.Find(id);
            EditProduct(product);

            // Save changes to the database
            db.SaveChanges();
        }


    } while (choice.ToLower() != "q");
}

catch (Exception ex)
{
    System.Console.WriteLine($"An error has occured: {ex.Message}");
    logger.Error(ex.Message);
}

static void EditProduct(Product product)
{
    // Edit Product Name
    System.Console.WriteLine("Edit Product Name");
    string newName = Console.ReadLine();
    product.ProductName = newName;

    // Edit Supplier Id
    System.Console.WriteLine("Edit Supplier Id:");
    int newSupplierId = int.Parse(Console.ReadLine());
    product.SupplierId = newSupplierId;

    // Edit Quantity Per Unit
    System.Console.WriteLine("Edit Product Quantity Per Unit");
    string newQuantityPerUnit = Console.ReadLine();
    product.QuantityPerUnit = newQuantityPerUnit;

    // Edit Unit Price
    System.Console.WriteLine("Edit Unit Price");
    decimal newUnitPrice = decimal.Parse(Console.ReadLine());
    product.UnitPrice = newUnitPrice;

    // Edit Units in Stock
    System.Console.WriteLine("Edit units in stock");
    short newUnitsInStock = short.Parse(Console.ReadLine());
    product.UnitsInStock = newUnitsInStock;

    // Edit Units on Order
    System.Console.WriteLine("Edit units on order");
    short newUnitsOnOrder = short.Parse(Console.ReadLine());
    product.UnitsOnOrder = newUnitsOnOrder;

    // Edit Units in Stock
    System.Console.WriteLine("Edit reorder level");
    short newReorderLevel = short.Parse(Console.ReadLine());
    product.ReorderLevel = newReorderLevel;

    // Edit Discontinued

    System.Console.WriteLine("Edit Discontinued status (true/false):");
    bool newDiscontinued;
    if (bool.TryParse(Console.ReadLine(), out newDiscontinued))
    {
        product.Discontinued = newDiscontinued;
    }
    else
    {
        System.Console.WriteLine("Invalid input. Please enter 'true' or 'false'.");
    }
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

