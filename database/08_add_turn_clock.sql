/*=====================================================================
  Bastion - Turn clock (8 of 8)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql
     4. 04_insert_leaderboard_demo.sql
     5. 05_create_friends.sql
     6. 06_insert_chat_test_data.sql
     7. 07_create_private_messages.sql
     8. 08_add_turn_clock.sql   <- this file

  Additive script: the clock of the player who has the turn ran from
  the last move, so a turn that changed because a player left or ran
  out of time was charged to the next player. This script adds the
  moment the current turn started. Matches already stored take the
  moment of their last move, or the end of the three-second versus
  countdown when they have no moves. Every step checks whether it
  already ran, so the script can be run again.

  Run it with an administration account.

  Contents:
     1. Match column for the start of the turn
     2. Start of the turn of stored matches
=====================================================================*/

USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.Match', N'TurnStartedAt') IS NULL
    ALTER TABLE dbo.Match ADD TurnStartedAt DATETIME2(3) NULL;  -- Moment the current turn started: the end of the versus countdown, the last move, or the moment the player with the turn left. The clock of that player runs from here.
GO

UPDATE m
SET m.TurnStartedAt = COALESCE(
        (SELECT MAX(mv.PlayedAt) FROM dbo.Move AS mv WHERE mv.MatchId = m.MatchId),
        DATEADD(SECOND, 3, m.StartedAt))
FROM dbo.Match AS m
WHERE m.TurnStartedAt IS NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Match') AND name = N'TurnStartedAt' AND is_nullable = 1)
    ALTER TABLE dbo.Match ALTER COLUMN TurnStartedAt DATETIME2(3) NOT NULL;
GO
IF OBJECT_ID(N'dbo.DF_Match_TurnStartedAt', N'D') IS NULL
    ALTER TABLE dbo.Match ADD CONSTRAINT DF_Match_TurnStartedAt DEFAULT (DATEADD(SECOND, 3, SYSUTCDATETIME())) FOR TurnStartedAt;  -- A match inserted without it starts its first turn after the versus countdown.
GO
