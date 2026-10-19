/*=====================================================================
  Bastion - Core test data (3 of 3)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql
     3. 03_insert_test_data.sql   <- this file

  English version of the team's test data scenario. Table names,
  column names and domain codes follow 02_create_tables.sql; the text
  that players and moderators typed (nicknames, emails, chat messages,
  report and appeal texts, notes) is kept as it was written.

  Loads the core catalogs and the part of the test scenario that fits
  in the 19 core tables. It is a snapshot of the database on 2026-09-01
  18:00:00 UTC. What the scenario has for the additions (items, coins,
  boxes, divisions, friends, the match against the AI) is loaded later
  with the later-phase test data script (D-22).
  Whatever the model stores as a consequence of other rows (elo,
  statistics, board state) was calculated from them, and the queries
  at the end check it.

  Run it with an administration account on the freshly created, empty
  tables: it sets the identifiers with IDENTITY_INSERT, which
  BastionServerConnection cannot use. Everything runs in one
  transaction: if one row fails, none is loaded.

  The file is UTF-8: run it with sqlcmd -f 65001, or open it in SQL
  Server Management Studio, which recognizes it by its byte order
  mark. Texts are N'...' literals, with their ñ and accents, and the
  last check verifies they were stored without loss (D-21).
=====================================================================*/


USE Bastion;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/*---------------------------------------------------------------------
  Preloaded catalogs (D-09)
---------------------------------------------------------------------*/

INSERT INTO dbo.GameMode (GameModeId, Code, BoardSize, PlayerCount, WallsPerPlayer, IsActive) VALUES
    (1, N'CLASSIC', 9, 2, 10, 1),
    (2, N'FOUR_PLAYERS', 9, 4, 5, 1),
    (3, N'QUICK', 7, 2, 6, 1);

-- A term without a language is filtered in every language (CU-28 RN-01).
SET IDENTITY_INSERT dbo.BannedWord ON;
INSERT INTO dbo.BannedWord (BannedWordId, Term, Language, Scope) VALUES
    (1, N'tonto', N'es-MX', N'BOTH'),
    (2, N'idiota', N'es-MX', N'BOTH'),
    (3, N'estúpido', N'es-MX', N'CHAT'),
    (4, N'admin', NULL, N'NICKNAME'),
    (5, N'moderador', N'es-MX', N'NICKNAME'),
    (6, N'moderator', N'en', N'NICKNAME'),
    (7, N'idiot', N'en', N'BOTH'),
    (8, N'stupid', N'en', N'CHAT');
SET IDENTITY_INSERT dbo.BannedWord OFF;

/*---------------------------------------------------------------------
  Account and access
---------------------------------------------------------------------*/

-- Each account stores the last version of the terms it accepted (D-23).
-- Test passwords (PBKDF2-SHA512, 210,000 iterations, 32-byte salt, 64-byte hash, as the server derives them):
--   admin_bastion    Admin#2026
--   ana_modera       Ana#Modera26
--   luis_quo         Luis#Quo2026
--   marta_muros      Marta#Muros9
--   pedro_peon       Pedro#Peon13
--   sofia_salto      Sofia#Salto8
--   nico_nuevo       Nico#Nuevo26
--   rayo_veloz       Rayo#Veloz77
--   4dm1n_oficial    Adm1n#Falso6
SET IDENTITY_INSERT dbo.Account ON;
INSERT INTO dbo.Account (AccountId, AccountType, AccountStatus, Role, Nickname, Email, PasswordHash, PasswordSalt, BirthDate, PreferredLanguage, FailedAttempts, LastFailedAttemptAt, LockedUntil, IsTwoFactorEnabled, RegisteredAt, LastVerificationSentAt, LastRecoverySentAt, DeletedAt, TermsVersion, TermsLanguage, TermsAcceptedAt) VALUES
    (1, N'REGISTERED', N'ACTIVE', N'ADMINISTRATOR', N'admin_bastion', N'admin@bastion.mx', 0xE512BE2EA88C1E07CB8FE4B40E7950A17431CAC589E9750FD95258A5954ED42477937DA66964EB1CABBE588F05BB0F4B110A811DB11874AB925A1E2B8A74CE7E, 0x6AA375508A68679CFCA3992EB2B591F8AA29BFAAFDAEC2A927AD9753875E33C2, '1990-05-12', N'es-MX', 3, '2026-09-01T09:02:10', '2026-09-01T09:07:10', 0, '2026-06-01T10:00:00', '2026-06-01T10:00:00', '2026-09-01T17:46:00', NULL, N'2026.1', N'es-MX', '2026-06-01T10:00:00'),
    (2, N'REGISTERED', N'ACTIVE', N'MODERATOR', N'ana_modera', N'ana.moderadora@correo.mx', 0x183865FFA989D54102FF3420AAF10EC87313150E9BE98691D670BA9ED627893A0C2BFD0951B84C7B719A23662E4694BDF6DA573B6E267DC26487898E72DC90B3, 0x4CE551C6E877767CD15D19AA3D51EDECB07E71B860C0873E3BF0A74B26FBD6EF, '1995-02-20', N'es-MX', 0, NULL, NULL, 0, '2026-06-02T09:00:00', '2026-06-02T09:00:00', NULL, NULL, N'2026.1', N'es-MX', '2026-06-02T09:00:00'),
    (3, N'REGISTERED', N'ACTIVE', N'PLAYER', N'luis_quo', N'luis.quo@correo.mx', 0x09137ADB64B559E494DE4718D2D5EE27FFE9B19E727D739FC59E5F8CE056392585EEBAD2433A9D43FBEBB89A145B5B98A4D49AF72B8D666A857FA1F86FB6F7E8, 0xA69F889A08067E07C075CFF432688E7E249BD39686F3025E9314EE567B6B1ED8, '2008-03-15', N'es-MX', 0, NULL, NULL, 0, '2026-06-10T16:00:00', '2026-09-01T17:20:00', NULL, NULL, N'2026.2', N'es-MX', '2026-08-01T10:00:00'),
    (4, N'REGISTERED', N'ACTIVE', N'PLAYER', N'marta_muros', N'marta@correo.mx', 0x7698539E0E1312CECC1F4D1DCF973D59B00F7FA7B90151EE22DC76B31DA4337E5CA8714E0A58EFA3057AE0A245A757969F4CC2C42008BD87582C18C3047F5CE2, 0xA8CE58A371D03E8E14D7445C7B3A627BA19A4CCFB5B543B759E9CED65BD7D2CF, '2009-07-01', N'es-MX', 0, NULL, NULL, 1, '2026-06-12T18:30:00', '2026-06-12T18:30:00', NULL, NULL, N'2026.1', N'es-MX', '2026-06-12T18:30:00'),
    (5, N'REGISTERED', N'ACTIVE', N'PLAYER', N'pedro_peon', N'pedro@correo.mx', 0x81443A951798AABBD54D1AF90E62747E4BECFC3FC1DE149DF206255428C8279B7FA3F73A2D8DBB94E68209A627E7512DD27925E3B38F526E43EA89B5EBAD76CC, 0xFDED7ABC77AF7117A8502AD5B9AAC96F6F03B20D78E86D31ECD519B029CBB577, '2012-11-30', N'es-MX', 0, NULL, NULL, 0, '2026-06-15T12:00:00', '2026-06-15T12:00:00', NULL, NULL, N'2026.1', N'es-MX', '2026-06-15T12:00:00'),
    (6, N'REGISTERED', N'ACTIVE', N'PLAYER', N'sofia_salto', N'sofia@correo.mx', 0xE2C9B182C094828794E98C18D5945E3AFB154D7CC150383487450F195AEE17DA7C8EC45EAB35D7CD5463752029041D3B111E84B910E93DED7E0DFD3E9DCD8980, 0x6EE9AE7164E84FE55577CB35F50477B846AE07F720F64B2AC52230175D117F0B, '2011-04-18', N'en', 0, NULL, NULL, 0, '2026-06-18T20:00:00', '2026-06-18T20:00:00', '2026-08-20T10:55:00', NULL, N'2026.1', N'en', '2026-06-18T20:00:00'),
    (7, N'GUEST', N'ACTIVE', N'PLAYER', N'Invitado_4821', NULL, NULL, NULL, NULL, N'es-MX', 0, NULL, NULL, 0, '2026-08-30T16:00:00', NULL, NULL, NULL, NULL, NULL, NULL),
    (8, N'REGISTERED', N'PENDING', N'PLAYER', N'nico_nuevo', N'nico@correo.mx', 0x80285278578D9BC462C1470C92F0AC987DB968C3C7F73E3EBEC87C2F0CE07E37C787CB76F991B99553DF5AFC234674C58A8AB52454BB08F3F29AD2C4149D31B7, 0x0998CE4808ACC6B9B6D429771717517E5A98721A65FAAFC9C89709FF4BFC167E, '2010-09-09', N'es-MX', 0, NULL, NULL, 0, '2026-08-28T19:00:00', '2026-09-01T12:00:00', NULL, NULL, N'2026.2', N'es-MX', '2026-08-28T19:00:00'),
    (9, N'REGISTERED', N'SUSPENDED', N'PLAYER', N'rayo_veloz', N'rayo@correo.mx', 0x28307055CFFE7A50CBB79479859A3941E07F3CB21A4D23E3F06B4C7B881336A80567CD9DC7EC38515F57C936000356C2235D606162E5D7BBB9BF997B1732F5F4, 0x6FA1740598450E476FEC27BA149736E6EEC0F8F50A2A8F91E737118E22D9150D, '2007-12-01', N'es-MX', 0, NULL, NULL, 0, '2026-06-20T15:00:00', '2026-06-20T15:00:00', NULL, NULL, N'2026.1', N'es-MX', '2026-06-20T15:00:00'),
    (10, N'REGISTERED', N'DELETED', N'PLAYER', N'anonimo_10', N'anonimo_10', NULL, NULL, NULL, N'es-MX', 0, NULL, NULL, 0, '2026-06-22T11:00:00', '2026-06-22T11:00:00', NULL, '2026-08-02T13:00:00', N'2026.1', N'es-MX', '2026-06-22T11:00:00'),
    (11, N'REGISTERED', N'BANNED', N'PLAYER', N'4dm1n_oficial', N'cuatro.admin@correo.mx', 0x3AAC179CA2304FF22B6CB88548480B36C74205C562B7390CB54BD5D3F05B69A42BF69C770CA6710E662C2373B056525FD3E46E1FE1B41613EE2230FDF6114A8F, 0x289136148379CAB38E8C850CD3546B01801C667E2324F759AD5E81CFE0587F1E, '2006-01-10', N'es-MX', 0, NULL, NULL, 0, '2026-08-22T19:00:00', '2026-08-22T19:00:00', NULL, NULL, N'2026.2', N'es-MX', '2026-08-22T19:00:00');
