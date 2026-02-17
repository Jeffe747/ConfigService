
USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ConfigService')
BEGIN
    CREATE DATABASE ConfigService;
END
GO
