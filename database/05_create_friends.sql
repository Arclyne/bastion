/*=====================================================================
  Bastion - Friends (5 of 5)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql
     4. 04_insert_leaderboard_demo.sql
     5. 05_create_friends.sql            <- this file

  Additive script: it changes a database that already has the core
  tables and their test data, so nobody has to recreate it. Friendship
  and FriendRequest were later-phase tables (D-22); the scope review
  moved them to the core because CU-22, CU-23 and CU-25 write them
  (D-25). It also adds the friend code and the last-seen moment to
  Account. Every step checks whether it already ran, so the script can
  be run again.

  Run it with an administration account. The test data looks up the
  accounts by nickname and expects the demonstration players of
  04_insert_leaderboard_demo.sql. The file is UTF-8: run it with
  sqlcmd -f 65001.

  Contents:
     1. Account columns for friends
     2. Relationships between players
     3. Procedures for physical deletes
     4. Test data
     5. Checks
=====================================================================*/

USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- 1. Account columns for friends

IF COL_LENGTH(N'dbo.Account', N'FriendCode') IS NULL
    ALTER TABLE dbo.Account ADD FriendCode CHAR(6) COLLATE Latin1_General_100_BIN2 NULL;  -- Public code to find a player without their nickname, six letters and digits without 0, O, 1, I or L, shown as XX-XXXX (CU-22 RN-05).
GO
IF COL_LENGTH(N'dbo.Account', N'LastSeenAt') IS NULL
    ALTER TABLE dbo.Account ADD LastSeenAt DATETIME2(0) NULL;  -- Last time the player left the game; shown to friends while disconnected (F-40). It records presence and is not derived from Session.
GO

BEGIN TRANSACTION;

UPDATE a
SET a.FriendCode = c.FriendCode
FROM dbo.Account AS a
JOIN (VALUES
    (N'admin_bastion', 'AD2M9N'),
    (N'ana_modera', 'AN4M3D'),
    (N'luis_quo', 'QU7K2P'),
    (N'marta_muros', 'MA3R7T'),
    (N'pedro_peon', 'PE8D4R'),
    (N'sofia_salto', 'SF5T2Z'),
    (N'Invitado_4821', 'GU4829'),
    (N'nico_nuevo', 'NC6V3W'),
    (N'rayo_veloz', 'RY9V2Z'),
    (N'anonimo_10', 'ZD2X7Q'),
    (N'4dm1n_oficial', 'BN3F8K'),
    (N'walter_wall', 'WA2W3T'),
    (N'nora_north', 'NR4N5H'),
    (N'ivan_ice', 'VN6C7E'),
    (N'clara_path', 'CP8T9H'),
    (N'diego_jump', 'DG2J3M'),
    (N'elena_fence', 'EF4N5C'),
    (N'hugo_block', 'HG6B7K'),
    (N'irene_maze', 'RZ8M9E'),
    (N'jorge_step', 'JR2S3P'),
    (N'karla_gate', 'KR4G5T'),
    (N'leo_tower', 'TW6R7E'),
    (N'mia_corner', 'MC8R9N'),
    (N'oscar_line', 'SC2N3E'),
    (N'paula_bridge', 'PB4R5G'),
    (N'raul_rush', 'RR6H7S')) AS c (Nickname, FriendCode)
    ON a.Nickname = c.Nickname
WHERE a.FriendCode IS NULL
    AND NOT EXISTS (SELECT 1 FROM dbo.Account AS taken WHERE taken.FriendCode = c.FriendCode);

DECLARE @Alphabet CHAR(31) = 'ABCDEFGHJKMNPQRSTUVWXYZ23456789';
DECLARE @AccountId INT;
DECLARE @Code CHAR(6);
DECLARE @Position INT;

WHILE EXISTS (SELECT 1 FROM dbo.Account WHERE FriendCode IS NULL)
BEGIN
    SELECT TOP (1) @AccountId = AccountId FROM dbo.Account WHERE FriendCode IS NULL ORDER BY AccountId;
    SET @Code = '';
    SET @Position = 0;
    WHILE @Position < 6
    BEGIN
        SET @Code = RTRIM(@Code) + SUBSTRING(@Alphabet, ABS(CHECKSUM(NEWID())) % 31 + 1, 1);
        SET @Position += 1;
    END;
    IF NOT EXISTS (SELECT 1 FROM dbo.Account WHERE FriendCode = @Code)
        UPDATE dbo.Account SET FriendCode = @Code WHERE AccountId = @AccountId;
END;

UPDATE a
SET a.LastSeenAt = s.LastSeenAt
FROM dbo.Account AS a
JOIN (
    SELECT AccountId, MAX(COALESCE(EndedAt, LastUsedAt)) AS LastSeenAt
    FROM dbo.Session
    GROUP BY AccountId) AS s
    ON a.AccountId = s.AccountId
WHERE a.LastSeenAt IS NULL;

COMMIT TRANSACTION;
GO

IF COLUMNPROPERTY(OBJECT_ID(N'dbo.Account'), N'FriendCode', 'AllowsNull') = 1
    ALTER TABLE dbo.Account ALTER COLUMN FriendCode CHAR(6) COLLATE Latin1_General_100_BIN2 NOT NULL;
