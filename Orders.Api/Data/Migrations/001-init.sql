-- Orders DB init (safe to run multiple times)

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CartItems_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_CartItems_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_CartItems_UserId ON dbo.CartItems(UserId);
    CREATE INDEX IX_CartItems_User_Product ON dbo.CartItems(UserId, ProductId);
END
GO

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT ('Pending'),
        Subtotal DECIMAL(18,2) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Orders_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
END
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,

        -- Snapshot values (microservice rule)
        ProductName NVARCHAR(200) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        ImageUrl NVARCHAR(500) NULL,

        LineTotal AS (Quantity * UnitPrice) PERSISTED,

        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
END
GO
