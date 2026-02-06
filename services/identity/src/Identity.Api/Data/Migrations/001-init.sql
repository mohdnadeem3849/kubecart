-- ============================================
-- Identity Service DB Migration
-- File: 001-init.sql
-- Database: KubeCart_Identity
-- ============================================

USE KubeCart_Identity;
GO

-- USERS TABLE
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- USER SESSIONS TABLE
CREATE TABLE UserSessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RefreshToken NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserSessions_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- AUDIT LOGS TABLE
CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(1000) NULL,
    RequestId NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- SEED ADMIN USER (TEMP PASSWORD)
INSERT INTO Users (Email, PasswordHash, Role)
VALUES ('admin@kubecart.local', 'TEMP_PASSWORD_HASH', 'Admin');
GO
