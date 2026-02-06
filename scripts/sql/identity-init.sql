-- =========================================
-- KubeCart Identity DB Init
-- Creates DB + Users table + sample users
-- =========================================

IF DB_ID('KubeCart_Identity') IS NULL
BEGIN
    CREATE DATABASE KubeCart_Identity;
END
GO

USE KubeCart_Identity;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Email NVARCHAR(256) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(512) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL
    );
END
GO

-- NOTE:
-- Passwords are hashed in your app. DevOps should register users using /api/auth/register
-- You can also insert pre-hashed users if you want, but safest is register endpoint.

PRINT '✅ KubeCart_Identity initialized';
GO
