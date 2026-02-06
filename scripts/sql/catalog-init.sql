-- =========================================
-- KubeCart Catalog DB Init
-- Creates DB + Categories + Products tables
-- =========================================

IF DB_ID('KubeCart_Catalog') IS NULL
BEGIN
    CREATE DATABASE KubeCart_Catalog;
END
GO

USE KubeCart_Catalog;
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL UNIQUE,
        CreatedAtUtc DATETIME2 NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CategoryId INT NULL,
        Name NVARCHAR(200) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL,

        CONSTRAINT FK_Products_Categories
            FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );
END
GO

-- Optional seed data (safe, simple)
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, CreatedAtUtc)
    VALUES
      ('Phones', SYSUTCDATETIME()),
      ('Accessories', SYSUTCDATETIME()),
      ('Gaming', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @Phones INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name='Phones');
    DECLARE @Gaming INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name='Gaming');
    DECLARE @Accessories INT = (SELECT TOP 1 Id FROM dbo.Categories WHERE Name='Accessories');

    INSERT INTO dbo.Products (CategoryId, Name, Price, ImageUrl, CreatedAtUtc)
    VALUES
      (@Phones, 'iPhone 15', 999.00, 'https://example.com/iphone15.jpg', SYSUTCDATETIME()),
      (@Gaming, 'Gaming Keyboard', 79.99, 'https://example.com/keyboard.jpg', SYSUTCDATETIME()),
      (@Accessories, 'Mouse', 29.99, 'https://example.com/mouse.jpg', SYSUTCDATETIME());
END
GO

PRINT '✅ KubeCart_Catalog initialized';
GO
