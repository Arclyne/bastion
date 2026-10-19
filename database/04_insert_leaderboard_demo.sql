/*=====================================================================
  Bastion - Leaderboard demonstration data (4 of 4)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql
     4. 04_insert_leaderboard_demo.sql   <- this file (optional)

  Adds fifteen demonstration players with enough ranked matches to fill
  the leaderboard (CU-35 RN-01 asks for five in a mode). Their statistics
  are typed in directly, without the matches behind them, so the
  consistency checks at the end of 03_insert_test_data.sql do not apply
  to these rows. They have no password and cannot sign in.

  Run it with an administration account, after 03_insert_test_data.sql.
=====================================================================*/

USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

INSERT INTO dbo.Account (AccountType, AccountStatus, Role, Nickname, Email, BirthDate, PreferredLanguage, RegisteredAt, TermsVersion, TermsLanguage, TermsAcceptedAt) VALUES
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'walter_wall', N'walter_wall@demo.bastion.test', '2000-01-10', N'en', '2026-07-01T12:00:00', N'2026.2', N'en', '2026-07-01T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'nora_north', N'nora_north@demo.bastion.test', '2001-02-11', N'es-MX', '2026-07-02T12:00:00', N'2026.2', N'es-MX', '2026-07-02T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'ivan_ice', N'ivan_ice@demo.bastion.test', '2002-03-12', N'es-MX', '2026-07-03T12:00:00', N'2026.2', N'es-MX', '2026-07-03T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'clara_path', N'clara_path@demo.bastion.test', '2003-04-13', N'es-MX', '2026-07-04T12:00:00', N'2026.2', N'es-MX', '2026-07-04T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'diego_jump', N'diego_jump@demo.bastion.test', '2004-05-14', N'en', '2026-07-05T12:00:00', N'2026.2', N'en', '2026-07-05T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'elena_fence', N'elena_fence@demo.bastion.test', '2005-06-15', N'es-MX', '2026-07-06T12:00:00', N'2026.2', N'es-MX', '2026-07-06T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'hugo_block', N'hugo_block@demo.bastion.test', '2006-07-16', N'es-MX', '2026-07-07T12:00:00', N'2026.2', N'es-MX', '2026-07-07T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'irene_maze', N'irene_maze@demo.bastion.test', '2007-08-17', N'es-MX', '2026-07-08T12:00:00', N'2026.2', N'es-MX', '2026-07-08T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'jorge_step', N'jorge_step@demo.bastion.test', '2008-09-18', N'en', '2026-07-09T12:00:00', N'2026.2', N'en', '2026-07-09T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'karla_gate', N'karla_gate@demo.bastion.test', '2000-01-10', N'es-MX', '2026-07-10T12:00:00', N'2026.2', N'es-MX', '2026-07-10T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'leo_tower', N'leo_tower@demo.bastion.test', '2001-02-11', N'es-MX', '2026-07-11T12:00:00', N'2026.2', N'es-MX', '2026-07-11T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'mia_corner', N'mia_corner@demo.bastion.test', '2002-03-12', N'es-MX', '2026-07-12T12:00:00', N'2026.2', N'es-MX', '2026-07-12T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'oscar_line', N'oscar_line@demo.bastion.test', '2003-04-13', N'en', '2026-07-13T12:00:00', N'2026.2', N'en', '2026-07-13T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'paula_bridge', N'paula_bridge@demo.bastion.test', '2004-05-14', N'es-MX', '2026-07-14T12:00:00', N'2026.2', N'es-MX', '2026-07-14T12:00:00'),
    (N'REGISTERED', N'ACTIVE', N'PLAYER', N'raul_rush', N'raul_rush@demo.bastion.test', '2005-06-15', N'es-MX', '2026-07-15T12:00:00', N'2026.2', N'es-MX', '2026-07-15T12:00:00');

