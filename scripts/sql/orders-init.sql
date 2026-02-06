-- =========================================
-- KubeCart Orders DB Init
-- Creates DB + CartItems + Orders + OrderItems
-- =========================================

IF DB_ID('KubeCart_Orders') IS NULL
BEGIN
    CREATE DATABASE KubeCart_Orders;
END
GO

USE KubeCart_Orders;
GO

-- 1) CartItems
IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL
    );
END
GO

-- 2) Orders
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT(0),
        Status NVARCHAR(50) NOT NULL DEFAULT('Pending'),
        CreatedAtUtc DATETIME2 NOT NULL,
        Notes NVARCHAR(1000) NULL
    );
END
GO

-- 3) OrderItems (snapshots)
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OrderId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        ProductNameSnapshot NVARCHAR(200) NOT NULL,
        UnitPriceSnapshot DECIMAL(18,2) NOT NULL,
        ImageUrlSnapshot NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_OrderItems_Orders
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id)
            ON DELETE CASCADE
    );
END
GO

-- Helpful indexes (optional)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CartItems_UserId' AND object_id=OBJECT_ID('dbo.CartItems'))
BEGIN
    CREATE INDEX IX_CartItems_UserId ON dbo.CartItems(UserId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_UserId' AND object_id=OBJECT_ID('dbo.Orders'))
BEGIN
    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_OrderItems_OrderId' AND object_id=OBJECT_ID('dbo.OrderItems'))
BEGIN
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
END
GO

PRINT '✅ KubeCart_Orders initialized';
GO
