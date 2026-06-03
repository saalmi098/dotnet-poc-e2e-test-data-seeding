IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'testdb')
    CREATE DATABASE testdb;
GO

USE testdb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Departments')
    CREATE TABLE Departments (
        Id       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
        Street   NVARCHAR(MAX)     NOT NULL,
        City     NVARCHAR(MAX)     NOT NULL,
        ZipCode  NVARCHAR(MAX)     NOT NULL
    );
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employees')
    CREATE TABLE Employees (
        Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
        Name         NVARCHAR(MAX)     NOT NULL,
        Email        NVARCHAR(MAX)     NOT NULL,
        DepartmentId INT               NULL,
        CONSTRAINT FK_Employees_Departments_DepartmentId
            FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL
    );
GO

IF NOT EXISTS (SELECT 1 FROM Departments)
BEGIN
    SET IDENTITY_INSERT Departments ON;
    INSERT INTO Departments (Id, Street, City, ZipCode) VALUES
        (1, 'Main Street', 'Springfield', '12345'),
        (2, 'Elm Street',  'Shelbyville', '54321'),
        (3, 'Oak Avenue',  'Ogdenville',  '67890');
    SET IDENTITY_INSERT Departments OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM Employees)
BEGIN
    SET IDENTITY_INSERT Employees ON;
    INSERT INTO Employees (Id, Name, Email, DepartmentId) VALUES
        (1, 'John Doe',      'john.doe@example.com',      1),
        (2, 'Jane Smith',    'jane.smith@example.com',    2),
        (3, 'Alice Johnson', 'alice.johnson@example.com', 3);
    SET IDENTITY_INSERT Employees OFF;
END
GO
