USE VendSysDex;
GO

CREATE OR ALTER PROCEDURE dbo.SaveDEXMeter
    @MachineId NVARCHAR(20),
    @DEXDateTime DATETIME2(0),
    @MachineSerialNumber NVARCHAR(20),
    @ValueOfPaidVends DECIMAL(18, 2),
    @DEXMeterId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @DEXMeterId = Id
    FROM dbo.DEXMeter WITH (UPDLOCK, HOLDLOCK)
    WHERE MachineId = @MachineId
      AND DEXDateTime = @DEXDateTime;

    IF @DEXMeterId IS NULL
    BEGIN
        INSERT INTO dbo.DEXMeter
        (
            MachineId,
            DEXDateTime,
            MachineSerialNumber,
            ValueOfPaidVends
        )
        VALUES
        (
            @MachineId,
            @DEXDateTime,
            @MachineSerialNumber,
            @ValueOfPaidVends
        );

        SET @DEXMeterId = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.DEXMeter
        SET MachineSerialNumber = @MachineSerialNumber,
            ValueOfPaidVends = @ValueOfPaidVends,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE Id = @DEXMeterId;

        DELETE FROM dbo.DEXLaneMeter
        WHERE DEXMeterId = @DEXMeterId;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.SaveDEXLaneMeter
    @DEXMeterId INT,
    @ProductIdentifier NVARCHAR(6),
    @Price DECIMAL(18, 2),
    @NumberOfVends INT,
    @ValueOfPaidSales DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DEXLaneMeter
    (
        DEXMeterId,
        ProductIdentifier,
        Price,
        NumberOfVends,
        ValueOfPaidSales
    )
    VALUES
    (
        @DEXMeterId,
        @ProductIdentifier,
        @Price,
        @NumberOfVends,
        @ValueOfPaidSales
    );
END
GO
