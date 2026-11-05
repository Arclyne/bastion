/*=====================================================================
  Bastion - Private messages between friends (7 of 7)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql
     4. 04_insert_leaderboard_demo.sql
     5. 05_create_friends.sql
     6. 06_insert_chat_test_data.sql
     7. 07_create_private_messages.sql   <- this file

  Additive script: the FRIENDS channel of Message had no recipient, so a
  private message could not say who it was for. This script adds the
  recipient and the moment it was read, so a friend who is not connected
  reads the messages when they come back and the conversation stays
  visible after the friendship ends. The core test message of the
  FRIENDS channel (luis_quo, 1 September) is assigned to marta_muros.
  Every step checks whether it already ran, so the script can be run
  again.

  Run it with an administration account. The test data looks up the
  accounts by nickname and expects the friends of
  05_create_friends.sql. The file is UTF-8: run it with sqlcmd -f 65001.

  Contents:
     1. Message columns for private messages
     2. Rules and indexes
     3. Test data
     4. Checks
=====================================================================*/

USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.Message', N'RecipientId') IS NULL
    ALTER TABLE dbo.Message ADD RecipientId INT NULL;  -- Friend who receives the message; only in the FRIENDS channel.
GO
IF COL_LENGTH(N'dbo.Message', N'ReadAt') IS NULL
    ALTER TABLE dbo.Message ADD ReadAt DATETIME2(3) NULL;  -- Moment the recipient read it; null while unread. Only in the FRIENDS channel.
GO
IF COL_LENGTH(N'dbo.Message', N'PairLowId') IS NULL
    ALTER TABLE dbo.Message ADD
        PairLowId AS (CASE WHEN RecipientId IS NULL THEN NULL WHEN AuthorId < RecipientId THEN AuthorId ELSE RecipientId END) PERSISTED,  -- Smaller identifier of the conversation pair.
        PairHighId AS (CASE WHEN RecipientId IS NULL THEN NULL WHEN AuthorId < RecipientId THEN RecipientId ELSE AuthorId END) PERSISTED;  -- Larger identifier of the conversation pair.
GO
IF OBJECT_ID(N'dbo.FK_Message_Recipient', N'F') IS NULL
    ALTER TABLE dbo.Message ADD CONSTRAINT FK_Message_Recipient FOREIGN KEY (RecipientId) REFERENCES dbo.Account (AccountId);  -- 1:N | An account receives many private messages.
GO

UPDATE m
SET m.RecipientId = a.AccountId,
    m.ReadAt = m.SentAt
FROM dbo.Message AS m
JOIN dbo.Account AS a ON a.Nickname = N'marta_muros'
WHERE m.Channel = 'FRIENDS'
  AND m.RecipientId IS NULL;
GO

IF OBJECT_ID(N'dbo.CK_Message_Channel', N'C') IS NOT NULL
   AND OBJECT_DEFINITION(OBJECT_ID(N'dbo.CK_Message_Channel')) NOT LIKE N'%RecipientId%'
    ALTER TABLE dbo.Message DROP CONSTRAINT CK_Message_Channel;
GO
IF OBJECT_ID(N'dbo.CK_Message_Channel', N'C') IS NULL
    ALTER TABLE dbo.Message ADD CONSTRAINT CK_Message_Channel CHECK (
        (Channel IN ('MATCH','SPECTATORS') AND MatchId IS NOT NULL AND RoomId IS NULL AND RecipientId IS NULL)
        OR (Channel = 'ROOM' AND RoomId IS NOT NULL AND MatchId IS NULL AND RecipientId IS NULL)
        OR (Channel = 'GLOBAL' AND MatchId IS NULL AND RoomId IS NULL AND RecipientId IS NULL)
        OR (Channel = 'FRIENDS' AND MatchId IS NULL AND RoomId IS NULL
            AND RecipientId IS NOT NULL AND RecipientId <> AuthorId));  -- A private message has a recipient other than its author; the other channels have none.
