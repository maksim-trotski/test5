CREATE DATABASE _5ElemCatalog;
GO

USE _5ElemCatalog;
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    ImageUrl NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
GO

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    CategoryId INT NULL,
    ImageUrl NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
);
GO

CREATE TRIGGER TR_Products_Update
ON Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Products 
    SET UpdatedAt = GETDATE() 
    WHERE Id IN (SELECT DISTINCT Id FROM inserted);
END;
GO

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Users_Username ON Users(Username);
GO

INSERT INTO Users (Username, Email, PasswordHash) 
VALUES ('admin', 'admin@example.com', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
GO

INSERT INTO Categories (Name, Description) VALUES 
('Электроника', 'Смартфоны, ноутбуки и аксессуары'),
('Одежда', 'Мужская и женская одежда'),
('Дом и сад', 'Товары для дома и дачи');
GO