IF DB_ID('InvoiceDb') IS NOT NULL
BEGIN
    ALTER DATABASE InvoiceDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InvoiceDb;
END
GO

CREATE DATABASE InvoiceDb;
GO

USE InvoiceDb;
GO

CREATE TABLE dbo.Invoices (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ClientName NVARCHAR(200) NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    IssueDate DATE NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX UX_Invoices_InvoiceNumber ON dbo.Invoices (InvoiceNumber);
GO

CREATE INDEX IX_Invoices_ClientName ON dbo.Invoices (ClientName);
GO

CREATE OR ALTER PROCEDURE dbo.sp_Invoice_Create
    @Id UNIQUEIDENTIFIER,
    @ClientName NVARCHAR(200),
    @InvoiceNumber NVARCHAR(50),
    @IssueDate DATE,
    @TotalAmount DECIMAL(18,2),
    @Currency NVARCHAR(10),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Invoices WHERE InvoiceNumber = @InvoiceNumber)
    BEGIN
        RAISERROR('InvoiceNumber already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Invoices (Id, ClientName, InvoiceNumber, IssueDate, TotalAmount, Currency, Description)
    VALUES (@Id, @ClientName, @InvoiceNumber, @IssueDate, @TotalAmount, @Currency, @Description);

    SELECT * FROM dbo.Invoices WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Invoice_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Invoices WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Invoice_SearchByClient
    @ClientName NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (200) *
    FROM dbo.Invoices
    WHERE ClientName LIKE '%' + @ClientName + '%'
    ORDER BY CreatedAt DESC;
END
GO