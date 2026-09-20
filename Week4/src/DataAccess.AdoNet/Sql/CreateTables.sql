-- Task 4.1 — run in SSMS against a real SQL Server instance.
-- 3NF: Students and Teachers are independent entities (no repeating groups,
-- every non-key column depends only on the table's own primary key); Users
-- is a separate table so authentication data never mixes with domain data.

CREATE TABLE Students (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Age         INT NOT NULL,
    Email       NVARCHAR(200) NOT NULL,
    EnrolledOn  DATETIME2 NULL  -- added later by Task 4.8's second migration; present here for the ADO.NET/DB-First layers to match EF's evolved schema
);

CREATE TABLE Teachers (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Name    NVARCHAR(100) NOT NULL,
    Subject NVARCHAR(100) NOT NULL
);

CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(300) NOT NULL,
    Role         NVARCHAR(20) NOT NULL
);

-- Task 4.6's unique Email index (EF Fluent API) mirrored here in raw SQL,
-- so the ADO.NET and DB First layers enforce the same constraint EF does.
CREATE UNIQUE INDEX UX_Students_Email ON Students(Email);
CREATE UNIQUE INDEX UX_Users_Username ON Users(Username);