GO
IF OBJECT_ID(N'dbo.UQ_Account_FriendCode', N'UQ') IS NULL
    ALTER TABLE dbo.Account ADD CONSTRAINT UQ_Account_FriendCode UNIQUE (FriendCode);  -- The friend code identifies one account (CU-22 RN-05).
GO
IF OBJECT_ID(N'dbo.CK_Account_FriendCode', N'C') IS NULL
    ALTER TABLE dbo.Account ADD CONSTRAINT CK_Account_FriendCode CHECK (
        LEN(FriendCode) = 6 AND FriendCode NOT LIKE '%[^ABCDEFGHJKMNPQRSTUVWXYZ23456789]%');  -- Only the characters that cannot be mistaken for one another.
GO

-- 2. Relationships between players

-- @entity Friendship | Mutual friendship between two registered players; one row per pair (CU-22, CU-23, CU-25).
-- @normalization Composite key on the ordered pair; `BefriendedAt` depends on the whole pair. Storing the smaller identifier first makes (1,2) and (2,1) the same row (CU-23 RN-03).
IF OBJECT_ID(N'dbo.Friendship', N'U') IS NULL
CREATE TABLE dbo.Friendship (
    AccountAId           INT            NOT NULL,  -- Smaller account identifier of the pair.
    AccountBId           INT            NOT NULL,  -- Larger account identifier of the pair.
    BefriendedAt         DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment the request was accepted.
    CONSTRAINT PK_Friendship PRIMARY KEY (AccountAId, AccountBId),
    CONSTRAINT FK_Friendship_AccountA FOREIGN KEY (AccountAId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account is the smaller member of many friendships.
    CONSTRAINT FK_Friendship_AccountB FOREIGN KEY (AccountBId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account is the larger member of many friendships.
    CONSTRAINT CK_Friendship_Order CHECK (AccountAId < AccountBId)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Friendship_AccountB')
    CREATE INDEX IX_Friendship_AccountB ON dbo.Friendship (AccountBId);
GO

-- @entity FriendRequest | Pending friend request; the row is deleted when it is accepted, rejected or cancelled (CU-23 RN-06).
-- @normalization Composite key; validity is 30 days from `RequestedAt`, computed when queried and never stored (CU-23 RN-05). The ordered-pair columns are computed only to allow one request per pair in either direction (CU-22 RN-03).
IF OBJECT_ID(N'dbo.FriendRequest', N'U') IS NULL
CREATE TABLE dbo.FriendRequest (
    RequesterId          INT            NOT NULL,  -- Player who sends the request; only they cancel it.
    RecipientId          INT            NOT NULL,  -- Player who receives it; only they accept or reject it (CU-23 RN-01).
    RequestedAt          DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of sending; the request expires 30 days later.
    PairLowId            AS (CASE WHEN RequesterId < RecipientId THEN RequesterId ELSE RecipientId END) PERSISTED,  -- Smaller identifier of the pair.
    PairHighId           AS (CASE WHEN RequesterId < RecipientId THEN RecipientId ELSE RequesterId END) PERSISTED,  -- Larger identifier of the pair.
    CONSTRAINT PK_FriendRequest PRIMARY KEY (RequesterId, RecipientId),
    CONSTRAINT FK_FriendRequest_Requester FOREIGN KEY (RequesterId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account sends many requests (requester role).
    CONSTRAINT FK_FriendRequest_Recipient FOREIGN KEY (RecipientId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account receives many requests (recipient role).
    CONSTRAINT CK_FriendRequest_Distinct CHECK (RequesterId <> RecipientId)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_FriendRequest_Pair')
    CREATE UNIQUE INDEX UX_FriendRequest_Pair ON dbo.FriendRequest (PairLowId, PairHighId);  -- One pending request per pair, whoever sent it (CU-22 RN-03).
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FriendRequest_Recipient')
    CREATE INDEX IX_FriendRequest_Recipient ON dbo.FriendRequest (RecipientId) INCLUDE (RequestedAt);
GO

-- 3. Procedures for physical deletes
-- The connection user has no DELETE permission (01_create_database.sql);
-- the EXECUTE permission granted at database scope covers these.

-- CU-23: the request is deleted when it is accepted, rejected or cancelled; CU-22 also deletes an expired one before sending a new one.
CREATE OR ALTER PROCEDURE dbo.usp_FriendRequest_Delete
    @RequesterId INT,
    @RecipientId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.FriendRequest WHERE RequesterId = @RequesterId AND RecipientId = @RecipientId;
    SELECT @@ROWCOUNT AS DeletedRows;
END;
GO

-- CU-25: the friendship ends for both players.
CREATE OR ALTER PROCEDURE dbo.usp_Friendship_Delete
    @AccountAId INT,
    @AccountBId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Friendship WHERE AccountAId = @AccountAId AND AccountBId = @AccountBId;
    SELECT @@ROWCOUNT AS DeletedRows;
END;
GO

-- 4. Test data
-- luis_quo has six friends and marta_muros four; marta_muros and
-- pedro_peon are not friends. Request dates are relative to the moment
-- the script runs: hugo_block's request expires one day later and
-- irene_maze's has already expired.

BEGIN TRANSACTION;

INSERT INTO dbo.Friendship (AccountAId, AccountBId, BefriendedAt)
SELECT
    IIF(first.AccountId < second.AccountId, first.AccountId, second.AccountId),
    IIF(first.AccountId < second.AccountId, second.AccountId, first.AccountId),
    p.BefriendedAt
FROM (VALUES
    (N'luis_quo', N'marta_muros', CAST('2026-08-10T19:00:00' AS DATETIME2(0))),
    (N'luis_quo', N'pedro_peon', CAST('2026-08-12T20:30:00' AS DATETIME2(0))),
    (N'luis_quo', N'sofia_salto', CAST('2026-08-15T18:10:00' AS DATETIME2(0))),
    (N'luis_quo', N'walter_wall', CAST('2026-08-20T21:00:00' AS DATETIME2(0))),
    (N'luis_quo', N'nora_north', CAST('2026-08-22T17:45:00' AS DATETIME2(0))),
    (N'luis_quo', N'ivan_ice', CAST('2026-08-25T16:20:00' AS DATETIME2(0))),
    (N'marta_muros', N'sofia_salto', CAST('2026-08-11T22:00:00' AS DATETIME2(0))),
    (N'marta_muros', N'clara_path', CAST('2026-08-18T19:30:00' AS DATETIME2(0))),
    (N'marta_muros', N'diego_jump', CAST('2026-08-27T20:15:00' AS DATETIME2(0))),
    (N'pedro_peon', N'sofia_salto', CAST('2026-08-14T15:00:00' AS DATETIME2(0)))) AS p (FirstNickname, SecondNickname, BefriendedAt)
JOIN dbo.Account AS first ON first.Nickname = p.FirstNickname
JOIN dbo.Account AS second ON second.Nickname = p.SecondNickname
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Friendship AS f
    WHERE f.AccountAId = IIF(first.AccountId < second.AccountId, first.AccountId, second.AccountId)
        AND f.AccountBId = IIF(first.AccountId < second.AccountId, second.AccountId, first.AccountId));

INSERT INTO dbo.FriendRequest (RequesterId, RecipientId, RequestedAt)
SELECT requester.AccountId, recipient.AccountId, DATEADD(DAY, -r.DaysAgo, SYSUTCDATETIME())
FROM (VALUES
    (N'elena_fence', N'luis_quo', 2),
    (N'hugo_block', N'luis_quo', 29),
    (N'irene_maze', N'luis_quo', 31),
    (N'luis_quo', N'jorge_step', 5),
    (N'luis_quo', N'karla_gate', 12),
    (N'pedro_peon', N'marta_muros', 1),
    (N'mia_corner', N'marta_muros', 3),
    (N'marta_muros', N'leo_tower', 7)) AS r (RequesterNickname, RecipientNickname, DaysAgo)
JOIN dbo.Account AS requester ON requester.Nickname = r.RequesterNickname
JOIN dbo.Account AS recipient ON recipient.Nickname = r.RecipientNickname
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.FriendRequest AS existing
    WHERE (existing.RequesterId = requester.AccountId AND existing.RecipientId = recipient.AccountId)
        OR (existing.RequesterId = recipient.AccountId AND existing.RecipientId = requester.AccountId));

COMMIT TRANSACTION;
GO

-- 5. Checks
-- Each query must return the value its comment states.

-- 5.1 Every account has a friend code, and none repeats: 0 rows.
SELECT FriendCode, COUNT(*) AS Accounts
FROM dbo.Account
GROUP BY FriendCode
HAVING FriendCode IS NULL OR COUNT(*) > 1;

-- 5.2 Friends per player: luis_quo 6, marta_muros 4, pedro_peon 2, sofia_salto 3.
SELECT a.Nickname, COUNT(*) AS Friends
FROM dbo.Account AS a
JOIN dbo.Friendship AS f ON a.AccountId IN (f.AccountAId, f.AccountBId)
GROUP BY a.Nickname
ORDER BY Friends DESC, a.Nickname;

-- 5.3 Pending and expired requests per recipient: luis_quo 2 pending and 1 expired, marta_muros 2 pending.
SELECT
    a.Nickname,
    SUM(IIF(r.RequestedAt > DATEADD(DAY, -30, SYSUTCDATETIME()), 1, 0)) AS Pending,
    SUM(IIF(r.RequestedAt <= DATEADD(DAY, -30, SYSUTCDATETIME()), 1, 0)) AS Expired
FROM dbo.FriendRequest AS r
JOIN dbo.Account AS a ON a.AccountId = r.RecipientId
GROUP BY a.Nickname
ORDER BY a.Nickname;

-- 5.4 No pair is both friends and pending: 0 rows.
SELECT r.RequesterId, r.RecipientId
FROM dbo.FriendRequest AS r
JOIN dbo.Friendship AS f ON f.AccountAId = r.PairLowId AND f.AccountBId = r.PairHighId;
GO