SET IDENTITY_INSERT dbo.Account OFF;

SET IDENTITY_INSERT dbo.Session ON;
INSERT INTO dbo.Session (SessionId, AccountId, TokenHash, StartedAt, ExpiresAt, LastUsedAt, EndedAt, CloseReason, IpAddress, DeviceFingerprint, DeviceName, ClientVersion) VALUES
    (1, 1, HASHBYTES('SHA2_256', 'session:1'), '2026-08-31T08:00:00', '2026-09-01T08:00:00', '2026-08-31T11:58:00', '2026-08-31T12:00:00', N'VOLUNTARY_LOGOUT', N'189.203.14.20', N'hw-admin-01', N'Laptop de oficina', N'1.0.2'),
    (2, 2, HASHBYTES('SHA2_256', 'session:2'), '2026-09-01T17:00:00', '2026-09-02T17:00:00', '2026-09-01T17:59:00', NULL, NULL, N'201.141.22.7', N'hw-ana-01', N'PC de escritorio', N'1.0.2'),
    (3, 3, HASHBYTES('SHA2_256', 'session:3'), '2026-08-31T20:00:00', '2026-09-01T20:00:00', '2026-08-31T22:30:00', '2026-09-01T17:10:00', N'REMOTE_LOGOUT', N'189.203.55.3', N'hw-luis-tab', N'Tableta', N'1.0.2'),
    (4, 3, HASHBYTES('SHA2_256', 'session:4'), '2026-09-01T17:05:00', '2026-09-02T17:05:00', '2026-09-01T17:59:00', NULL, NULL, N'189.203.55.9', N'hw-luis-pc', N'PC de escritorio', N'1.0.2'),
    (5, 4, HASHBYTES('SHA2_256', 'session:5'), '2026-08-01T19:00:00', '2026-08-02T19:00:00', '2026-08-01T20:29:00', '2026-08-01T20:30:00', N'VOLUNTARY_LOGOUT', N'187.190.3.41', N'hw-marta-01', N'Laptop', N'1.0.2'),
    (6, 4, HASHBYTES('SHA2_256', 'session:6'), '2026-09-01T17:54:00', '2026-09-02T17:54:00', '2026-09-01T17:59:00', NULL, NULL, N'187.190.3.41', N'hw-marta-01', N'Laptop', N'1.0.2'),
    (7, 5, HASHBYTES('SHA2_256', 'session:7'), '2026-09-01T17:45:00', '2026-09-02T17:45:00', '2026-09-01T17:59:00', NULL, NULL, N'201.141.80.12', N'hw-pedro-01', N'PC familiar', N'1.0.2'),
    (8, 6, HASHBYTES('SHA2_256', 'session:8'), '2026-08-19T18:00:00', '2026-08-20T18:00:00', '2026-08-19T21:00:00', '2026-08-20T11:05:00', N'CREDENTIALS_CHANGED', N'187.190.66.5', N'hw-sofia-01', N'Celular', N'1.0.2'),
    (9, 6, HASHBYTES('SHA2_256', 'session:9'), '2026-09-01T17:25:00', '2026-09-02T17:25:00', '2026-09-01T17:59:00', NULL, NULL, N'187.190.66.5', N'hw-sofia-01', N'Celular', N'1.0.2'),
    (10, 9, HASHBYTES('SHA2_256', 'session:10'), '2026-08-26T17:00:00', '2026-08-27T17:00:00', '2026-08-26T19:00:00', '2026-08-27T10:00:00', N'SANCTION', N'201.141.90.33', N'hw-rayo-01', N'Laptop', N'1.0.2'),
    (11, 10, HASHBYTES('SHA2_256', 'session:11'), '2026-08-02T12:30:00', '2026-08-03T12:30:00', '2026-08-02T12:59:00', '2026-08-02T13:00:00', N'ACCOUNT_DELETED', N'189.203.77.8', N'hw-diez-01', N'PC', N'1.0.2'),
    (12, 7, HASHBYTES('SHA2_256', 'session:12'), '2026-08-30T16:00:00', '2026-08-31T16:00:00', '2026-08-30T17:30:00', NULL, NULL, N'189.203.40.2', N'hw-invitado-4821', N'Celular', N'1.0.2'),
    (13, 7, HASHBYTES('SHA2_256', 'session:13'), '2026-09-01T17:48:00', '2026-09-02T17:48:00', '2026-09-01T17:59:00', NULL, NULL, N'189.203.40.2', N'hw-invitado-4821', N'Celular', N'1.0.2'),
    (14, 7, HASHBYTES('SHA2_256', 'session:14'), '2026-08-31T16:20:00', '2026-09-01T16:20:00', '2026-08-31T16:40:00', NULL, NULL, N'189.203.40.2', N'hw-invitado-4821', N'Celular', N'1.0.2'),
    (15, 11, HASHBYTES('SHA2_256', 'session:15'), '2026-08-22T19:15:00', '2026-08-23T19:15:00', '2026-08-22T21:00:00', '2026-08-23T10:00:00', N'SANCTION', N'201.141.33.8', N'hw-once-01', N'PC', N'1.0.2');
SET IDENTITY_INSERT dbo.Session OFF;