-- One statistics row per mode, as a new account gets (CU-02); only some reach five ranked matches.
INSERT INTO dbo.ModeStatistic (AccountId, GameModeId, EloRating, LastMatchAt, MatchesPlayed, MatchesWon, MatchesLost, Abandonments)
SELECT AccountId, 1, 1221, '2026-09-05T14:27:00', 40, 21, 4, 1 FROM dbo.Account WHERE Nickname = N'walter_wall'
UNION ALL SELECT AccountId, 2, 1104, '2026-09-26T21:58:00', 8, 8, 0, 0 FROM dbo.Account WHERE Nickname = N'walter_wall'
UNION ALL SELECT AccountId, 3, 872, '2026-09-04T21:31:00', 28, 0, 16, 1 FROM dbo.Account WHERE Nickname = N'walter_wall'
UNION ALL SELECT AccountId, 1, 992, '2026-09-01T13:20:00', 6, 2, 3, 0 FROM dbo.Account WHERE Nickname = N'nora_north'
UNION ALL SELECT AccountId, 2, 1009, '2026-09-16T15:05:00', 1, 1, 0, 0 FROM dbo.Account WHERE Nickname = N'nora_north'
UNION ALL SELECT AccountId, 3, 1000, '2026-09-24T17:52:00', 1, 0, 0, 0 FROM dbo.Account WHERE Nickname = N'nora_north'
UNION ALL SELECT AccountId, 1, 1042, '2026-09-02T14:49:00', 25, 12, 9, 2 FROM dbo.Account WHERE Nickname = N'ivan_ice'
UNION ALL SELECT AccountId, 2, 1000, '2026-09-18T17:06:00', 3, 0, 0, 0 FROM dbo.Account WHERE Nickname = N'ivan_ice'
UNION ALL SELECT AccountId, 3, 1090, '2026-09-13T20:00:00', 29, 12, 6, 1 FROM dbo.Account WHERE Nickname = N'ivan_ice'
UNION ALL SELECT AccountId, 1, 950, '2026-09-01T20:05:00', 19, 7, 12, 0 FROM dbo.Account WHERE Nickname = N'clara_path'
UNION ALL SELECT AccountId, 2, 1088, '2026-09-05T21:57:00', 12, 11, 0, 0 FROM dbo.Account WHERE Nickname = N'clara_path'
UNION ALL SELECT AccountId, 3, 1060, '2026-09-17T22:10:00', 4, 4, 0, 0 FROM dbo.Account WHERE Nickname = N'clara_path'
UNION ALL SELECT AccountId, 1, 1238, '2026-09-26T10:44:00', 39, 28, 11, 1 FROM dbo.Account WHERE Nickname = N'diego_jump'
UNION ALL SELECT AccountId, 2, 1064, '2026-09-17T12:21:00', 4, 4, 0, 0 FROM dbo.Account WHERE Nickname = N'diego_jump'
UNION ALL SELECT AccountId, 3, 1182, '2026-09-25T18:05:00', 23, 15, 1, 1 FROM dbo.Account WHERE Nickname = N'diego_jump'
UNION ALL SELECT AccountId, 1, 1360, '2026-09-20T20:29:00', 34, 24, 0, 0 FROM dbo.Account WHERE Nickname = N'elena_fence'
UNION ALL SELECT AccountId, 2, 1015, '2026-09-04T13:58:00', 1, 1, 0, 0 FROM dbo.Account WHERE Nickname = N'elena_fence'
UNION ALL SELECT AccountId, 3, 1000, NULL, 0, 0, 0, 0 FROM dbo.Account WHERE Nickname = N'elena_fence'
UNION ALL SELECT AccountId, 1, 1048, '2026-09-06T19:51:00', 27, 14, 8, 2 FROM dbo.Account WHERE Nickname = N'hugo_block'
UNION ALL SELECT AccountId, 2, 1080, '2026-09-04T21:33:00', 11, 7, 2, 0 FROM dbo.Account WHERE Nickname = N'hugo_block'
UNION ALL SELECT AccountId, 3, 1250, '2026-09-07T16:13:00', 39, 28, 3, 1 FROM dbo.Account WHERE Nickname = N'hugo_block'
UNION ALL SELECT AccountId, 1, 1210, '2026-09-15T22:12:00', 19, 17, 2, 0 FROM dbo.Account WHERE Nickname = N'irene_maze'
UNION ALL SELECT AccountId, 2, 952, '2026-09-07T19:44:00', 3, 0, 3, 0 FROM dbo.Account WHERE Nickname = N'irene_maze'
UNION ALL SELECT AccountId, 3, 1000, '2026-09-19T14:14:00', 4, 2, 2, 2 FROM dbo.Account WHERE Nickname = N'irene_maze'
UNION ALL SELECT AccountId, 1, 1072, '2026-09-18T12:03:00', 17, 11, 2, 1 FROM dbo.Account WHERE Nickname = N'jorge_step'
UNION ALL SELECT AccountId, 2, 1009, '2026-09-13T19:49:00', 2, 1, 0, 0 FROM dbo.Account WHERE Nickname = N'jorge_step'
UNION ALL SELECT AccountId, 3, 916, '2026-09-01T15:16:00', 8, 0, 6, 0 FROM dbo.Account WHERE Nickname = N'jorge_step'
UNION ALL SELECT AccountId, 1, 1165, '2026-09-16T11:19:00', 13, 11, 0, 0 FROM dbo.Account WHERE Nickname = N'karla_gate'
UNION ALL SELECT AccountId, 2, 1192, '2026-09-19T18:05:00', 34, 25, 1, 0 FROM dbo.Account WHERE Nickname = N'karla_gate'
UNION ALL SELECT AccountId, 3, 1032, '2026-09-25T19:02:00', 2, 2, 0, 0 FROM dbo.Account WHERE Nickname = N'karla_gate'
UNION ALL SELECT AccountId, 1, 1039, '2026-09-13T22:27:00', 26, 14, 11, 0 FROM dbo.Account WHERE Nickname = N'leo_tower'
UNION ALL SELECT AccountId, 2, 1000, NULL, 0, 0, 0, 0 FROM dbo.Account WHERE Nickname = N'leo_tower'
UNION ALL SELECT AccountId, 3, 1108, '2026-09-13T14:42:00', 17, 13, 1, 0 FROM dbo.Account WHERE Nickname = N'leo_tower'
UNION ALL SELECT AccountId, 1, 1130, '2026-09-09T17:17:00', 27, 18, 5, 0 FROM dbo.Account WHERE Nickname = N'mia_corner'
UNION ALL SELECT AccountId, 2, 1013, '2026-09-03T21:30:00', 1, 1, 0, 0 FROM dbo.Account WHERE Nickname = N'mia_corner'
UNION ALL SELECT AccountId, 3, 1000, '2026-09-17T12:41:00', 3, 1, 1, 1 FROM dbo.Account WHERE Nickname = N'mia_corner'
UNION ALL SELECT AccountId, 1, 945, '2026-09-09T18:14:00', 19, 1, 6, 0 FROM dbo.Account WHERE Nickname = N'oscar_line'
UNION ALL SELECT AccountId, 2, 1120, '2026-09-15T12:44:00', 39, 11, 1, 1 FROM dbo.Account WHERE Nickname = N'oscar_line'
UNION ALL SELECT AccountId, 3, 1060, '2026-09-25T10:26:00', 40, 20, 16, 2 FROM dbo.Account WHERE Nickname = N'oscar_line'
UNION ALL SELECT AccountId, 1, 1090, '2026-09-20T15:17:00', 24, 10, 4, 2 FROM dbo.Account WHERE Nickname = N'paula_bridge'
UNION ALL SELECT AccountId, 2, 990, '2026-09-13T13:49:00', 1, 0, 1, 0 FROM dbo.Account WHERE Nickname = N'paula_bridge'
UNION ALL SELECT AccountId, 3, 1000, '2026-09-07T12:10:00', 4, 0, 0, 0 FROM dbo.Account WHERE Nickname = N'paula_bridge'
UNION ALL SELECT AccountId, 1, 1080, '2026-09-11T13:37:00', 22, 15, 7, 0 FROM dbo.Account WHERE Nickname = N'raul_rush'
UNION ALL SELECT AccountId, 2, 1042, '2026-09-25T19:06:00', 3, 3, 0, 0 FROM dbo.Account WHERE Nickname = N'raul_rush'
UNION ALL SELECT AccountId, 3, 922, '2026-09-10T10:08:00', 31, 8, 14, 1 FROM dbo.Account WHERE Nickname = N'raul_rush';

COMMIT TRANSACTION;
GO

-- Check: the classic ranking as the server builds it (CU-35 RN-01, RN-06).
SELECT TOP (10)
       a.Nickname,
       s.EloRating,
       s.MatchesPlayed,
       s.MatchesWon
FROM dbo.ModeStatistic AS s
JOIN dbo.Account AS a ON a.AccountId = s.AccountId
JOIN dbo.GameMode AS m ON m.GameModeId = s.GameModeId
WHERE m.Code = 'CLASSIC'
  AND s.MatchesPlayed >= 5
  AND a.AccountStatus = 'ACTIVE'
ORDER BY s.EloRating DESC, s.LastMatchAt;
GO
