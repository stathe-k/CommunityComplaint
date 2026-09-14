/* ============================================================
   Community Complaint Reporting Website
   Database Script (SQL Server)
   ============================================================
   Run this in SSMS / sqlcmd to create the database and tables
   exactly as described in the project's Database Design section.
   If you are using EF Core Migrations instead (dotnet ef database
   update), you do NOT need to run this script — the migration
   will create an equivalent schema automatically.
   ============================================================ */

IF DB_ID('CommunityComplaintDB') IS NULL
BEGIN
    CREATE DATABASE CommunityComplaintDB;
END
GO

USE CommunityComplaintDB;
GO

-- ------------------------------------------------------------
-- Table: Users
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users (
    UserId        INT IDENTITY(1,1) PRIMARY KEY,
    Name          VARCHAR(100)  NOT NULL,
    MobileNumber  VARCHAR(15)   NOT NULL UNIQUE
);
GO

-- ------------------------------------------------------------
-- Table: Complaints
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Complaints', 'U') IS NOT NULL DROP TABLE dbo.Complaints;
GO
CREATE TABLE dbo.Complaints (
    ComplaintId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT           NOT NULL,
    Category        VARCHAR(50)   NOT NULL,
    Description     VARCHAR(MAX)  NOT NULL,
    Location        VARCHAR(255)  NOT NULL,
    Photo           VARCHAR(255)  NULL,
    Date            DATETIME      NOT NULL DEFAULT GETDATE(),
    Status          VARCHAR(20)   NOT NULL DEFAULT 'Pending',
    AssignedDept    VARCHAR(50)   NOT NULL DEFAULT 'Unassigned',
    CONSTRAINT FK_Complaints_Users FOREIGN KEY (UserId)
        REFERENCES dbo.Users (UserId) ON DELETE CASCADE,
    CONSTRAINT CK_Complaints_Status CHECK (Status IN ('Pending', 'In Progress', 'Resolved'))
);
GO

-- ------------------------------------------------------------
-- Table: Admins
-- Password column stores a hashed password (ASP.NET Core
-- PasswordHasher output), never plain text.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Admins', 'U') IS NOT NULL DROP TABLE dbo.Admins;
GO
CREATE TABLE dbo.Admins (
    AdminId    INT IDENTITY(1,1) PRIMARY KEY,
    Username   VARCHAR(50)   NOT NULL UNIQUE,
    Password   VARCHAR(255)  NOT NULL
);
GO

-- ------------------------------------------------------------
-- Helpful indexes for lookups used by the app
-- ------------------------------------------------------------
CREATE INDEX IX_Complaints_UserId ON dbo.Complaints (UserId);
CREATE INDEX IX_Complaints_Status ON dbo.Complaints (Status);
GO

/* ------------------------------------------------------------
   NOTE on seeding the admin account:
   Do not INSERT a plain-text password here. The application
   hashes the password with ASP.NET Core's PasswordHasher, and a
   plain-text row would never match at login. The app seeds a
   default admin automatically on first run (see
   Data/DbInitializer.cs):
       Username: admin
       Password: Admin@123
   Change this password after first login.
   ------------------------------------------------------------ */

/* ------------------------------------------------------------
   Optional sample data for manual testing (citizen + complaint)
   ------------------------------------------------------------ */
INSERT INTO dbo.Users (Name, MobileNumber) VALUES ('Yashshree Gite', '9876543210');

INSERT INTO dbo.Complaints (UserId, Category, Description, Location, Status, AssignedDept)
VALUES (
    (SELECT UserId FROM dbo.Users WHERE MobileNumber = '9876543210'),
    'Pothole',
    'Large pothole near the bus stop causing traffic issues.',
    'MG Road, Chh. Sambhajinagar',
    'Pending',
    'Unassigned'
);
GO