GO
IF OBJECT_ID(N'dbo.CK_Message_ReadAt', N'C') IS NULL
    ALTER TABLE dbo.Message ADD CONSTRAINT CK_Message_ReadAt CHECK (ReadAt IS NULL OR (Channel = 'FRIENDS' AND ReadAt >= SentAt));  -- Only a private message is read, and never before it was sent.
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Message_Pair')
    CREATE INDEX IX_Message_Pair ON dbo.Message (PairLowId, PairHighId, SentAt) WHERE Channel = 'FRIENDS';  -- A conversation in order, whoever wrote each message.
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Message_Unread')
    CREATE INDEX IX_Message_Unread ON dbo.Message (RecipientId, AuthorId) WHERE Channel = 'FRIENDS' AND ReadAt IS NULL;  -- Unread messages per recipient and author.
GO

-- luis_quo talks with marta_muros (yesterday and today, two of her
-- messages unread), with sofia_salto (all read) and with clara_path,
-- who is not his friend, so that conversation is read only. Dates are
-- relative to the moment the script runs.

DECLARE @Luis INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'luis_quo');
DECLARE @Marta INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'marta_muros');
DECLARE @Sofia INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'sofia_salto');
DECLARE @Clara INT = (SELECT AccountId FROM dbo.Account WHERE Nickname = N'clara_path');
DECLARE @Now DATETIME2(3) = SYSUTCDATETIME();
DECLARE @Yesterday DATETIME2(3) = DATEADD(DAY, -1, @Now);

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Message
    WHERE Channel = 'FRIENDS'
      AND AuthorId = @Marta
      AND RecipientId = @Luis
      AND Text = N'buena partida! ese muro en la columna e me mató')
BEGIN
    INSERT INTO dbo.Message (AuthorId, Channel, RecipientId, Text, SentAt, ReadAt) VALUES
        (@Marta, 'FRIENDS', @Luis, N'buena partida! ese muro en la columna e me mató', DATEADD(MINUTE, -30, @Yesterday), DATEADD(MINUTE, -29, @Yesterday)),
        (@Luis, 'FRIENDS', @Marta, N'jaja lo vi venir desde el turno 10', DATEADD(MINUTE, -29, @Yesterday), DATEADD(MINUTE, -28, @Yesterday)),
        (@Marta, 'FRIENDS', @Luis, N'¿La revancha a las 7?', DATEADD(MINUTE, -20, @Now), NULL),
        (@Marta, 'FRIENDS', @Luis, N'Puedo con reloj de 10 min, el de 5 me pone nerviosa', DATEADD(MINUTE, -19, @Now), NULL),
        (@Sofia, 'FRIENDS', @Luis, N'gg', DATEADD(DAY, -8, @Now), DATEADD(DAY, -8, @Now)),
        (@Clara, 'FRIENDS', @Luis, N'¿me pasas la sala?', DATEADD(DAY, -12, @Now), DATEADD(DAY, -12, @Now));
END;
GO

-- 4.1 No message breaks the channel rules: 0 rows.
SELECT MessageId, Channel, MatchId, RoomId, RecipientId
FROM dbo.Message
WHERE (Channel = 'FRIENDS' AND RecipientId IS NULL)
   OR (Channel <> 'FRIENDS' AND RecipientId IS NOT NULL);

-- 4.2 Conversations of luis_quo: clara_path 1, marta_muros 5, sofia_salto 1.
SELECT other.Nickname, COUNT(*) AS Messages, SUM(IIF(m.ReadAt IS NULL AND m.RecipientId = luis.AccountId, 1, 0)) AS UnreadByLuis
FROM dbo.Message AS m
JOIN dbo.Account AS luis ON luis.Nickname = N'luis_quo'
JOIN dbo.Account AS other ON other.AccountId = IIF(m.AuthorId = luis.AccountId, m.RecipientId, m.AuthorId)
WHERE m.Channel = 'FRIENDS'
  AND luis.AccountId IN (m.AuthorId, m.RecipientId)
GROUP BY other.Nickname
ORDER BY other.Nickname;
GO
