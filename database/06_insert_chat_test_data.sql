/*=====================================================================
  Bastion - Global chat test data (6 of 6)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql
     4. 04_insert_leaderboard_demo.sql
     5. 05_create_friends.sql
     6. 06_insert_chat_test_data.sql     <- this file

  Additive script: it does not change the schema. The global chat keeps
  its history in the server memory and reads the banned words and the
  chat sanctions that already exist. The core test data has no chat
  sanction in effect, so this script adds one to pedro_peon that ends
  thirty days after the script runs. The step checks whether it already
  ran, so the script can be run again.

  Run it with an administration account. The file is UTF-8: run it with
  sqlcmd -f 65001.

  Contents:
     1. Chat sanction in effect
     2. Checks
=====================================================================*/

USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*---------------------------------------------------------------------
  1. Chat sanction in effect
---------------------------------------------------------------------*/

DECLARE @SanctionedId INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'pedro_peon');
DECLARE @ModeratorId INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'ana_modera');
DECLARE @Reason NVARCHAR(500) = N'Envió el mismo mensaje muchas veces en el chat global.';

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Sanction
    WHERE AccountId = @SanctionedId
      AND Scope = 'CHAT'
      AND Reason = @Reason)
BEGIN
    INSERT INTO dbo.Sanction (AccountId, ModeratorId, ReportId, Scope, SanctionType, Reason, StartedAt, EndedAt)
    VALUES (
        @SanctionedId,
        @ModeratorId,
        NULL,
        'CHAT',
        'TEMPORARY',
        @Reason,
        SYSUTCDATETIME(),
        DATEADD(DAY, 30, SYSUTCDATETIME()));
END;
GO

/*---------------------------------------------------------------------
  2. Checks
---------------------------------------------------------------------*/

-- 2.1 Chat sanctions in effect: only pedro_peon, ending in about thirty days.
SELECT a.Nickname, s.SanctionType, s.EndedAt
FROM dbo.Sanction AS s
JOIN dbo.Account AS a ON a.AccountId = s.AccountId
WHERE s.Scope = 'CHAT'
  AND s.RevokedAt IS NULL
  AND (s.EndedAt IS NULL OR s.EndedAt > SYSUTCDATETIME());

-- 2.2 Terms the global chat filters: tonto, idiota, estúpido, idiot and stupid.
SELECT Term, Language, Scope
FROM dbo.BannedWord
WHERE Scope IN ('CHAT', 'BOTH')
ORDER BY Term;
GO
