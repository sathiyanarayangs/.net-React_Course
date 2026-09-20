-- Task 4.4 — run in SSMS after CreateTables.sql.

CREATE PROCEDURE usp_GetStudentById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Name, Age, Email, EnrolledOn
    FROM Students
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE usp_InsertStudent
    @Name NVARCHAR(100),
    @Age INT,
    @Email NVARCHAR(200),
    @EnrolledOn DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Students (Name, Age, Email, EnrolledOn)
    VALUES (@Name, @Age, @Email, @EnrolledOn);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO
