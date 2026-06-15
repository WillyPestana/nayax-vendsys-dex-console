USE VendSysDex;
GO

IF OBJECT_ID(N'dbo.DEXMeter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DEXMeter
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DEXMeter PRIMARY KEY,
        MachineId NVARCHAR(20) NOT NULL,
        DEXDateTime DATETIME2(0) NOT NULL,
        MachineSerialNumber NVARCHAR(20) NOT NULL,
        ValueOfPaidVends DECIMAL(18, 2) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_DEXMeter_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2(0) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_DEXMeter_MachineId_DEXDateTime' AND object_id = OBJECT_ID(N'dbo.DEXMeter'))
BEGIN
    CREATE UNIQUE INDEX UX_DEXMeter_MachineId_DEXDateTime
        ON dbo.DEXMeter (MachineId, DEXDateTime);
END
GO

IF OBJECT_ID(N'dbo.DEXLaneMeter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DEXLaneMeter
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DEXLaneMeter PRIMARY KEY,
        DEXMeterId INT NOT NULL,
        ProductIdentifier NVARCHAR(6) NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        NumberOfVends INT NOT NULL,
        ValueOfPaidSales DECIMAL(18, 2) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_DEXLaneMeter_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DEXLaneMeter_DEXMeter
            FOREIGN KEY (DEXMeterId)
            REFERENCES dbo.DEXMeter (Id)
            ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DEXLaneMeter_DEXMeterId' AND object_id = OBJECT_ID(N'dbo.DEXLaneMeter'))
BEGIN
    CREATE INDEX IX_DEXLaneMeter_DEXMeterId
        ON dbo.DEXLaneMeter (DEXMeterId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DEXLaneMeter_ProductIdentifier' AND object_id = OBJECT_ID(N'dbo.DEXLaneMeter'))
BEGIN
    CREATE INDEX IX_DEXLaneMeter_ProductIdentifier
        ON dbo.DEXLaneMeter (ProductIdentifier);
END
GO
