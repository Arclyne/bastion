/*=====================================================================
  Bastion - Database creation (1 of 3)
  Engine: SQL Server 2019 or later (CON-04); tested on SQL Server 2025.

  Run order:
     1. 01_create_database.sql    <- this file
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql

  Run it with an account that can create databases and logins
  (sysadmin), not with BastionServerConnection. If the Bastion database
  already exists, it is dropped and recreated empty: this is an
  installation script, not an upgrade script.

  The login password is never stored in this file. It is a sqlcmd
  variable that must be supplied when the script runs:
     sqlcmd -S <server> -U <admin> -i 01_create_database.sql -v BastionServerPassword="..."
  In SQL Server Management Studio or Azure Data Studio, enable SQLCMD
  mode and add :setvar BastionServerPassword "..." before running it.
  The same password then goes into the server's connection string,
  kept in an environment variable or user-secrets.
=====================================================================*/

/*---------------------------------------------------------------------
  1. Database
  UTF-8 encoding (D-21). The Modern_Spanish_100_CI_AS_SC_UTF8 collation
  is Spanish, case-insensitive and accent-sensitive, and it sets the
  text encoding:
    - UTF8: VARCHAR columns and literals without the N prefix store
      UTF-8 text, so they accept ñ, accented letters and any Unicode
      character, not just those in code page 1252. NVARCHAR columns
      store UTF-16 with any collation.
    - SC: supplementary characters, such as emojis, count as one
      character in LEN, SUBSTRING and length limits.
  Columns that must also be compared ignoring accents, such as
  Nickname, declare their own collation, also _SC_UTF8.
---------------------------------------------------------------------*/

USE master;
GO
IF DB_ID(N'Bastion') IS NOT NULL
BEGIN
    ALTER DATABASE Bastion SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Bastion;
END
GO
CREATE DATABASE Bastion COLLATE Modern_Spanish_100_CI_AS_SC_UTF8;
GO

/*---------------------------------------------------------------------
  2. Server connection user
  The game server connects with this user. It can only read, insert,
  update and execute procedures across the whole database: it cannot
  delete rows, create or alter objects, or grant permissions.
---------------------------------------------------------------------*/

USE master;
GO
IF SUSER_ID(N'BastionServerConnection') IS NULL
    CREATE LOGIN BastionServerConnection
        WITH PASSWORD = N'$(BastionServerPassword)',
             DEFAULT_DATABASE = Bastion,
             CHECK_POLICY = ON,
             CHECK_EXPIRATION = OFF;
GO

USE Bastion;
GO
IF USER_ID(N'BastionServerConnection') IS NULL
    CREATE USER BastionServerConnection FOR LOGIN BastionServerConnection WITH DEFAULT_SCHEMA = dbo;
GO

-- Database-level permissions: they cover every table, view and procedure,
-- present and future, in every schema.
GRANT SELECT, INSERT, UPDATE, EXECUTE ON DATABASE::Bastion TO BastionServerConnection;
GO

-- Check: it must list CONNECT (implicit when the user is created),
-- SELECT, INSERT, UPDATE and EXECUTE, all with DATABASE scope and GRANT state.
SELECT pr.name            AS PrincipalName,
       pe.permission_name AS PermissionName,
       pe.state_desc      AS PermissionState,
       pe.class_desc      AS PermissionScope
FROM sys.database_permissions AS pe
JOIN sys.database_principals  AS pr ON pr.principal_id = pe.grantee_principal_id
WHERE pr.name = N'BastionServerConnection'
ORDER BY pe.permission_name;
GO