-- Verification and recovery tokens and second-factor codes, in a single table (D-23).
SET IDENTITY_INSERT dbo.VerificationCode ON;
INSERT INTO dbo.VerificationCode (VerificationCodeId, AccountId, Purpose, CodeHash, TargetEmail, IssuedAt, ExpiresAt, Attempts, Status) VALUES
    (1, 1, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:1'), NULL, '2026-06-01T10:00:00', '2026-06-02T10:00:00', 0, N'USED'),
    (2, 2, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:2'), NULL, '2026-06-02T09:00:00', '2026-06-03T09:00:00', 0, N'USED'),
    (3, 3, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:3'), NULL, '2026-06-10T16:00:00', '2026-06-11T16:00:00', 0, N'USED'),
    (4, 4, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:4'), NULL, '2026-06-12T18:30:00', '2026-06-13T18:30:00', 0, N'USED'),
    (5, 5, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:5'), NULL, '2026-06-15T12:00:00', '2026-06-16T12:00:00', 0, N'USED'),
    (6, 6, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:6'), NULL, '2026-06-18T20:00:00', '2026-06-19T20:00:00', 0, N'USED'),
    (7, 9, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:7'), NULL, '2026-06-20T15:00:00', '2026-06-21T15:00:00', 0, N'USED'),
    (8, 10, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:8'), NULL, '2026-06-22T11:00:00', '2026-06-23T11:00:00', 0, N'USED'),
    (9, 4, N'TWO_FACTOR', HASHBYTES('SHA2_256', 'code:1'), NULL, '2026-08-01T19:05:00', '2026-08-01T19:10:00', 0, N'USED'),
    (10, 4, N'TWO_FACTOR', HASHBYTES('SHA2_256', 'code:2'), NULL, '2026-08-15T20:00:00', '2026-08-15T20:05:00', 3, N'INVALIDATED'),
    (11, 6, N'PASSWORD_RESET', HASHBYTES('SHA2_256', 'recovery:1'), NULL, '2026-08-20T10:55:00', '2026-08-20T11:25:00', 1, N'USED'),
    (12, 11, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:12'), NULL, '2026-08-22T19:00:00', '2026-08-23T19:00:00', 0, N'USED'),
    (13, 8, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:9'), NULL, '2026-08-28T19:00:00', '2026-08-29T19:00:00', 0, N'INVALIDATED'),
    (14, 8, N'REGISTRATION', HASHBYTES('SHA2_256', 'verification:10'), NULL, '2026-09-01T12:00:00', '2026-09-02T12:00:00', 0, N'PENDING'),
    (15, 3, N'EMAIL_CHANGE', HASHBYTES('SHA2_256', 'verification:11'), N'luis.quo.nuevo@correo.mx', '2026-09-01T17:20:00', '2026-09-02T17:20:00', 0, N'PENDING'),
    (16, 1, N'PASSWORD_RESET', HASHBYTES('SHA2_256', 'recovery:2'), NULL, '2026-09-01T17:40:00', '2026-09-01T18:10:00', 0, N'INVALIDATED'),
    (17, 1, N'PASSWORD_RESET', HASHBYTES('SHA2_256', 'recovery:3'), NULL, '2026-09-01T17:46:00', '2026-09-01T18:16:00', 0, N'PENDING'),
    (18, 4, N'TWO_FACTOR', HASHBYTES('SHA2_256', 'code:3'), NULL, '2026-09-01T17:53:00', '2026-09-01T17:58:00', 0, N'USED'),
    (19, 4, N'TWO_FACTOR', HASHBYTES('SHA2_256', 'code:4'), NULL, '2026-09-01T17:58:00', '2026-09-01T18:03:00', 0, N'PENDING');
SET IDENTITY_INSERT dbo.VerificationCode OFF;

/*---------------------------------------------------------------------
  Match
---------------------------------------------------------------------*/

SET IDENTITY_INSERT dbo.Match ON;
INSERT INTO dbo.Match (MatchId, GameModeId, MatchType, Status, ClockMinutes, StartedAt, EndedAt, EndReason, CurrentTurn) VALUES
    (1, 1, N'RANKED', N'FINISHED', 5, '2026-07-05T17:00:00', '2026-07-05T17:01:42', N'GOAL', 2),
    (2, 1, N'RANKED', N'FINISHED', 5, '2026-07-08T18:00:00', '2026-07-08T18:00:52', N'RESIGNATION', 1),
    (3, 1, N'RANKED', N'FINISHED', 5, '2026-07-12T16:30:00', '2026-07-12T16:31:54', N'GOAL', 1),
    (4, 1, N'RANKED', N'FINISHED', 5, '2026-07-20T19:00:00', '2026-07-20T19:01:56', N'GOAL', 1),
    (5, 1, N'RANKED', N'FINISHED', 5, '2026-07-26T17:30:00', '2026-07-26T17:31:40', N'GOAL', 2),
    (6, 3, N'RANKED', N'FINISHED', 3, '2026-08-10T18:00:00', '2026-08-10T18:01:11', N'DRAW', 1),
    (7, 2, N'RANKED', N'FINISHED', 10, '2026-08-15T19:00:00', '2026-08-15T19:01:30', N'ABANDONMENT', 2),
    (9, 1, N'RANKED', N'FINISHED', 5, '2026-08-26T17:10:00', '2026-08-26T17:11:55', N'GOAL', 2),
    (10, 3, N'PRIVATE', N'FINISHED', 10, '2026-08-31T16:30:00', '2026-08-31T16:31:05', N'GOAL', 2),
    (11, 3, N'RANKED', N'FINISHED', 3, '2026-08-28T18:00:00', '2026-08-28T18:03:29', N'TIMEOUT', 2),
    (12, 1, N'RANKED', N'IN_PROGRESS', 5, '2026-09-01T17:30:00', NULL, NULL, 1);
SET IDENTITY_INSERT dbo.Match OFF;

INSERT INTO dbo.Participation (MatchId, AccountId, TurnOrder, PawnSymbol, CurrentSquare, RemainingWalls, RemainingClock, InitialElo, FinalElo, Result, Placement, EndReason, IsConnected, DisconnectedAt) VALUES
    (1, 3, 1, N'BLUE', N'e9', 10, 249352, 1000, 1016, N'WON', NULL, NULL, 1, NULL),
    (1, 4, 2, N'RED', N'd7', 6, 248933, 1000, 984, N'LOST', NULL, NULL, 1, NULL),
    (2, 4, 1, N'BLUE', N'f3', 10, 276855, 984, 969, N'LOST', NULL, NULL, 1, NULL),
    (2, 3, 2, N'RED', N'e7', 9, 280098, 1016, 1031, N'WON', NULL, NULL, 1, NULL),
    (3, 3, 1, N'BLUE', N'f5', 7, 247688, 1031, 1012, N'LOST', NULL, NULL, 1, NULL),
    (3, 4, 2, N'RED', N'e1', 10, 238336, 969, 988, N'WON', NULL, NULL, 1, NULL),
    (4, 4, 1, N'BLUE', N'c4', 7, 237856, 988, 973, N'LOST', NULL, NULL, 1, NULL),
    (4, 3, 2, N'RED', N'e1', 10, 246504, 1012, 1027, N'WON', NULL, NULL, 1, NULL),
    (5, 3, 1, N'BLUE', N'e9', 10, 251954, 1027, 1041, N'WON', NULL, NULL, 1, NULL),
    (5, 4, 2, N'RED', N'e4', 9, 248532, 973, 959, N'LOST', NULL, NULL, 1, NULL),
    (6, 5, 1, N'BLUE', N'd4', 5, 148800, 1000, 1000, N'DRAW', NULL, NULL, 1, NULL),
    (6, 6, 2, N'RED', N'e5', 5, 153124, 1000, 1000, N'DRAW', NULL, NULL, 1, NULL),
    (7, 3, 1, N'BLUE', N'e5', 5, 572532, 1000, 1024, N'WON', 1, NULL, 1, NULL),
    (7, 4, 2, N'RED', N'c5', 4, 580568, 1000, 1008, N'LOST', 2, N'ABANDONMENT', 1, NULL),
    (7, 5, 3, N'GREEN', N'e8', 5, 596640, 1000, 976, N'LOST', 4, N'ABANDONMENT', 1, NULL),
    (7, 6, 4, N'YELLOW', N'g5', 5, 580685, 1000, 992, N'LOST', 3, N'ABANDONMENT', 1, NULL),
    (9, 3, 1, N'BLUE', N'd9', 10, 234012, 1041, 1055, N'WON', NULL, NULL, 1, NULL),
    (9, 9, 2, N'RED', N'g6', 7, 251344, 1000, 986, N'LOST', NULL, NULL, 1, NULL),
    (10, 6, 1, N'BLUE', N'd7', 8, 568176, NULL, NULL, N'WON', NULL, NULL, 1, NULL),
    (10, 7, 2, N'RED', N'c5', 6, 567480, NULL, NULL, N'LOST', NULL, NULL, 1, NULL),
    (11, 4, 1, N'BLUE', N'e4', 6, 151220, 1000, 1016, N'WON', NULL, NULL, 1, NULL),
    (11, 5, 2, N'RED', N'd5', 5, 0, 1000, 984, N'LOST', NULL, NULL, 1, NULL),
    (12, 3, 1, N'BLUE', N'e4', 10, 284985, 1055, NULL, NULL, NULL, NULL, 1, NULL),
    (12, 6, 2, N'RED', N'e7', 9, 279228, 1000, NULL, NULL, NULL, NULL, 1, '2026-09-01T17:38:00');

INSERT INTO dbo.Move (MatchId, MoveNumber, AccountId, MoveType, FromSquare, ToSquare, Groove, Orientation, TimeSpent, PlayedAt) VALUES
    (1, 1, 3, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 7148, '2026-07-05T17:00:07.148'),
    (1, 2, 4, N'WALL', NULL, NULL, N'a7', N'HORIZONTAL', 6067, '2026-07-05T17:00:13.215'),
    (1, 3, 3, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 4986, '2026-07-05T17:00:18.201'),
    (1, 4, 4, N'PAWN_MOVE', N'e9', N'd9', NULL, NULL, 3905, '2026-07-05T17:00:22.106'),
    (1, 5, 3, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 2824, '2026-07-05T17:00:24.930'),
    (1, 6, 4, N'WALL', NULL, NULL, N'g7', N'HORIZONTAL', 10743, '2026-07-05T17:00:35.673'),
    (1, 7, 3, N'PAWN_MOVE', N'e4', N'e5', NULL, NULL, 9662, '2026-07-05T17:00:45.335'),
    (1, 8, 4, N'PAWN_MOVE', N'd9', N'd8', NULL, NULL, 8581, '2026-07-05T17:00:53.916'),
    (1, 9, 3, N'PAWN_MOVE', N'e5', N'e6', NULL, NULL, 7500, '2026-07-05T17:01:01.416'),
    (1, 10, 4, N'WALL', NULL, NULL, N'b3', N'VERTICAL', 6419, '2026-07-05T17:01:07.835'),
    (1, 11, 3, N'PAWN_MOVE', N'e6', N'e7', NULL, NULL, 5338, '2026-07-05T17:01:13.173'),
    (1, 12, 4, N'PAWN_MOVE', N'd8', N'd7', NULL, NULL, 4257, '2026-07-05T17:01:17.430'),
    (1, 13, 3, N'PAWN_MOVE', N'e7', N'e8', NULL, NULL, 3176, '2026-07-05T17:01:20.606'),
    (1, 14, 4, N'WALL', NULL, NULL, N'h2', N'HORIZONTAL', 11095, '2026-07-05T17:01:31.701'),
    (1, 15, 3, N'PAWN_MOVE', N'e8', N'e9', NULL, NULL, 10014, '2026-07-05T17:01:41.715'),
    (2, 1, 4, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 3877, '2026-07-08T18:00:03.877'),
    (2, 2, 3, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 2796, '2026-07-08T18:00:06.673'),
    (2, 3, 4, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 10715, '2026-07-08T18:00:17.388'),
    (2, 4, 3, N'WALL', NULL, NULL, N'd3', N'HORIZONTAL', 9634, '2026-07-08T18:00:27.022'),
    (2, 5, 4, N'PAWN_MOVE', N'e3', N'f3', NULL, NULL, 8553, '2026-07-08T18:00:35.575'),
    (2, 6, 3, N'PAWN_MOVE', N'e8', N'e7', NULL, NULL, 7472, '2026-07-08T18:00:43.047'),
    (3, 1, 3, N'PAWN_MOVE', N'e1', N'f1', NULL, NULL, 9606, '2026-07-12T16:30:09.606'),
    (3, 2, 4, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 8525, '2026-07-12T16:30:18.131'),
    (3, 3, 3, N'PAWN_MOVE', N'f1', N'f2', NULL, NULL, 7444, '2026-07-12T16:30:25.575'),
    (3, 4, 4, N'PAWN_MOVE', N'e8', N'e7', NULL, NULL, 6363, '2026-07-12T16:30:31.938'),
    (3, 5, 3, N'WALL', NULL, NULL, N'a5', N'HORIZONTAL', 5282, '2026-07-12T16:30:37.220'),
    (3, 6, 4, N'PAWN_MOVE', N'e7', N'e6', NULL, NULL, 4201, '2026-07-12T16:30:41.421'),
    (3, 7, 3, N'PAWN_MOVE', N'f2', N'f3', NULL, NULL, 3120, '2026-07-12T16:30:44.541'),
    (3, 8, 4, N'PAWN_MOVE', N'e6', N'e5', NULL, NULL, 11039, '2026-07-12T16:30:55.580'),
    (3, 9, 3, N'WALL', NULL, NULL, N'h6', N'VERTICAL', 9958, '2026-07-12T16:31:05.538'),
    (3, 10, 4, N'PAWN_MOVE', N'e5', N'e4', NULL, NULL, 8877, '2026-07-12T16:31:14.415'),
    (3, 11, 3, N'PAWN_MOVE', N'f3', N'f4', NULL, NULL, 7796, '2026-07-12T16:31:22.211'),
    (3, 12, 4, N'PAWN_MOVE', N'e4', N'e3', NULL, NULL, 6715, '2026-07-12T16:31:28.926'),
    (3, 13, 3, N'WALL', NULL, NULL, N'c7', N'HORIZONTAL', 5634, '2026-07-12T16:31:34.560'),
    (3, 14, 4, N'PAWN_MOVE', N'e3', N'e2', NULL, NULL, 4553, '2026-07-12T16:31:39.113'),
    (3, 15, 3, N'PAWN_MOVE', N'f4', N'f5', NULL, NULL, 3472, '2026-07-12T16:31:42.585'),
    (3, 16, 4, N'PAWN_MOVE', N'e2', N'e1', NULL, NULL, 11391, '2026-07-12T16:31:53.976'),
    (4, 1, 4, N'PAWN_MOVE', N'e1', N'd1', NULL, NULL, 6335, '2026-07-20T19:00:06.335'),
    (4, 2, 3, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 5254, '2026-07-20T19:00:11.589'),
    (4, 3, 4, N'PAWN_MOVE', N'd1', N'd2', NULL, NULL, 4173, '2026-07-20T19:00:15.762'),
    (4, 4, 3, N'PAWN_MOVE', N'e8', N'e7', NULL, NULL, 3092, '2026-07-20T19:00:18.854'),
    (4, 5, 4, N'WALL', NULL, NULL, N'f5', N'VERTICAL', 11011, '2026-07-20T19:00:29.865'),
    (4, 6, 3, N'PAWN_MOVE', N'e7', N'e6', NULL, NULL, 9930, '2026-07-20T19:00:39.795'),
    (4, 7, 4, N'PAWN_MOVE', N'd2', N'd3', NULL, NULL, 8849, '2026-07-20T19:00:48.644'),
    (4, 8, 3, N'PAWN_MOVE', N'e6', N'e5', NULL, NULL, 7768, '2026-07-20T19:00:56.412'),
    (4, 9, 4, N'WALL', NULL, NULL, N'b6', N'HORIZONTAL', 6687, '2026-07-20T19:01:03.099'),
    (4, 10, 3, N'PAWN_MOVE', N'e5', N'e4', NULL, NULL, 5606, '2026-07-20T19:01:08.705'),
    (4, 11, 4, N'PAWN_MOVE', N'd3', N'c3', NULL, NULL, 4525, '2026-07-20T19:01:13.230'),
    (4, 12, 3, N'PAWN_MOVE', N'e4', N'e3', NULL, NULL, 3444, '2026-07-20T19:01:16.674'),
    (4, 13, 4, N'PAWN_MOVE', N'c3', N'c4', NULL, NULL, 11363, '2026-07-20T19:01:28.037'),
    (4, 14, 3, N'PAWN_MOVE', N'e3', N'e2', NULL, NULL, 10282, '2026-07-20T19:01:38.319'),
    (4, 15, 4, N'WALL', NULL, NULL, N'g2', N'HORIZONTAL', 9201, '2026-07-20T19:01:47.520'),
    (4, 16, 3, N'PAWN_MOVE', N'e2', N'e1', NULL, NULL, 8120, '2026-07-20T19:01:55.640'),
    (5, 1, 3, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 3064, '2026-07-26T17:30:03.064'),
    (5, 2, 4, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 10983, '2026-07-26T17:30:14.047'),
    (5, 3, 3, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 9902, '2026-07-26T17:30:23.949'),
    (5, 4, 4, N'PAWN_MOVE', N'e8', N'e7', NULL, NULL, 8821, '2026-07-26T17:30:32.770'),
    (5, 5, 3, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 7740, '2026-07-26T17:30:40.510'),
    (5, 6, 4, N'PAWN_MOVE', N'e7', N'e6', NULL, NULL, 6659, '2026-07-26T17:30:47.169'),
    (5, 7, 3, N'PAWN_MOVE', N'e4', N'e5', NULL, NULL, 5578, '2026-07-26T17:30:52.747'),
    (5, 8, 4, N'WALL', NULL, NULL, N'a2', N'VERTICAL', 4497, '2026-07-26T17:30:57.244'),
    (5, 9, 3, N'PAWN_MOVE', N'e5', N'e7', NULL, NULL, 3416, '2026-07-26T17:31:00.660'),
    (5, 10, 4, N'PAWN_MOVE', N'e6', N'e5', NULL, NULL, 11335, '2026-07-26T17:31:11.995'),
    (5, 11, 3, N'PAWN_MOVE', N'e7', N'e8', NULL, NULL, 10254, '2026-07-26T17:31:22.249'),
    (5, 12, 4, N'PAWN_MOVE', N'e5', N'e4', NULL, NULL, 9173, '2026-07-26T17:31:31.422'),
    (5, 13, 3, N'PAWN_MOVE', N'e8', N'e9', NULL, NULL, 8092, '2026-07-26T17:31:39.514'),
    (6, 1, 5, N'PAWN_MOVE', N'd1', N'd2', NULL, NULL, 8793, '2026-08-10T18:00:08.793'),
    (6, 2, 6, N'PAWN_MOVE', N'd7', N'd6', NULL, NULL, 7712, '2026-08-10T18:00:16.505'),
    (6, 3, 5, N'WALL', NULL, NULL, N'c5', N'HORIZONTAL', 6631, '2026-08-10T18:00:23.136'),
    (6, 4, 6, N'PAWN_MOVE', N'd6', N'e6', NULL, NULL, 5550, '2026-08-10T18:00:28.686'),
    (6, 5, 5, N'PAWN_MOVE', N'd2', N'd3', NULL, NULL, 4469, '2026-08-10T18:00:33.155'),
    (6, 6, 6, N'WALL', NULL, NULL, N'c2', N'HORIZONTAL', 3388, '2026-08-10T18:00:36.543'),
    (6, 7, 5, N'PAWN_MOVE', N'd3', N'd4', NULL, NULL, 11307, '2026-08-10T18:00:47.850'),
    (6, 8, 6, N'PAWN_MOVE', N'e6', N'e5', NULL, NULL, 10226, '2026-08-10T18:00:58.076'),
    (7, 1, 3, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 5522, '2026-08-15T19:00:05.522'),
    (7, 2, 4, N'PAWN_MOVE', N'a5', N'b5', NULL, NULL, 4441, '2026-08-15T19:00:09.963'),
    (7, 3, 5, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 3360, '2026-08-15T19:00:13.323'),
    (7, 4, 6, N'PAWN_MOVE', N'i5', N'h5', NULL, NULL, 11279, '2026-08-15T19:00:24.602'),
    (7, 5, 3, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 10198, '2026-08-15T19:00:34.800'),
    (7, 6, 4, N'WALL', NULL, NULL, N'b6', N'HORIZONTAL', 9117, '2026-08-15T19:00:43.917'),
    (7, 7, 6, N'PAWN_MOVE', N'h5', N'g5', NULL, NULL, 8036, '2026-08-15T19:00:51.953'),
    (7, 8, 3, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 6955, '2026-08-15T19:00:58.908'),
    (7, 9, 4, N'PAWN_MOVE', N'b5', N'c5', NULL, NULL, 5874, '2026-08-15T19:01:04.782'),
    (7, 10, 3, N'PAWN_MOVE', N'e4', N'e5', NULL, NULL, 4793, '2026-08-15T19:01:09.575'),
    (9, 1, 3, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 7980, '2026-08-26T17:10:07.980'),
    (9, 2, 9, N'PAWN_MOVE', N'e9', N'f9', NULL, NULL, 6899, '2026-08-26T17:10:14.879'),
    (9, 3, 3, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 5818, '2026-08-26T17:10:20.697'),
    (9, 4, 9, N'WALL', NULL, NULL, N'e5', N'HORIZONTAL', 4737, '2026-08-26T17:10:25.434'),
    (9, 5, 3, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 3656, '2026-08-26T17:10:29.090'),
    (9, 6, 9, N'PAWN_MOVE', N'f9', N'f8', NULL, NULL, 2575, '2026-08-26T17:10:31.665'),
    (9, 7, 3, N'PAWN_MOVE', N'e4', N'e5', NULL, NULL, 10494, '2026-08-26T17:10:42.159'),
    (9, 8, 9, N'WALL', NULL, NULL, N'd6', N'VERTICAL', 9413, '2026-08-26T17:10:51.572'),
    (9, 9, 3, N'PAWN_MOVE', N'e5', N'd5', NULL, NULL, 8332, '2026-08-26T17:10:59.904'),
    (9, 10, 9, N'PAWN_MOVE', N'f8', N'f7', NULL, NULL, 7251, '2026-08-26T17:11:07.155'),
    (9, 11, 3, N'PAWN_MOVE', N'd5', N'd6', NULL, NULL, 6170, '2026-08-26T17:11:13.325'),
    (9, 12, 9, N'PAWN_MOVE', N'f7', N'f6', NULL, NULL, 5089, '2026-08-26T17:11:18.414'),
    (9, 13, 3, N'PAWN_MOVE', N'd6', N'd7', NULL, NULL, 4008, '2026-08-26T17:11:22.422'),
    (9, 14, 9, N'WALL', NULL, NULL, N'h3', N'HORIZONTAL', 2927, '2026-08-26T17:11:25.349'),
    (9, 15, 3, N'PAWN_MOVE', N'd7', N'd8', NULL, NULL, 10846, '2026-08-26T17:11:36.195'),
    (9, 16, 9, N'PAWN_MOVE', N'f6', N'g6', NULL, NULL, 9765, '2026-08-26T17:11:45.960'),
    (9, 17, 3, N'PAWN_MOVE', N'd8', N'd9', NULL, NULL, 8684, '2026-08-26T17:11:54.644'),
    (10, 1, 6, N'PAWN_MOVE', N'd1', N'd2', NULL, NULL, 4709, '2026-08-31T16:30:04.709'),
    (10, 2, 7, N'PAWN_MOVE', N'd7', N'c7', NULL, NULL, 3628, '2026-08-31T16:30:08.337'),
    (10, 3, 6, N'PAWN_MOVE', N'd2', N'd3', NULL, NULL, 2547, '2026-08-31T16:30:10.884'),
    (10, 4, 7, N'WALL', NULL, NULL, N'e3', N'VERTICAL', 10466, '2026-08-31T16:30:21.350'),
    (10, 5, 6, N'PAWN_MOVE', N'd3', N'd4', NULL, NULL, 9385, '2026-08-31T16:30:30.735'),
    (10, 6, 7, N'PAWN_MOVE', N'c7', N'c6', NULL, NULL, 8304, '2026-08-31T16:30:39.039'),
    (10, 7, 6, N'PAWN_MOVE', N'd4', N'd5', NULL, NULL, 7223, '2026-08-31T16:30:46.262'),
    (10, 8, 7, N'PAWN_MOVE', N'c6', N'c5', NULL, NULL, 6142, '2026-08-31T16:30:52.404'),
    (10, 9, 6, N'PAWN_MOVE', N'd5', N'd6', NULL, NULL, 5061, '2026-08-31T16:30:57.465'),
    (10, 10, 7, N'WALL', NULL, NULL, N'a1', N'HORIZONTAL', 3980, '2026-08-31T16:31:01.445'),
    (10, 11, 6, N'PAWN_MOVE', N'd6', N'd7', NULL, NULL, 2899, '2026-08-31T16:31:04.344'),
    (11, 1, 4, N'PAWN_MOVE', N'd1', N'd2', NULL, NULL, 10438, '2026-08-28T18:00:10.438'),
    (11, 2, 5, N'PAWN_MOVE', N'd7', N'd6', NULL, NULL, 9357, '2026-08-28T18:00:19.795'),
    (11, 3, 4, N'PAWN_MOVE', N'd2', N'd3', NULL, NULL, 8276, '2026-08-28T18:00:28.071'),
    (11, 4, 5, N'WALL', NULL, NULL, N'c3', N'HORIZONTAL', 7195, '2026-08-28T18:00:35.266'),
    (11, 5, 4, N'PAWN_MOVE', N'd3', N'e3', NULL, NULL, 6114, '2026-08-28T18:00:41.380'),
    (11, 6, 5, N'PAWN_MOVE', N'd6', N'd5', NULL, NULL, 5033, '2026-08-28T18:00:46.413'),
    (11, 7, 4, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 3952, '2026-08-28T18:00:50.365'),
    (12, 1, 3, N'PAWN_MOVE', N'e1', N'e2', NULL, NULL, 7167, '2026-09-01T17:30:07.167'),
    (12, 2, 6, N'PAWN_MOVE', N'e9', N'e8', NULL, NULL, 6086, '2026-09-01T17:30:13.253'),
    (12, 3, 3, N'PAWN_MOVE', N'e2', N'e3', NULL, NULL, 5005, '2026-09-01T17:30:18.258'),
    (12, 4, 6, N'WALL', NULL, NULL, N'e4', N'HORIZONTAL', 3924, '2026-09-01T17:30:22.182'),
    (12, 5, 3, N'PAWN_MOVE', N'e3', N'e4', NULL, NULL, 2843, '2026-09-01T17:30:25.025'),
    (12, 6, 6, N'PAWN_MOVE', N'e8', N'e7', NULL, NULL, 10762, '2026-09-01T17:30:35.787');

SET IDENTITY_INSERT dbo.DrawOffer ON;
INSERT INTO dbo.DrawOffer (DrawOfferId, MatchId, OffererId, MoveNumber, OfferedAt, Status, RespondedAt) VALUES
    (1, 4, 3, 4, '2026-07-20T19:00:21', N'EXPIRED', '2026-07-20T19:00:30'),
    (2, 6, 6, 2, '2026-08-10T18:00:19', N'REJECTED', '2026-08-10T18:00:25'),
    (3, 6, 6, 8, '2026-08-10T18:01:01', N'ACCEPTED', '2026-08-10T18:01:11'),
    (4, 12, 6, 6, '2026-09-01T17:30:38', N'PENDING', NULL);
SET IDENTITY_INSERT dbo.DrawOffer OFF;

/*---------------------------------------------------------------------
  Rooms and chat
---------------------------------------------------------------------*/

SET IDENTITY_INSERT dbo.Room ON;
INSERT INTO dbo.Room (RoomId, Code, HostId, GameModeId, WallsPerPlayer, ClockMinutes, Status, MatchId, CreatedAt, LastActivityAt) VALUES
    (1, N'K7MTQ3', 6, 3, 8, 10, N'CLOSED', 10, '2026-08-31T16:20:00', '2026-08-31T16:31:05'),
    (2, N'QX7PRM', 5, 1, 10, 5, N'OPEN', NULL, '2026-09-01T17:50:00', '2026-09-01T17:58:30');
SET IDENTITY_INSERT dbo.Room OFF;

SET IDENTITY_INSERT dbo.Message ON;
INSERT INTO dbo.Message (MessageId, AuthorId, Channel, MatchId, RoomId, Text, SentAt) VALUES
    (1, 9, N'GLOBAL', NULL, NULL, N'Nadie aquí sabe jugar', '2026-08-25T21:00:00'),
    (2, 9, N'MATCH', 9, NULL, N'Juegas horrible, mejor ni lo intentes', '2026-08-26T17:11:05'),
    (3, 3, N'MATCH', 9, NULL, N'Tranquilo, es solo un juego', '2026-08-26T17:11:20'),
    (4, 6, N'ROOM', NULL, 1, N'Welcome! We can start whenever you want', '2026-08-31T16:25:00'),
    (5, 3, N'FRIENDS', NULL, NULL, N'¿Jugamos más tarde?', '2026-09-01T17:15:00'),
    (6, 6, N'MATCH', 12, NULL, N'¡Suerte!', '2026-09-01T17:30:05'),
    (7, 3, N'MATCH', 12, NULL, N'Igualmente, compañera 👍', '2026-09-01T17:30:12'),
    (8, 2, N'SPECTATORS', 12, NULL, N'Buen muro de sofía', '2026-09-01T17:31:00'),
    (9, 5, N'ROOM', NULL, 2, N'¿Listos?', '2026-09-01T17:53:00'),
    (10, 7, N'ROOM', NULL, 2, N'Ya casi', '2026-09-01T17:53:30'),
    (11, 4, N'GLOBAL', NULL, NULL, N'¿Alguien para una rápida?', '2026-09-01T17:56:00');
SET IDENTITY_INSERT dbo.Message OFF;

/*---------------------------------------------------------------------
  Ranking
---------------------------------------------------------------------*/

-- One row per account and active mode (CU-02 RN-06); the counters summarize its ranked matches.
INSERT INTO dbo.ModeStatistic (AccountId, GameModeId, EloRating, LastMatchAt, MatchesPlayed, MatchesWon, MatchesLost, Abandonments) VALUES
    (1, 1, 1000, NULL, 0, 0, 0, 0),
    (1, 2, 1000, NULL, 0, 0, 0, 0),
    (1, 3, 1000, NULL, 0, 0, 0, 0),
    (2, 1, 1000, NULL, 0, 0, 0, 0),
    (2, 2, 1000, NULL, 0, 0, 0, 0),
    (2, 3, 1000, NULL, 0, 0, 0, 0),
    (3, 1, 1055, '2026-08-26T17:11:55', 6, 5, 1, 0),
    (3, 2, 1024, '2026-08-15T19:01:30', 1, 1, 0, 0),
    (3, 3, 1000, NULL, 0, 0, 0, 0),
    (4, 1, 959, '2026-07-26T17:31:40', 5, 1, 4, 0),
    (4, 2, 1008, '2026-08-15T19:01:30', 1, 0, 1, 1),
    (4, 3, 1016, '2026-08-28T18:03:29', 1, 1, 0, 0),
    (5, 1, 1000, NULL, 0, 0, 0, 0),
    (5, 2, 976, '2026-08-15T19:01:30', 1, 0, 1, 1),
    (5, 3, 984, '2026-08-28T18:03:29', 2, 0, 1, 0),
    (6, 1, 1000, NULL, 0, 0, 0, 0),
    (6, 2, 992, '2026-08-15T19:01:30', 1, 0, 1, 1),
    (6, 3, 1000, '2026-08-10T18:01:11', 1, 0, 0, 0),
    (7, 1, 1000, NULL, 0, 0, 0, 0),
    (7, 2, 1000, NULL, 0, 0, 0, 0),
    (7, 3, 1000, NULL, 0, 0, 0, 0),
    (8, 1, 1000, NULL, 0, 0, 0, 0),
    (8, 2, 1000, NULL, 0, 0, 0, 0),
    (8, 3, 1000, NULL, 0, 0, 0, 0),
    (9, 1, 986, '2026-08-26T17:11:55', 1, 0, 1, 0),
    (9, 2, 1000, NULL, 0, 0, 0, 0),
    (9, 3, 1000, NULL, 0, 0, 0, 0),
    (10, 1, 1000, NULL, 0, 0, 0, 0),
    (10, 2, 1000, NULL, 0, 0, 0, 0),
    (10, 3, 1000, NULL, 0, 0, 0, 0),
    (11, 1, 1000, NULL, 0, 0, 0, 0),
    (11, 2, 1000, NULL, 0, 0, 0, 0),
    (11, 3, 1000, NULL, 0, 0, 0, 0);

/*---------------------------------------------------------------------
  Relationships between players
---------------------------------------------------------------------*/

INSERT INTO dbo.Mute (AccountId, MutedId, MutedAt) VALUES
    (4, 9, '2026-08-25T21:05:00');

INSERT INTO dbo.Block (BlockerId, BlockedId, BlockedAt) VALUES
    (3, 9, '2026-08-26T17:25:00');

/*---------------------------------------------------------------------
  Moderation and logs
---------------------------------------------------------------------*/

SET IDENTITY_INSERT dbo.Report ON;
INSERT INTO dbo.Report (ReportId, ReporterId, ReportedId, Reason, Description, MatchId, SubmittedAt, Status, ModeratorId, AssignedAt, ResolutionNote, ResolvedAt) VALUES
    (1, 3, 9, N'OFFENSIVE_LANGUAGE', N'Me insultó en el chat de la partida.', 9, '2026-08-26T17:20:00', N'RESOLVED_SANCTIONED', 2, '2026-08-27T09:50:00', N'Insultos comprobados en el chat de la partida.', '2026-08-27T10:00:00'),
    (2, 4, 9, N'HARASSMENT', N'Molesta a todos en el chat global.', NULL, '2026-08-25T21:06:00', N'PENDING', NULL, NULL, NULL, NULL),
    (3, 6, 3, N'UNSPORTSMANLIKE_CONDUCT', N'Creo que esperó a que abandonáramos.', 7, '2026-08-15T19:30:00', N'IN_REVIEW', 2, '2026-09-01T17:40:00', NULL, NULL),
    (4, 3, 11, N'INAPPROPRIATE_NAME', N'Su nickname imita al del administrador.', NULL, '2026-08-22T20:30:00', N'RESOLVED_SANCTIONED', 2, '2026-08-23T09:55:00', N'El nickname suplanta al equipo del juego.', '2026-08-23T10:00:00'),
    (5, 4, 3, N'CHEATING', N'Me saltó y llegó a la meta muy rápido; creo que usa un programa.', 5, '2026-07-26T17:40:00', N'RESOLVED_NO_SANCTION', 2, '2026-07-27T09:00:00', N'La repetición muestra jugadas legales; el salto es válido (CU-20 RN-03).', '2026-07-27T09:20:00');
SET IDENTITY_INSERT dbo.Report OFF;

-- The replacement is linked afterwards with UPDATE, once it exists (CU-44 RN-09).
SET IDENTITY_INSERT dbo.Sanction ON;
INSERT INTO dbo.Sanction (SanctionId, AccountId, ModeratorId, ReportId, Scope, SanctionType, Reason, StartedAt, EndedAt, RevokedAt, ReplacementSanctionId) VALUES
    (1, 9, 2, 1, N'ACCOUNT', N'TEMPORARY', N'Insultos en el chat de una partida.', '2026-08-27T10:00:00', '2026-08-28T10:00:00', '2026-08-27T16:00:00', NULL),
    (2, 9, 2, 1, N'ACCOUNT', N'TEMPORARY', N'Insultos reiterados; se amplía la suspensión.', '2026-08-27T16:00:00', '2026-09-03T16:00:00', NULL, NULL),
    (3, 5, 2, NULL, N'CHAT', N'TEMPORARY', N'Mensajes repetidos para molestar.', '2026-07-10T12:00:00', '2026-07-11T12:00:00', NULL, NULL),
    (4, 6, 2, NULL, N'CHAT', N'PERMANENT', N'Lenguaje ofensivo en el chat global.', '2026-08-05T18:00:00', NULL, '2026-08-06T12:00:00', NULL),
    (5, 11, 2, 4, N'ACCOUNT', N'PERMANENT', N'Suplanta al equipo del juego con su nickname.', '2026-08-23T10:00:00', NULL, NULL, NULL);
SET IDENTITY_INSERT dbo.Sanction OFF;

SET IDENTITY_INSERT dbo.Appeal ON;
INSERT INTO dbo.Appeal (AppealId, SanctionId, Text, HasFlaggedLanguage, SubmittedAt, Status, ModeratorId, AssignedAt, ResolutionNote, ResolvedAt) VALUES
    (1, 4, N'Fue un malentendido con una palabra en inglés.', 0, '2026-08-05T20:00:00', N'ACCEPTED', 1, '2026-08-06T11:30:00', N'La palabra no era un insulto en su contexto.', '2026-08-06T12:00:00'),
    (2, 2, N'No fue para tanto, solo le dije que jugaba tonto.', 1, '2026-08-29T10:00:00', N'PENDING', NULL, NULL, NULL, NULL),
    (3, 3, N'Solo repetí el mensaje porque nadie me contestaba.', 0, '2026-07-10T13:00:00', N'REJECTED', 1, '2026-07-10T15:00:00', N'Los mensajes repetidos para molestar constan en el chat.', '2026-07-10T15:10:00');
SET IDENTITY_INSERT dbo.Appeal OFF;

SET IDENTITY_INSERT dbo.ModerationLog ON;
INSERT INTO dbo.ModerationLog (ModerationLogId, ModeratorId, Action, AffectedAccountId, Detail, OccurredAt) VALUES
    (1, 1, N'ROLE_GRANTED', 2, N'Rol MODERADOR otorgado.', '2026-06-05T10:00:00'),
    (2, 2, N'SANCTION_APPLIED', 5, N'Sanción 3: chat, temporal.', '2026-07-10T12:00:00'),
    (3, 1, N'APPEAL_RESOLVED', 5, N'Apelación 3 rechazada; se mantiene la sanción 3.', '2026-07-10T15:10:00'),
    (4, 2, N'REPORT_CLAIMED', 3, N'Reporte 5.', '2026-07-27T09:00:00'),
    (5, 2, N'REPORT_RESOLVED', 3, N'Reporte 5 resuelto sin sanción.', '2026-07-27T09:20:00'),
    (6, 2, N'SANCTION_APPLIED', 6, N'Sanción 4: chat, permanente.', '2026-08-05T18:00:00'),
    (7, 1, N'APPEAL_RESOLVED', 6, N'Apelación 1 aceptada; sanción 4 retirada.', '2026-08-06T12:00:00'),
    (8, 2, N'REPORT_CLAIMED', 11, N'Reporte 4.', '2026-08-23T09:55:00'),
    (9, 2, N'SANCTION_APPLIED', 11, N'Sanción 5: cuenta, permanente.', '2026-08-23T10:00:00'),
    (10, 2, N'REPORT_RESOLVED', 11, N'Reporte 4 resuelto con sanción.', '2026-08-23T10:00:00'),
    (11, 2, N'REPORT_CLAIMED', 9, N'Reporte 2.', '2026-08-26T10:00:00'),
    (12, 2, N'REPORT_RELEASED', 9, N'Reporte 2: vuelve a la cola a los treinta minutos (CU-43 RN-03).', '2026-08-26T10:30:00'),
    (13, 2, N'REPORT_CLAIMED', 9, N'Reporte 1.', '2026-08-27T09:50:00'),
    (14, 2, N'SANCTION_APPLIED', 9, N'Sanción 1: cuenta, temporal.', '2026-08-27T10:00:00'),
    (15, 2, N'REPORT_RESOLVED', 9, N'Reporte 1 resuelto con sanción.', '2026-08-27T10:00:00'),
    (16, 2, N'SANCTION_APPLIED', 9, N'Sanción 2: sustituye a la 1.', '2026-08-27T16:00:00'),
    (17, 4, N'UNAUTHORIZED_ATTEMPT', NULL, N'Intentó abrir la cola de reportes.', '2026-08-30T18:00:00'),
    (18, 1, N'LOG_VIEWED', NULL, N'Bitácora de acceso, últimos siete días.', '2026-08-31T09:00:00'),
    (19, 2, N'REPORT_CLAIMED', 3, N'Reporte 3.', '2026-09-01T17:40:00');
SET IDENTITY_INSERT dbo.ModerationLog OFF;

SET IDENTITY_INSERT dbo.AccessLog ON;
INSERT INTO dbo.AccessLog (AccessLogId, AccountId, EnteredIdentifier, Result, IpAddress, OccurredAt) VALUES
    (1, 1, NULL, N'REGISTRATION_SUCCEEDED', N'189.203.14.20', '2026-06-01T10:00:00'),
    (2, 1, NULL, N'ACCOUNT_VERIFIED', N'189.203.14.20', '2026-06-01T10:10:00'),
    (3, 2, NULL, N'REGISTRATION_SUCCEEDED', N'201.141.22.7', '2026-06-02T09:00:00'),
    (4, 2, NULL, N'ACCOUNT_VERIFIED', N'201.141.22.7', '2026-06-02T09:10:00'),
    (5, 3, NULL, N'REGISTRATION_SUCCEEDED', N'189.203.55.3', '2026-06-10T16:00:00'),
    (6, 3, NULL, N'ACCOUNT_VERIFIED', N'189.203.55.3', '2026-06-10T16:10:00'),
    (7, 4, NULL, N'REGISTRATION_SUCCEEDED', N'187.190.3.41', '2026-06-12T18:30:00'),
    (8, 4, NULL, N'ACCOUNT_VERIFIED', N'187.190.3.41', '2026-06-12T18:40:00'),
    (9, 5, NULL, N'REGISTRATION_SUCCEEDED', N'201.141.80.12', '2026-06-15T12:00:00'),
    (10, 5, NULL, N'ACCOUNT_VERIFIED', N'201.141.80.12', '2026-06-15T12:10:00'),
    (11, 6, NULL, N'REGISTRATION_SUCCEEDED', N'187.190.66.5', '2026-06-18T20:00:00'),
    (12, 6, NULL, N'ACCOUNT_VERIFIED', N'187.190.66.5', '2026-06-18T20:10:00'),
    (13, 9, NULL, N'REGISTRATION_SUCCEEDED', N'201.141.90.33', '2026-06-20T15:00:00'),
    (14, 9, NULL, N'ACCOUNT_VERIFIED', N'201.141.90.33', '2026-06-20T15:10:00'),
    (15, 10, NULL, N'REGISTRATION_SUCCEEDED', N'189.203.77.8', '2026-06-22T11:00:00'),
    (16, 10, NULL, N'ACCOUNT_VERIFIED', N'189.203.77.8', '2026-06-22T11:10:00'),
    (17, 2, NULL, N'PASSWORD_CHANGED', N'201.141.22.7', '2026-07-15T12:00:00'),
    (18, 4, N'marta_muros', N'LOGIN_SUCCEEDED', N'187.190.3.41', '2026-08-01T19:00:00'),
    (19, 4, N'marta_muros', N'TWO_FACTOR_ENABLED', N'187.190.3.41', '2026-08-01T19:06:00'),
    (20, 4, NULL, N'VOLUNTARY_LOGOUT', N'187.190.3.41', '2026-08-01T20:30:00'),
    (21, 10, NULL, N'LOGIN_SUCCEEDED', N'189.203.77.8', '2026-08-02T12:30:00'),
    (22, 10, NULL, N'ACCOUNT_DELETED', N'189.203.77.8', '2026-08-02T13:00:00'),
    (23, 4, N'marta_muros', N'TWO_FACTOR_FAILED', N'187.190.3.41', '2026-08-15T20:03:00'),
    (24, 6, N'sofia_salto', N'LOGIN_SUCCEEDED', N'187.190.66.5', '2026-08-19T18:00:00'),
    (25, 6, NULL, N'PASSWORD_RESET_REQUESTED', N'187.190.66.5', '2026-08-20T10:55:00'),
    (26, 6, NULL, N'PASSWORD_RESET_SUCCEEDED', N'187.190.66.5', '2026-08-20T11:05:00'),
    (27, 11, NULL, N'REGISTRATION_SUCCEEDED', N'201.141.33.8', '2026-08-22T19:00:00'),
    (28, 11, NULL, N'ACCOUNT_VERIFIED', N'201.141.33.8', '2026-08-22T19:10:00'),
    (29, 11, N'4dm1n_oficial', N'LOGIN_SUCCEEDED', N'201.141.33.8', '2026-08-22T19:15:00'),
    (30, 11, N'4dm1n_oficial', N'ACCOUNT_SANCTIONED', N'201.141.33.8', '2026-08-24T18:00:00'),
    (31, 9, N'rayo_veloz', N'LOGIN_SUCCEEDED', N'201.141.90.33', '2026-08-26T17:00:00'),
    (32, 8, NULL, N'REGISTRATION_SUCCEEDED', N'201.141.5.60', '2026-08-28T19:00:00'),
    (33, NULL, N'admin', N'REGISTRATION_REJECTED', N'201.141.5.61', '2026-08-29T12:00:00'),
    (34, NULL, N'pepe@correo.mx', N'PASSWORD_RESET_UNKNOWN_EMAIL', N'201.141.8.14', '2026-08-29T13:00:00'),
    (35, 7, NULL, N'GUEST_CREATED', N'189.203.40.2', '2026-08-30T16:00:00'),
    (36, 9, N'rayo_veloz', N'ACCOUNT_SANCTIONED', N'201.141.90.33', '2026-08-30T19:00:00'),
    (37, 1, N'admin_bastion', N'LOGIN_SUCCEEDED', N'189.203.14.20', '2026-08-31T08:00:00'),
    (38, 1, NULL, N'VOLUNTARY_LOGOUT', N'189.203.14.20', '2026-08-31T12:00:00'),
    (39, 3, N'luis_quo', N'LOGIN_SUCCEEDED', N'189.203.55.3', '2026-08-31T20:00:00'),
    (40, 1, N'admin_bastion', N'WRONG_PASSWORD', N'189.203.14.20', '2026-09-01T09:00:00'),
    (41, 1, N'admin_bastion', N'WRONG_PASSWORD', N'189.203.14.20', '2026-09-01T09:01:00'),
    (42, 1, N'admin_bastion', N'WRONG_PASSWORD', N'189.203.14.20', '2026-09-01T09:02:10'),
    (43, 1, N'admin_bastion', N'ACCOUNT_LOCKED', N'189.203.14.20', '2026-09-01T09:04:00'),
    (44, 8, N'nico_nuevo', N'ACCOUNT_PENDING', N'201.141.5.60', '2026-09-01T11:55:00'),
    (45, 2, N'ana_modera', N'LOGIN_SUCCEEDED', N'201.141.22.7', '2026-09-01T17:00:00'),
    (46, NULL, N'luis_qou', N'ACCOUNT_NOT_FOUND', N'189.203.55.9', '2026-09-01T17:04:30'),
    (47, 3, N'luis_quo', N'LOGIN_SUCCEEDED', N'189.203.55.9', '2026-09-01T17:05:00'),
    (48, 3, NULL, N'REMOTE_LOGOUT', N'189.203.55.9', '2026-09-01T17:10:00'),
    (49, 3, NULL, N'EMAIL_CHANGE_REQUESTED', N'189.203.55.9', '2026-09-01T17:20:00'),
    (50, 6, N'sofia_salto', N'LOGIN_SUCCEEDED', N'187.190.66.5', '2026-09-01T17:25:00'),
    (51, 1, NULL, N'PASSWORD_RESET_REQUESTED', N'189.203.14.20', '2026-09-01T17:40:00'),
    (52, 5, N'pedro_peon', N'LOGIN_SUCCEEDED', N'201.141.80.12', '2026-09-01T17:45:00'),
    (53, 1, NULL, N'PASSWORD_RESET_REQUESTED', N'189.203.14.20', '2026-09-01T17:46:00'),
    (54, 4, N'marta_muros', N'LOGIN_SUCCEEDED', N'187.190.3.41', '2026-09-01T17:54:00');
SET IDENTITY_INSERT dbo.AccessLog OFF;

UPDATE dbo.Sanction SET ReplacementSanctionId = 2 WHERE SanctionId = 1;

COMMIT TRANSACTION;
GO

/*---------------------------------------------------------------------
  Check: every row must show 0 mismatches. Recalculates in the
  database itself the controlled redundancies and the rules the test
  data must satisfy.
---------------------------------------------------------------------*/
SELECT CheckName, Mismatches FROM (
    SELECT 1 AS CheckNumber, N'Matches, wins and losses per mode' AS CheckName, COUNT(*) AS Mismatches
    FROM dbo.ModeStatistic AS ms
    CROSS APPLY (SELECT COUNT(*) AS Played,
                        SUM(CASE WHEN p.Result = 'WON' THEN 1 ELSE 0 END) AS Won,
                        SUM(CASE WHEN p.Result = 'LOST' THEN 1 ELSE 0 END) AS Lost
                 FROM dbo.Participation AS p JOIN dbo.Match AS m ON m.MatchId = p.MatchId
                 WHERE p.AccountId = ms.AccountId AND m.GameModeId = ms.GameModeId
                   AND m.MatchType = 'RANKED' AND p.Result IS NOT NULL) AS c
    WHERE ms.MatchesPlayed <> c.Played OR ms.MatchesWon <> ISNULL(c.Won, 0) OR ms.MatchesLost <> ISNULL(c.Lost, 0)
    UNION ALL
    SELECT 2, N'Current elo equals the last final elo', COUNT(*)
    FROM dbo.ModeStatistic AS ms
    WHERE ms.EloRating <> ISNULL((SELECT TOP (1) p.FinalElo FROM dbo.Participation AS p
                                  JOIN dbo.Match AS m ON m.MatchId = p.MatchId
                                  WHERE p.AccountId = ms.AccountId AND m.GameModeId = ms.GameModeId AND p.FinalElo IS NOT NULL
                                  ORDER BY m.EndedAt DESC), 1000)
    UNION ALL
    SELECT 3, N'Remaining walls: the initial ones minus the ones placed', COUNT(*)
    FROM dbo.Participation AS p
    JOIN dbo.Match AS m ON m.MatchId = p.MatchId
    JOIN dbo.GameMode AS gm ON gm.GameModeId = m.GameModeId
    LEFT JOIN dbo.Room AS r ON r.MatchId = m.MatchId
    WHERE p.RemainingWalls <> COALESCE(r.WallsPerPlayer, gm.WallsPerPlayer)
          - (SELECT COUNT(*) FROM dbo.Move AS mv WHERE mv.MatchId = p.MatchId AND mv.AccountId = p.AccountId
             AND mv.MoveType = 'WALL')
    UNION ALL
    SELECT 4, N'Remaining clock: the initial one minus the time spent, or zero if it ran out', COUNT(*)
    FROM dbo.Participation AS p JOIN dbo.Match AS m ON m.MatchId = p.MatchId
    WHERE p.RemainingClock <> CASE WHEN m.EndReason = 'TIMEOUT' AND p.Result = 'LOST' THEN 0
          ELSE m.ClockMinutes * 60000
          - ISNULL((SELECT SUM(mv.TimeSpent) FROM dbo.Move AS mv WHERE mv.MatchId = p.MatchId AND mv.AccountId = p.AccountId), 0) END
    UNION ALL
    SELECT 5, N'Current square equals the one of the last move', COUNT(*)
    FROM dbo.Participation AS p
    CROSS APPLY (SELECT TOP (1) mv.ToSquare FROM dbo.Move AS mv
                 WHERE mv.MatchId = p.MatchId AND mv.AccountId = p.AccountId AND mv.MoveType = 'PAWN_MOVE'
                 ORDER BY mv.MoveNumber DESC) AS lastMove
    WHERE p.CurrentSquare <> lastMove.ToSquare
    UNION ALL
    SELECT 6, N'Statistics for every account in every active mode (CU-02 RN-06)', COUNT(*)
    FROM dbo.Account AS a CROSS JOIN dbo.GameMode AS gm
    WHERE gm.IsActive = 1 AND NOT EXISTS (SELECT 1 FROM dbo.ModeStatistic AS ms WHERE ms.AccountId = a.AccountId AND ms.GameModeId = gm.GameModeId)
    UNION ALL
    SELECT 7, N'One winner in every decided match', COUNT(*)
    FROM dbo.Match AS m
    WHERE m.Status = 'FINISHED' AND m.EndReason <> 'DRAW'
      AND (SELECT COUNT(*) FROM dbo.Participation AS p WHERE p.MatchId = m.MatchId AND p.Result = 'WON') <> 1
    UNION ALL
    SELECT 8, N'Two or four participants per match, depending on its mode', COUNT(*)
    FROM dbo.Match AS m JOIN dbo.GameMode AS gm ON gm.GameModeId = m.GameModeId
    WHERE (SELECT COUNT(*) FROM dbo.Participation AS p WHERE p.MatchId = m.MatchId) <> gm.PlayerCount
    UNION ALL
    SELECT 9, N'Second factor enabled with an issued code (CU-09 FA-10)', COUNT(*)
    FROM dbo.Account AS a
    WHERE a.IsTwoFactorEnabled = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.VerificationCode AS vc WHERE vc.AccountId = a.AccountId AND vc.Purpose = 'TWO_FACTOR')
    UNION ALL
    SELECT 10, N'Texts with ñ, accents and emojis identical to the inserted ones (D-21)', COUNT(*)
    FROM (
          SELECT e.v FROM (VALUES
                  (N'est' + NCHAR(250) + N'pido')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.BannedWord AS t WHERE t.Term = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'Nadie aqu' + NCHAR(237) + N' sabe jugar'),
                  (NCHAR(191) + N'Jugamos m' + NCHAR(225) + N's tarde?'),
                  (NCHAR(161) + N'Suerte!'),
                  (N'Igualmente, compa' + NCHAR(241) + N'era ' + NCHAR(128077)),
                  (N'Buen muro de sof' + NCHAR(237) + N'a'),
                  (NCHAR(191) + N'Listos?'),
                  (NCHAR(191) + N'Alguien para una r' + NCHAR(225) + N'pida?')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.Message AS t WHERE t.Text = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'Me insult' + NCHAR(243) + N' en el chat de la partida.'),
                  (N'Creo que esper' + NCHAR(243) + N' a que abandon' + NCHAR(225) + N'ramos.'),
                  (N'Me salt' + NCHAR(243) + N' y lleg' + NCHAR(243) + N' a la meta muy r' + NCHAR(225) + N'pido; creo que usa un programa.')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.Report AS t WHERE t.Description = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'La repetici' + NCHAR(243) + N'n muestra jugadas legales; el salto es v' + NCHAR(225) + N'lido (CU-20 RN-03).')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.Report AS t WHERE t.ResolutionNote = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'Insultos reiterados; se ampl' + NCHAR(237) + N'a la suspensi' + NCHAR(243) + N'n.')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.Sanction AS t WHERE t.Reason = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'Fue un malentendido con una palabra en ingl' + NCHAR(233) + N's.'),
                  (N'Solo repet' + NCHAR(237) + N' el mensaje porque nadie me contestaba.')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.Appeal AS t WHERE t.Text = e.v COLLATE Latin1_General_100_BIN2)
          UNION ALL
          SELECT e.v FROM (VALUES
                  (N'Sanci' + NCHAR(243) + N'n 3: chat, temporal.'),
                  (N'Apelaci' + NCHAR(243) + N'n 3 rechazada; se mantiene la sanci' + NCHAR(243) + N'n 3.'),
                  (N'Reporte 5 resuelto sin sanci' + NCHAR(243) + N'n.'),
                  (N'Sanci' + NCHAR(243) + N'n 4: chat, permanente.'),
                  (N'Apelaci' + NCHAR(243) + N'n 1 aceptada; sanci' + NCHAR(243) + N'n 4 retirada.'),
                  (N'Sanci' + NCHAR(243) + N'n 5: cuenta, permanente.'),
                  (N'Reporte 4 resuelto con sanci' + NCHAR(243) + N'n.'),
                  (N'Sanci' + NCHAR(243) + N'n 1: cuenta, temporal.'),
                  (N'Reporte 1 resuelto con sanci' + NCHAR(243) + N'n.'),
                  (N'Sanci' + NCHAR(243) + N'n 2: sustituye a la 1.'),
                  (N'Intent' + NCHAR(243) + N' abrir la cola de reportes.'),
                  (N'Bit' + NCHAR(225) + N'cora de acceso, ' + NCHAR(250) + N'ltimos siete d' + NCHAR(237) + N'as.')) AS e(v)
          WHERE NOT EXISTS (SELECT 1 FROM dbo.ModerationLog AS t WHERE t.Detail = e.v COLLATE Latin1_General_100_BIN2)
         ) AS mismatched
) AS c
ORDER BY CheckNumber;

-- Rows loaded per table.
SELECT t.name AS TableName, SUM(p.rows) AS LoadedRows
FROM sys.tables AS t
JOIN sys.partitions AS p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;
GO
