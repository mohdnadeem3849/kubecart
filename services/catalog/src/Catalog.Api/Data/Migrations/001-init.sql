USE KubeCart_Catalog;
GO

-- Categories
IF OBJECT_ID('dbo.Categories','U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- Products
IF OBJECT_ID('dbo.Products','U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CategoryId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(Id)
    );
END
GO

-- ProductImages (URLs)
IF OBJECT_ID('dbo.ProductImages','U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ImageUrl NVARCHAR(1000) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products(Id)
    );
END
GO

-- AuditLogs
IF OBJECT_ID('dbo.AuditLogs','U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Action NVARCHAR(100) NOT NULL,
        TimestampUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Detail NVARCHAR(2000) NULL
    );
END
GO

-- Seed data (only if empty)
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    DECLARE @c1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @c2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @c3 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Categories (Id, Name) VALUES
    (@c1, 'Electronics'),
    (@c2, 'Home & Kitchen'),
    (@c3, 'Fitness');

    DECLARE @p1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @p2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @p3 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Products (Id, CategoryId, Name, Description, Price, Stock) VALUES
    (@p1, @c1, 'Wireless Mouse', 'Ergonomic wireless mouse', 19.99, 50),
    (@p2, @c2, 'Steel Water Bottle', '1L insulated bottle', 24.50, 100),
    (@p3, @c3, 'Yoga Mat', 'Non-slip yoga mat', 29.00, 40);

    INSERT INTO dbo.ProductImages (Id, ProductId, ImageUrl) VALUES
    (NEWID(), @p1, 'https://example.com/images/mouse1.jpg'),
    (NEWID(), @p2, 'https://example.com/images/bottle1.jpg'),
    (NEWID(), @p3, 'https://example.com/images/yogamat1.jpg');
END
GO
