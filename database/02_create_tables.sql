/*=====================================================================
  Bastion - Core table creation (2 of 3)
  Engine: SQL Server 2019 or later (CON-04).

  Run order:
     1. 01_create_database.sql
     2. 02_create_tables.sql      <- this file
     3. 03_insert_test_data.sql
  Later, only when the later phase starts, run the later-phase scripts
  (their tables first, then their test data).

  Creates the core tables in the empty Bastion database: the ones
  required by the client's mandatory use cases and by the gameplay use
  cases (section "Requirements and use case prioritization"). The tables
  for the team's additions (cosmetics, shop, boxes, coins, divisions,
  friends, spectator, AI, tutorial, profile) belong to the later phase
  and are not created here (D-22).

  Contents:
     1. Preloaded catalogs (D-09)
     2. Account and access
     3. Match
     4. Rooms and chat
     5. Ranking
     6. Relationships between players
     7. Moderation and logs
     8. Procedures for physical deletes

  Conventions:
    - Tables in singular PascalCase and columns in PascalCase, in
      English, so they match EF Core's default property mapping.
    - Constraints carry their kind as a prefix: PK_ primary key,
      FK_ foreign key, UQ_ unique, CK_ check; UX_ filtered unique
      index, IX_ index.
    - Dates in UTC with DATETIME2(0).
    - Text that a person writes or reads is NVARCHAR (CON-05). The
      database uses the Modern_Spanish_100_CI_AS_SC_UTF8 collation, so
      all text accepts ñ, accented letters and any Unicode character.
    - Nothing is stored translated: catalogs and domains store a code
      that is the key of its name in the resource dictionaries (D-21).
    - Every column has a comment. The attribute tables of the data
      model document are generated from these comments, so each
      comment is the description shown in the document.
=====================================================================*/

USE Bastion;
GO
-- Filtered indexes and computed columns require these options when the
-- tables are created or altered; sqlcmd turns them off by default.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*---------------------------------------------------------------------
  1. Preloaded catalogs (D-09)
  No use case writes them: they are loaded through SQL.
---------------------------------------------------------------------*/

-- @entity GameMode | Catalog of game modes; each mode sets the board, the players and the walls (D-04).
-- @normalization Simple key; `Code` is a candidate key. Holds PlayerCount, which in the diagram was on *Match* and depended on it through the mode.
CREATE TABLE dbo.GameMode (
    GameModeId         TINYINT        NOT NULL,  -- Mode identifier.
    Code               VARCHAR(40)    NOT NULL,  -- Key of the mode in the resource dictionaries (D-21): `CLASSIC`, `FOUR_PLAYERS` or `QUICK`.
    BoardSize          TINYINT        NOT NULL,  -- Squares per side: 7 or 9.
    PlayerCount        TINYINT        NOT NULL,  -- Seats in the match: 2 or 4, never 3 (CU-17 RN-02).
    WallsPerPlayer     TINYINT        NOT NULL,  -- Walls for each player: 10, 5 or 6 (CU-21 RN-02).
    IsActive           BIT            NOT NULL DEFAULT (1),  -- Only active modes get statistics when an account signs up (CU-02).
    CONSTRAINT PK_GameMode PRIMARY KEY (GameModeId),
    CONSTRAINT UQ_GameMode_Code UNIQUE (Code),  -- No two modes share the same key.
    CONSTRAINT CK_GameMode_Parameters CHECK (BoardSize IN (7, 9) AND PlayerCount IN (2, 4) AND WallsPerPlayer BETWEEN 1 AND 20)
);
GO

-- @entity BannedWord | Catalog for the word filter the server applies to nicknames and chat (CON-09).
-- @normalization Surrogate key; the natural key (`Term`, `Language`, `Scope`) is declared unique. No active flag: no table references the catalog, so a term that is no longer filtered is deleted through SQL (D-09).
CREATE TABLE dbo.BannedWord (
    BannedWordId       INT IDENTITY(1,1) NOT NULL,  -- Term identifier.
    Term               NVARCHAR(100) COLLATE Latin1_General_100_CI_AI_SC_UTF8 NOT NULL,  -- Banned term, with its ñ and accents; compared ignoring case and accents.
    Language           VARCHAR(10)    NULL,  -- Language of the term: `es-MX` or `en`; null if it is filtered in every language (CU-28 RN-01).
    Scope              VARCHAR(10)    NOT NULL,  -- `NICKNAME`, `CHAT` or `BOTH`.
    CONSTRAINT PK_BannedWord PRIMARY KEY (BannedWordId),
    CONSTRAINT UQ_BannedWord_Term UNIQUE (Term, Language, Scope),  -- A term is not repeated in the same language and scope.
    CONSTRAINT CK_BannedWord_Language CHECK (Language IN ('es-MX','en')),  -- Supported languages (D-21); a null passes the check.
    CONSTRAINT CK_BannedWord_Scope CHECK (Scope IN ('NICKNAME','CHAT','BOTH'))
);
GO

/*---------------------------------------------------------------------
  2. Account and access
---------------------------------------------------------------------*/

-- @entity Account | Account of a player, moderator or administrator. It is the central entity: almost every other one depends on it and its row is never deleted (D-07, D-17).
-- @normalization Simple key; `Nickname` is a candidate key. Without the diagram's composite attribute full_name or its derived attribute age. Terms acceptance lives here: only the last accepted version is queried, and it depends on the account (D-23).
CREATE TABLE dbo.Account (
    AccountId                        INT IDENTITY(1,1) NOT NULL,  -- Account identifier; it does not change when a guest is linked (D-02).
    AccountType                      VARCHAR(10)    NOT NULL DEFAULT ('REGISTERED'),  -- `REGISTERED`, or `GUEST` in the later phase (CU-06). Kept from the core because making `Email` nullable later would force its unique index to be rebuilt (D-22).
    AccountStatus                    VARCHAR(10)    NOT NULL DEFAULT ('PENDING'),  -- `PENDING`, `ACTIVE`, `SUSPENDED`, `BANNED` or `DELETED` (CU-01 RN-04).
    Role                             VARCHAR(13)    NOT NULL DEFAULT ('PLAYER'),  -- `PLAYER`, `MODERATOR` or `ADMINISTRATOR`; exactly one (CU-46 RN-03).
    Nickname                         NVARCHAR(30) COLLATE Latin1_General_100_CI_AI_SC_UTF8 NOT NULL,  -- Display name, 3 to 30 characters; unique ignoring case and accents (CU-02 RN-01).
    Email                            NVARCHAR(254)  NULL,  -- Email normalized to lowercase; null only for guests (CU-02 RN-02).
    PasswordHash                     VARBINARY(64)  NULL,  -- Cryptographic hash of the password; never in plain text (CU-01 RN-02).
    PasswordSalt                     VARBINARY(32)  NULL,  -- Salt of the hash.
    BirthDate                        DATE           NULL,  -- Used to check the minimum age of eight; the age is not stored (CU-02).
    PreferredLanguage                VARCHAR(10)    NOT NULL DEFAULT ('en'),  -- Language of the interface and the emails, `en` (the default) or `es-MX` (D-21); it follows the player to any device (CU-12 RN-08).
    FailedAttempts                   TINYINT        NOT NULL DEFAULT (0),  -- Consecutive failed sign-in attempts (CU-01 RN-03).
    LastFailedAttemptAt              DATETIME2(0)   NULL,  -- Used to reset the counter after 24 hours (CU-01 RN-03).
    LockedUntil                      DATETIME2(0)   NULL,  -- End of the current escalating lockout.
    IsTwoFactorEnabled               BIT            NOT NULL DEFAULT (0),  -- Whether the account requires a second factor (CU-01 RN-08).
    RegisteredAt                     DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Account sign-up.
    LastVerificationSentAt           DATETIME2(0)   NULL,  -- Last verification email that was actually sent; throttles resending (CU-01 RN-10). Not taken from the token: if sending fails, the token exists and this date does not change.
    LastRecoverySentAt               DATETIME2(0)   NULL,  -- Last recovery email that was actually sent; throttles sending (CU-03 RN-06).
    TermsVersion                     VARCHAR(20)    NULL,  -- Last version of the terms of use the account accepted (CU-02 RN-12); replaces the TermsAcceptance table (D-23). Null only for guests.
    TermsLanguage                    VARCHAR(10)    NULL,  -- Language in which the accepted text was shown, `es-MX` or `en` (D-21).
    TermsAcceptedAt                  DATETIME2(0)   NULL,  -- Moment of acceptance.
    DeletedAt                        DATETIME2(0)   NULL,  -- Moment of deletion, which is final: the account is anonymized in the same transaction (D-07, D-17).
    CONSTRAINT PK_Account PRIMARY KEY (AccountId),
    CONSTRAINT UQ_Account_Nickname UNIQUE (Nickname),  -- The nickname is a sign-in identifier (CU-01 RN-14).
    CONSTRAINT CK_Account_AccountType CHECK (AccountType IN ('GUEST','REGISTERED')),
    CONSTRAINT CK_Account_AccountStatus CHECK (AccountStatus IN ('PENDING','ACTIVE','SUSPENDED','BANNED','DELETED')),
    CONSTRAINT CK_Account_Role CHECK (Role IN ('PLAYER','MODERATOR','ADMINISTRATOR')),
    CONSTRAINT CK_Account_PreferredLanguage CHECK (PreferredLanguage IN ('es-MX','en')),  -- Supported languages (D-21); any other is rejected (CU-12 FA-07).
    CONSTRAINT CK_Account_Credentials CHECK (
        (AccountType = 'GUEST' AND Email IS NULL AND PasswordHash IS NULL AND PasswordSalt IS NULL)
        OR (AccountType = 'REGISTERED' AND Email IS NOT NULL)),
    CONSTRAINT CK_Account_RegisteredRole CHECK (Role = 'PLAYER' OR AccountType = 'REGISTERED'),
    CONSTRAINT CK_Account_Deletion CHECK (
        (AccountStatus = 'DELETED' AND DeletedAt IS NOT NULL)
        OR (AccountStatus <> 'DELETED' AND DeletedAt IS NULL)),
    CONSTRAINT CK_Account_TermsLanguage CHECK (TermsLanguage IN ('es-MX','en')),  -- Supported languages (D-21).
    CONSTRAINT CK_Account_Terms CHECK (
        (AccountType = 'REGISTERED' AND TermsVersion IS NOT NULL AND TermsLanguage IS NOT NULL AND TermsAcceptedAt IS NOT NULL)
        OR (AccountType = 'GUEST' AND TermsVersion IS NULL AND TermsLanguage IS NULL AND TermsAcceptedAt IS NULL))  -- Every registered account accepted the terms; a guest accepts them when linking (CU-07).
);
GO
CREATE UNIQUE INDEX UX_Account_Email ON dbo.Account (Email) WHERE Email IS NOT NULL;  -- The email is unique among the accounts that have one (CU-02 RN-02).
GO

-- @entity Session | Session opened from a device. An account can have several at once (D-01); it is not deleted when closed.
-- @normalization Simple key; `TokenHash` is a candidate key.
CREATE TABLE dbo.Session (
    SessionId          INT IDENTITY(1,1) NOT NULL,  -- Session identifier.
    AccountId          INT            NOT NULL,  -- Account that owns the session.
    TokenHash          VARBINARY(32)  NOT NULL,  -- Hash of the session token.
    StartedAt          DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Session opening.
    ExpiresAt          DATETIME2(0)   NOT NULL,  -- Maximum validity of twenty-four hours (CU-01 RN-07).
    LastUsedAt         DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Used to spot forgotten sessions (CU-10); updated by the minute.
    EndedAt            DATETIME2(0)   NULL,  -- Explicit close; null while it is open.
    CloseReason        VARCHAR(20)    NULL,  -- `VOLUNTARY_LOGOUT`, `REMOTE_LOGOUT`, `CREDENTIALS_CHANGED`, `ACCOUNT_DELETED` or `SANCTION`.
    IpAddress          VARCHAR(45)    NOT NULL,  -- Source network address.
    DeviceFingerprint  VARCHAR(128)   NOT NULL,  -- Device fingerprint.
    DeviceName         NVARCHAR(100)  NULL,  -- Readable device name (D-01).
    ClientVersion      VARCHAR(20)    NOT NULL,  -- Version of the client that opened the session.
    CONSTRAINT PK_Session PRIMARY KEY (SessionId),
    CONSTRAINT UQ_Session_TokenHash UNIQUE (TokenHash),  -- A token identifies a single session.
    CONSTRAINT FK_Session_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account opens many sessions; each session belongs to one account.
    CONSTRAINT CK_Session_CloseReason CHECK (CloseReason IN ('VOLUNTARY_LOGOUT','REMOTE_LOGOUT','CREDENTIALS_CHANGED','ACCOUNT_DELETED','SANCTION')),
    CONSTRAINT CK_Session_Closure CHECK ((EndedAt IS NULL AND CloseReason IS NULL) OR (EndedAt IS NOT NULL AND CloseReason IS NOT NULL)),
    CONSTRAINT CK_Session_Validity CHECK (ExpiresAt > StartedAt)
);
GO
CREATE INDEX IX_Session_Open ON dbo.Session (AccountId) WHERE EndedAt IS NULL;
GO

-- @entity VerificationCode | Single-use code or link sent to the account: verify the sign-up, confirm an email change, recover the password or complete the second factor. Replaces EmailVerificationToken, RecoveryToken and SecondFactorCode (D-23).
-- @normalization Simple key; `Purpose` determines whether `TargetEmail` applies. The three tables it replaces had the same columns and the same life cycle.
CREATE TABLE dbo.VerificationCode (
    VerificationCodeId INT IDENTITY(1,1) NOT NULL,  -- Code identifier.
    AccountId          INT            NOT NULL,  -- Account it was sent to.
    Purpose            VARCHAR(15)    NOT NULL,  -- `REGISTRATION`, `EMAIL_CHANGE`, `PASSWORD_RESET` or `TWO_FACTOR`. Determines the validity period and whether `TargetEmail` applies.
    CodeHash           VARBINARY(32)  NOT NULL,  -- Hash of the code or of the link token; never in plain text (CU-01 RN-02, CU-04 RN-06).
    TargetEmail        NVARCHAR(254)  NULL,  -- New email; only for `EMAIL_CHANGE` (D-20). For the others it is the account's email and is not repeated.
    IssuedAt           DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Issue time.
    ExpiresAt          DATETIME2(0)   NOT NULL,  -- Twenty-four hours for `REGISTRATION` and `EMAIL_CHANGE` (CU-04 RN-02), thirty minutes for `PASSWORD_RESET` (CU-03 RN-04) and five for `TWO_FACTOR` (CU-01 RN-09).
    Attempts           TINYINT        NOT NULL DEFAULT (0),  -- Verification attempts of a typed code; three at most (CU-01 RN-09, CU-03 RN-09).
    Status             VARCHAR(11)    NOT NULL DEFAULT ('PENDING'),  -- `PENDING`, `USED` or `INVALIDATED`.
    CONSTRAINT PK_VerificationCode PRIMARY KEY (VerificationCodeId),
    CONSTRAINT FK_VerificationCode_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account receives many codes over time.
    CONSTRAINT CK_VerificationCode_Purpose CHECK (
        (Purpose = 'EMAIL_CHANGE' AND TargetEmail IS NOT NULL)
        OR (Purpose IN ('REGISTRATION','PASSWORD_RESET','TWO_FACTOR') AND TargetEmail IS NULL)),
    CONSTRAINT CK_VerificationCode_Attempts CHECK (Attempts BETWEEN 0 AND 3),
    CONSTRAINT CK_VerificationCode_Status CHECK (Status IN ('PENDING','USED','INVALIDATED')),
    CONSTRAINT CK_VerificationCode_Validity CHECK (ExpiresAt > IssuedAt)
);
GO
CREATE UNIQUE INDEX UX_VerificationCode_Current ON dbo.VerificationCode (AccountId, Purpose) WHERE Status = 'PENDING';  -- A single current code per account and purpose (CU-01 RN-09, CU-03 RN-05, CU-04 RN-03).
CREATE UNIQUE INDEX UX_VerificationCode_Link ON dbo.VerificationCode (CodeHash) WHERE Purpose IN ('REGISTRATION','EMAIL_CHANGE');  -- An email link identifies a single code; typed codes are short and may repeat across accounts.
GO

/*---------------------------------------------------------------------
  3. Match
---------------------------------------------------------------------*/

-- @entity Match | Ranked or private match. It is stored in full because the history, the replay, reconnection and reports depend on it.
-- @normalization Simple key. Without the diagram's player count or duration (transitive dependency and derived value). `MatchType` tells the ranked match, which awards elo, from the private one. No winner column: it is the participation with result `WON`. `CurrentTurn` is controlled redundancy of the current state; the *Move* rows prevail.
CREATE TABLE dbo.Match (
    MatchId              INT IDENTITY(1,1) NOT NULL,  -- Match identifier.
    GameModeId           TINYINT        NOT NULL,  -- Mode; the board, seats and walls come from it and are not repeated here.
    MatchType            VARCHAR(15)    NOT NULL,  -- `RANKED` or `PRIVATE`; the later phase adds `AI` (CU-27).
    Status               VARCHAR(20)    NOT NULL DEFAULT ('IN_PROGRESS'),  -- `IN_PROGRESS`, `PENDING_CLOSE` or `FINISHED` (CU-25 RN-13).
    ClockMinutes         TINYINT        NOT NULL,  -- 3, 5 or 10. In the later phase it accepts null for the AI without a clock (CU-27 RN-06).
    StartedAt            DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Start. The duration is calculated, not stored (CU-36 RN-02).
    EndedAt              DATETIME2(0)   NULL,  -- End of the match.
    EndReason            VARCHAR(15)    NULL,  -- `GOAL`, `TIMEOUT`, `RESIGNATION`, `DRAW` or `ABANDONMENT` (CU-25 RN-01).
    CurrentTurn          TINYINT        NOT NULL DEFAULT (1),  -- `TurnOrder` of the participant who has the turn. Controlled redundancy: it is derived from the *Move* rows, which prevail if they disagree.
    CONSTRAINT PK_Match PRIMARY KEY (MatchId),
    CONSTRAINT FK_Match_GameMode FOREIGN KEY (GameModeId) REFERENCES dbo.GameMode (GameModeId),  -- 1:N | A mode is played in many matches; each match has one mode.
    CONSTRAINT CK_Match_MatchType CHECK (MatchType IN ('RANKED','PRIVATE')),
    CONSTRAINT CK_Match_Status CHECK (Status IN ('IN_PROGRESS','PENDING_CLOSE','FINISHED')),
    CONSTRAINT CK_Match_EndReason CHECK (EndReason IN ('GOAL','TIMEOUT','RESIGNATION','DRAW','ABANDONMENT')),
    CONSTRAINT CK_Match_ClockMinutes CHECK (ClockMinutes IN (3, 5, 10)),
    CONSTRAINT CK_Match_Closure CHECK (
        (Status = 'IN_PROGRESS' AND EndedAt IS NULL AND EndReason IS NULL)
        OR (Status = 'PENDING_CLOSE' AND EndedAt IS NULL AND EndReason IS NOT NULL)
        OR (Status = 'FINISHED' AND EndedAt IS NOT NULL AND EndReason IS NOT NULL))
);
GO

-- @entity Participation | A player's participation in a match; it is the N:M relationship between Account and Match (the diagram's Participates relationship).
-- @normalization Candidate keys (MatchId, AccountId) and (MatchId, TurnOrder). `EloChange` and `IsFinished` are computed: they depended on other columns of the row. `CurrentSquare`, `RemainingWalls` and `RemainingClock` are controlled redundancy of the current state; the *Move* rows prevail.
CREATE TABLE dbo.Participation (
    MatchId              INT            NOT NULL,  -- Match.
    AccountId            INT            NOT NULL,  -- Player.
    TurnOrder            TINYINT        NOT NULL,  -- Order in which the player plays, from 1 to 4.
    PawnSymbol           VARCHAR(10)    NOT NULL,  -- Symbol and color that identify the pawn (piece color in the diagram).
    CurrentSquare        VARCHAR(3)     NOT NULL,  -- Square the pawn occupies, in board notation. Controlled redundancy: it is the one from its last move.
    RemainingWalls       TINYINT        NOT NULL,  -- Walls left; starts with those of the mode or the room (CU-18). Controlled redundancy: the initial ones minus the player's `WALL` moves.
    RemainingClock       INT            NULL,  -- Milliseconds left; null only in the later phase, for the AI without a clock. Controlled redundancy: the initial clock minus the time spent on the player's moves, or zero for whoever loses on time, because the move that arrived late is not stored (CU-20 FA-06).
    InitialElo           SMALLINT       NULL,  -- Elo the player entered with; basis of the calculation (CU-17 RN-04, CU-25 RN-04).
    FinalElo             SMALLINT       NULL,  -- Resulting elo; only in ranked matches.
    EloChange            AS (FinalElo - InitialElo),  -- Elo change shown in the history (CU-36 RN-03). Computed and not persisted: it depends on two other columns of the row.
    Result               VARCHAR(10)    NULL,  -- `WON`, `LOST` or `DRAW`; null until the participation ends.
    Placement            TINYINT        NULL,  -- Placement in the four-player mode, from 1 to 4 (CU-25 RN-02); null in two-player matches, where the result already tells it.
    EndReason            VARCHAR(15)    NULL,  -- How the player left before the match ended, in four-player mode: `GOAL`, `RESIGNATION`, `ABANDONMENT` or `TIMEOUT`.
    IsConnected          BIT            NOT NULL DEFAULT (1),  -- Connection state; stored so it survives a restart (CU-26).
    DisconnectedAt       DATETIME2(0)   NULL,  -- Start of the last disconnection, for the sixty-second window (CU-24 RN-06); not cleared on reconnection, so it does not replace `IsConnected`.
    IsFinished           AS (CASE WHEN Result IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END),  -- Whether the participation is closed (D-11). Computed: it is exactly `Result` not being null.
    CONSTRAINT PK_Participation PRIMARY KEY (MatchId, AccountId),
    CONSTRAINT UQ_Participation_TurnOrder UNIQUE (MatchId, TurnOrder),  -- Two players do not share a turn in the same match.
    CONSTRAINT FK_Participation_Match FOREIGN KEY (MatchId) REFERENCES dbo.Match (MatchId),  -- 1:N | A match has 2 or 4 participations, depending on its mode.
    CONSTRAINT FK_Participation_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account takes part in many matches.
    CONSTRAINT CK_Participation_Result CHECK (Result IN ('WON','LOST','DRAW')),
    CONSTRAINT CK_Participation_EndReason CHECK (EndReason IN ('GOAL','TIMEOUT','RESIGNATION','ABANDONMENT')),
    CONSTRAINT CK_Participation_Ranges CHECK (TurnOrder BETWEEN 1 AND 4 AND Placement BETWEEN 1 AND 4 AND RemainingWalls <= 20),
    CONSTRAINT CK_Participation_Connection CHECK (IsConnected = 1 OR DisconnectedAt IS NOT NULL)
);
GO
CREATE UNIQUE INDEX UX_Participation_Unfinished ON dbo.Participation (AccountId) WHERE Result IS NULL;  -- A single unfinished participation per player (D-11). Filters on `Result` because a filtered index does not accept computed columns.
GO

-- @entity Move | Pawn move or wall placement, in order. The board is rebuilt by replaying the moves; walls have no table of their own (CU-21).
-- @normalization Composite key; every attribute describes the whole move. `MoveType` determines which squares apply.
CREATE TABLE dbo.Move (
    MatchId              INT            NOT NULL,  -- Match.
    MoveNumber           SMALLINT       NOT NULL,  -- Sequential number within the match (CU-20 RN-06).
    AccountId            INT            NOT NULL,  -- Player who made it. The later phase makes it nullable for the AI's moves (CU-27 RN-03).
    MoveType             VARCHAR(10)    NOT NULL,  -- `PAWN_MOVE` or `WALL`. Determines which squares apply.
    FromSquare           VARCHAR(3)     NULL,  -- Square the pawn leaves; only for `PAWN_MOVE`.
    ToSquare             VARCHAR(3)     NULL,  -- Square the pawn reaches; only for `PAWN_MOVE`.
    Groove               VARCHAR(3)     NULL,  -- Groove where the wall was placed; only for `WALL`.
    Orientation          VARCHAR(10)    NULL,  -- `HORIZONTAL` or `VERTICAL`; only for `WALL`.
    TimeSpent            INT            NOT NULL,  -- Milliseconds the player took.
    PlayedAt             DATETIME2(3)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment it was recorded.
    CONSTRAINT PK_Move PRIMARY KEY (MatchId, MoveNumber),
    CONSTRAINT FK_Move_Match FOREIGN KEY (MatchId) REFERENCES dbo.Match (MatchId),  -- 1:N | A match has many moves.
    CONSTRAINT FK_Move_Participation FOREIGN KEY (MatchId, AccountId) REFERENCES dbo.Participation (MatchId, AccountId),  -- 1:N | A participant makes many moves.
    CONSTRAINT CK_Move_MoveType CHECK (
        (MoveType = 'PAWN_MOVE' AND FromSquare IS NOT NULL AND ToSquare IS NOT NULL AND Groove IS NULL AND Orientation IS NULL)
        OR (MoveType = 'WALL' AND Groove IS NOT NULL AND Orientation IN ('HORIZONTAL','VERTICAL') AND FromSquare IS NULL AND ToSquare IS NULL)),
    CONSTRAINT CK_Move_Ranges CHECK (MoveNumber >= 1 AND TimeSpent >= 0)
);
GO

-- @entity DrawOffer | Draw offer in a two-player match (CU-22). Stored for reconnection and as evidence of harassment.
-- @normalization Simple key.
CREATE TABLE dbo.DrawOffer (
    DrawOfferId          INT IDENTITY(1,1) NOT NULL,  -- Offer identifier.
    MatchId              INT            NOT NULL,  -- Match.
    OffererId            INT            NOT NULL,  -- Participant who makes the offer.
    MoveNumber           SMALLINT       NOT NULL,  -- Move after which it was offered; limits offers to one every five moves (CU-22 RN-03).
    OfferedAt            DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of the offer.
    Status               VARCHAR(10)    NOT NULL DEFAULT ('PENDING'),  -- `PENDING`, `ACCEPTED`, `REJECTED` or `EXPIRED`.
    RespondedAt          DATETIME2(0)   NULL,  -- Moment it was accepted, rejected or expired.
    CONSTRAINT PK_DrawOffer PRIMARY KEY (DrawOfferId),
    CONSTRAINT FK_DrawOffer_Participation FOREIGN KEY (MatchId, OffererId) REFERENCES dbo.Participation (MatchId, AccountId),  -- 1:N | A participant makes many offers in their match.
    CONSTRAINT CK_DrawOffer_Status CHECK (Status IN ('PENDING','ACCEPTED','REJECTED','EXPIRED')),
    CONSTRAINT CK_DrawOffer_Response CHECK ((Status = 'PENDING' AND RespondedAt IS NULL) OR (Status <> 'PENDING' AND RespondedAt IS NOT NULL))
);
GO
CREATE UNIQUE INDEX UX_DrawOffer_Pending ON dbo.DrawOffer (MatchId) WHERE Status = 'PENDING';  -- At most one pending offer per match; a second one counts as acceptance (CU-22 RN-04).
GO

/*---------------------------------------------------------------------
  4. Rooms and chat
  The matchmaking queue and the seats of each room live in the game
  server's memory: they die with the TCP connections (D-24).
---------------------------------------------------------------------*/

-- @entity Room | Private room with a code and the settings chosen by the host (CU-18).
-- @normalization Simple key. `Code` is only unique among rooms that are not closed, so it is not a candidate key.
CREATE TABLE dbo.Room (
    RoomId                  INT IDENTITY(1,1) NOT NULL,  -- Room identifier.
    Code                    CHAR(6)        NOT NULL,  -- Six characters without 0, O, 1 or I; unique among rooms that are not closed (CU-18 RN-01).
    HostId                  INT            NOT NULL,  -- Player who created the room.
    GameModeId              TINYINT        NOT NULL,  -- Mode, which sets the board and the seats.
    WallsPerPlayer          TINYINT        NOT NULL,  -- From 1 to 20, chosen by the host (CU-18 RN-04).
    ClockMinutes            TINYINT        NOT NULL,  -- 3, 5 or 10.
    Status                  VARCHAR(10)    NOT NULL DEFAULT ('OPEN'),  -- `OPEN`, `IN_MATCH` or `CLOSED`.
    MatchId                 INT            NULL,  -- Match started from the room.
    CreatedAt               DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Room creation.
    LastActivityAt          DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Used to close the room after thirty minutes without activity (CU-18 RN-08).
    CONSTRAINT PK_Room PRIMARY KEY (RoomId),
    CONSTRAINT FK_Room_Host FOREIGN KEY (HostId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account hosts many rooms over time.
    CONSTRAINT FK_Room_GameMode FOREIGN KEY (GameModeId) REFERENCES dbo.GameMode (GameModeId),  -- 1:N | A mode is played in many rooms.
    CONSTRAINT FK_Room_Match FOREIGN KEY (MatchId) REFERENCES dbo.Match (MatchId),  -- 1:0..1 | A room starts at most one match; a private match comes from a room.
    CONSTRAINT CK_Room_Code CHECK (Code LIKE '[A-HJ-NP-Z2-9][A-HJ-NP-Z2-9][A-HJ-NP-Z2-9][A-HJ-NP-Z2-9][A-HJ-NP-Z2-9][A-HJ-NP-Z2-9]' COLLATE Latin1_General_100_BIN2),
    CONSTRAINT CK_Room_Settings CHECK (WallsPerPlayer BETWEEN 1 AND 20 AND ClockMinutes IN (3, 5, 10)),
    CONSTRAINT CK_Room_Status CHECK (Status IN ('OPEN','IN_MATCH','CLOSED')),
    CONSTRAINT CK_Room_Match CHECK (Status <> 'IN_MATCH' OR MatchId IS NOT NULL)
);
GO
CREATE UNIQUE INDEX UX_Room_Code ON dbo.Room (Code) WHERE Status <> 'CLOSED';  -- The code is unique among rooms that are not closed and can be reused later (CU-18 RN-01).
CREATE UNIQUE INDEX UX_Room_Match ON dbo.Room (MatchId) WHERE MatchId IS NOT NULL;  -- A match comes from a single room.
GO

-- @entity Message | Chat message. Stored because a report attaches it as evidence (CU-28, CU-42).
-- @normalization Simple key; `Channel` determines whether match or room applies.
CREATE TABLE dbo.Message (
    MessageId            BIGINT IDENTITY(1,1) NOT NULL,  -- Message identifier.
    AuthorId             INT            NOT NULL,  -- Player who wrote it.
    Channel              VARCHAR(12)    NOT NULL,  -- `MATCH` or `ROOM` in the core. The CHECK already accepts `SPECTATORS`, `GLOBAL` and `FRIENDS`, from the later phase, because they need no new columns. Determines what the message belongs to.
    MatchId              INT            NULL,  -- Match; only in the `MATCH` and `SPECTATORS` channels.
    RoomId               INT            NULL,  -- Room; only in the `ROOM` channel.
    Text                 NVARCHAR(500)  NOT NULL,  -- Text as it was written, in Unicode and untranslated (CU-28 RN-12).
    SentAt               DATETIME2(3)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment it was sent.
    CONSTRAINT PK_Message PRIMARY KEY (MessageId),
    CONSTRAINT FK_Message_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account writes many messages.
    CONSTRAINT FK_Message_Match FOREIGN KEY (MatchId) REFERENCES dbo.Match (MatchId),  -- 1:N | A match has many messages from players and spectators.
    CONSTRAINT FK_Message_Room FOREIGN KEY (RoomId) REFERENCES dbo.Room (RoomId),  -- 1:N | A room has many waiting messages.
    CONSTRAINT CK_Message_Channel CHECK (
        (Channel IN ('MATCH','SPECTATORS') AND MatchId IS NOT NULL AND RoomId IS NULL)
        OR (Channel = 'ROOM' AND RoomId IS NOT NULL AND MatchId IS NULL)
        OR (Channel IN ('GLOBAL','FRIENDS') AND MatchId IS NULL AND RoomId IS NULL)),
    CONSTRAINT CK_Message_Text CHECK (LEN(Text) > 0)
);
GO
CREATE INDEX IX_Message_Channel ON dbo.Message (Channel, MatchId, RoomId, SentAt);
GO

/*---------------------------------------------------------------------
  5. Ranking
---------------------------------------------------------------------*/

-- @entity ModeStatistic | Statistics and elo of each account in each mode, from ranked matches only (CU-25 RN-14); it is the N:M relationship between Account and GameMode (the diagram's Has relationship, now per mode, D-04).
-- @normalization Composite account-mode key; every counter depends on both. Replaces the diagram's 1:1 Statistics entity, which would have forced the counters to be repeated for each mode. Without the derived win_percentage. The counters are controlled redundancy. In the core it stores only what matchmaking and moderation use; streaks, maximums and division arrive with the later phase (D-22).
CREATE TABLE dbo.ModeStatistic (
    AccountId            INT            NOT NULL,  -- Account.
    GameModeId           TINYINT        NOT NULL,  -- Mode.
    EloRating            SMALLINT       NOT NULL DEFAULT (1000),  -- Current elo in the mode; written only by CU-25.
    LastMatchAt          DATETIME2(0)   NULL,  -- End of the last ranked match in the mode: when the current elo was reached; breaks ties in the ranking (CU-35 RN-06).
    MatchesPlayed        INT            NOT NULL DEFAULT (0),  -- Ranked matches, including draws, which are neither a win nor a loss (CU-25 FA-05).
    MatchesWon           INT            NOT NULL DEFAULT (0),  -- Wins. The percentage is calculated, not stored (CU-15 RN-01).
    MatchesLost          INT            NOT NULL DEFAULT (0),  -- Losses, including resignations and abandonments.
    Abandonments         INT            NOT NULL DEFAULT (0),  -- Abandonments in online matches, for moderation (CU-24 RN-05).
    CONSTRAINT PK_ModeStatistic PRIMARY KEY (AccountId, GameModeId),
    CONSTRAINT FK_ModeStatistic_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account has one statistics row per mode.
    CONSTRAINT FK_ModeStatistic_GameMode FOREIGN KEY (GameModeId) REFERENCES dbo.GameMode (GameModeId),  -- 1:N | A mode has statistics for many accounts.
    CONSTRAINT CK_ModeStatistic_Counters CHECK (MatchesWon + MatchesLost <= MatchesPlayed
        AND MatchesWon >= 0 AND MatchesLost >= 0 AND Abandonments >= 0)
);
GO
CREATE INDEX IX_ModeStatistic_Ranking ON dbo.ModeStatistic (GameModeId, EloRating DESC, LastMatchAt) INCLUDE (MatchesPlayed);
GO

/*---------------------------------------------------------------------
  6. Relationships between players
---------------------------------------------------------------------*/

-- @entity Mute | Player muted by another; only the player who muted queries it (CU-33).
-- @normalization Composite key; `MutedAt` depends on the whole pair.
CREATE TABLE dbo.Mute (
    AccountId            INT            NOT NULL,  -- Player who mutes.
    MutedId              INT            NOT NULL,  -- Muted player.
    MutedAt              DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of the mute.
    CONSTRAINT PK_Mute PRIMARY KEY (AccountId, MutedId),
    CONSTRAINT FK_Mute_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account mutes many others (muter role).
    CONSTRAINT FK_Mute_Muted FOREIGN KEY (MutedId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account is muted by many others (muted role).
    CONSTRAINT CK_Mute_Distinct CHECK (AccountId <> MutedId)
);
GO

-- @entity Block | Block between two players; it prevents matchmaking, inviting, searching, sending requests and sharing a room (CU-33 RN-05).
-- @normalization Composite key; `BlockedAt` depends on the whole pair.
CREATE TABLE dbo.Block (
    BlockerId            INT            NOT NULL,  -- Player who blocks.
    BlockedId            INT            NOT NULL,  -- Blocked player.
    BlockedAt            DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of the block.
    CONSTRAINT PK_Block PRIMARY KEY (BlockerId, BlockedId),
    CONSTRAINT FK_Block_Blocker FOREIGN KEY (BlockerId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account blocks many others (blocker role).
    CONSTRAINT FK_Block_Blocked FOREIGN KEY (BlockedId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account is blocked by many others (blocked role).
    CONSTRAINT CK_Block_Distinct CHECK (BlockerId <> BlockedId)
);
GO
CREATE INDEX IX_Block_Blocked ON dbo.Block (BlockedId);
GO

/*---------------------------------------------------------------------
  7. Moderation and logs
  Nothing produced by moderation is ever deleted.
---------------------------------------------------------------------*/

-- @entity Report | Complaint from one player against another. Evidence is attached by reference to the match, not by copy (CU-42 RN-01).
-- @normalization Simple key. The reason is a closed domain with a `CHECK`: a catalog of five codes with no other attributes added nothing (D-23). The attached match is validated with composite foreign keys to *Participation* (CU-42 RN-09).
CREATE TABLE dbo.Report (
    ReportId             INT IDENTITY(1,1) NOT NULL,  -- Report identifier.
    ReporterId           INT            NOT NULL,  -- Player who reports (the diagram's reporter role).
    ReportedId           INT            NOT NULL,  -- Reported player.
    Reason               VARCHAR(25)    NOT NULL,  -- `OFFENSIVE_LANGUAGE`, `HARASSMENT`, `INAPPROPRIATE_NAME`, `CHEATING` or `UNSPORTSMANLIKE_CONDUCT` (CU-42 RN-08); it is the key of its description in the resource dictionaries (D-21, D-23).
    Description          NVARCHAR(500)  NULL,  -- Optional text from the reporter.
    MatchId              INT            NULL,  -- Match the evidence comes from, if attached; both the reporter and the reported player played it (CU-42 RN-09).
    SubmittedAt          DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Submission.
    Status               VARCHAR(20)    NOT NULL DEFAULT ('PENDING'),  -- `PENDING`, `IN_REVIEW`, `RESOLVED_NO_SANCTION` or `RESOLVED_SANCTIONED`.
    ModeratorId          INT            NULL,  -- Moderator who is reviewing it or resolved it.
    AssignedAt           DATETIME2(0)   NULL,  -- Moment it was claimed; after thirty minutes it goes back to the queue (CU-43 RN-03).
    ResolutionNote       NVARCHAR(1000) NULL,  -- Mandatory note when resolving (CU-43 RN-07).
    ResolvedAt           DATETIME2(0)   NULL,  -- Moment of the resolution.
    CONSTRAINT PK_Report PRIMARY KEY (ReportId),
    CONSTRAINT FK_Report_Reporter FOREIGN KEY (ReporterId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account files many reports (the diagram's Files relationship).
    CONSTRAINT FK_Report_Reported FOREIGN KEY (ReportedId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account receives many reports (the diagram's Points relationship).
    CONSTRAINT FK_Report_ReporterParticipation FOREIGN KEY (MatchId, ReporterId) REFERENCES dbo.Participation (MatchId, AccountId),  -- 1:N | The attached match is one the reporter played (CU-42 RN-09; the diagram's About relationship).
    CONSTRAINT FK_Report_ReportedParticipation FOREIGN KEY (MatchId, ReportedId) REFERENCES dbo.Participation (MatchId, AccountId),  -- 1:N | The attached match is one the reported player played (CU-42 RN-09).
    CONSTRAINT FK_Report_Moderator FOREIGN KEY (ModeratorId) REFERENCES dbo.Account (AccountId),  -- 1:N | A moderator reviews many reports.
    CONSTRAINT CK_Report_Distinct CHECK (ReporterId <> ReportedId),
    CONSTRAINT CK_Report_Reason CHECK (Reason IN ('OFFENSIVE_LANGUAGE','HARASSMENT','INAPPROPRIATE_NAME','CHEATING','UNSPORTSMANLIKE_CONDUCT')),
    CONSTRAINT CK_Report_Status CHECK (
        (Status = 'PENDING' AND ModeratorId IS NULL AND AssignedAt IS NULL AND ResolvedAt IS NULL)
        OR (Status = 'IN_REVIEW' AND ModeratorId IS NOT NULL AND AssignedAt IS NOT NULL AND ResolvedAt IS NULL)
        OR (Status IN ('RESOLVED_NO_SANCTION','RESOLVED_SANCTIONED') AND ModeratorId IS NOT NULL
            AND ResolutionNote IS NOT NULL AND ResolvedAt IS NOT NULL))
);
GO
CREATE UNIQUE INDEX UX_Report_Match ON dbo.Report (ReporterId, ReportedId, MatchId) WHERE MatchId IS NOT NULL;  -- The same player is not reported twice for the same match (CU-42 RN-03).
CREATE INDEX IX_Report_Queue ON dbo.Report (Status, SubmittedAt);
GO

-- @entity Sanction | Sanction applied by a moderator, with account or chat scope (D-05). Replaces the diagram's Ban; when lifted it is deactivated, not deleted.
-- @normalization Simple key. `IsActive` became computed because it depended on `RevokedAt`. Without the diagram's report_count and active from Ban, which were derived. `SanctionType` is a discriminator.
CREATE TABLE dbo.Sanction (
    SanctionId             INT IDENTITY(1,1) NOT NULL,  -- Sanction identifier.
    AccountId              INT            NOT NULL,  -- Sanctioned player.
    ModeratorId            INT            NOT NULL,  -- Moderator who applied it; kept even if the moderator role is later revoked (CU-47 RN-03).
    ReportId               INT            NULL,  -- Report that originated it, if any.
    Scope                  VARCHAR(10)    NOT NULL,  -- `ACCOUNT` or `CHAT` (CU-44 RN-03).
    SanctionType           VARCHAR(10)    NOT NULL,  -- `TEMPORARY` or `PERMANENT`. Determines whether `EndedAt` applies.
    Reason                 NVARCHAR(500)  NOT NULL,  -- What the moderator verified (CU-44 RN-06).
    StartedAt              DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Start of the sanction.
    EndedAt                DATETIME2(0)   NULL,  -- End of a temporary sanction; null if it is permanent (CU-44 RN-04).
    RevokedAt              DATETIME2(0)   NULL,  -- Manual revocation after an appeal or a replacement (CU-43 RN-06).
    ReplacementSanctionId  INT            NULL,  -- Sanction with the same scope that replaced it (CU-44 RN-09).
    IsActive               AS (CASE WHEN RevokedAt IS NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END),  -- Whether it was not revoked. Computed: it is exactly `RevokedAt` being null. Whether it is in effect today also depends on `EndedAt`.
    CONSTRAINT PK_Sanction PRIMARY KEY (SanctionId),
    CONSTRAINT FK_Sanction_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account receives many sanctions (the diagram's Receives relationship).
    CONSTRAINT FK_Sanction_Moderator FOREIGN KEY (ModeratorId) REFERENCES dbo.Account (AccountId),  -- 1:N | A moderator applies many sanctions.
    CONSTRAINT FK_Sanction_Report FOREIGN KEY (ReportId) REFERENCES dbo.Report (ReportId),  -- 1:N | A report originates zero, one or several sanctions (the diagram's Originates relationship).
    CONSTRAINT FK_Sanction_Replacement FOREIGN KEY (ReplacementSanctionId) REFERENCES dbo.Sanction (SanctionId),  -- 1:0..1 | A sanction can be replaced by a later one.
    CONSTRAINT CK_Sanction_Scope CHECK (Scope IN ('ACCOUNT','CHAT')),
    CONSTRAINT CK_Sanction_SanctionType CHECK (
        (SanctionType = 'TEMPORARY' AND EndedAt IS NOT NULL AND EndedAt > StartedAt)
        OR (SanctionType = 'PERMANENT' AND EndedAt IS NULL)),
    CONSTRAINT CK_Sanction_Replacement CHECK (ReplacementSanctionId IS NULL OR RevokedAt IS NOT NULL),
    CONSTRAINT CK_Sanction_Distinct CHECK (AccountId <> ModeratorId)
);
GO
CREATE INDEX IX_Sanction_Account ON dbo.Sanction (AccountId, Scope) WHERE RevokedAt IS NULL;
GO

-- @entity Appeal | Appeal of a sanction by the sanctioned player; there is at most one per sanction (CU-45 RN-01).
-- @normalization Surrogate key; `SanctionId` is a candidate key.
CREATE TABLE dbo.Appeal (
    AppealId             INT IDENTITY(1,1) NOT NULL,  -- Appeal identifier.
    SanctionId           INT            NOT NULL,  -- Appealed sanction.
    Text                 NVARCHAR(1000) NOT NULL,  -- Text from the sanctioned player; not rejected for its language (CU-45 RN-04).
    HasFlaggedLanguage   BIT            NOT NULL DEFAULT (0),  -- Whether the word filter found a term, as a warning to the moderator.
    SubmittedAt          DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Submission.
    Status               VARCHAR(12)    NOT NULL DEFAULT ('PENDING'),  -- `PENDING`, `IN_REVIEW`, `ACCEPTED` or `REJECTED`.
    ModeratorId          INT            NULL,  -- Moderator who reviews it; different from the one who applied the sanction (CU-45 RN-06).
    AssignedAt           DATETIME2(0)   NULL,  -- Moment it was claimed; after thirty minutes it goes back to the queue (CU-43 RN-03).
    ResolutionNote       NVARCHAR(1000) NULL,  -- Mandatory note when resolving.
    ResolvedAt           DATETIME2(0)   NULL,  -- Moment of the resolution.
    CONSTRAINT PK_Appeal PRIMARY KEY (AppealId),
    CONSTRAINT UQ_Appeal_SanctionId UNIQUE (SanctionId),  -- A single appeal per sanction (CU-45 RN-01).
    CONSTRAINT FK_Appeal_Sanction FOREIGN KEY (SanctionId) REFERENCES dbo.Sanction (SanctionId),  -- 1:0..1 | A sanction has at most one appeal.
    CONSTRAINT FK_Appeal_Moderator FOREIGN KEY (ModeratorId) REFERENCES dbo.Account (AccountId),  -- 1:N | A moderator resolves many appeals.
    CONSTRAINT CK_Appeal_Status CHECK (
        (Status = 'PENDING' AND ModeratorId IS NULL AND AssignedAt IS NULL AND ResolvedAt IS NULL)
        OR (Status = 'IN_REVIEW' AND ModeratorId IS NOT NULL AND AssignedAt IS NOT NULL AND ResolvedAt IS NULL)
        OR (Status IN ('ACCEPTED','REJECTED') AND ModeratorId IS NOT NULL AND ResolutionNote IS NOT NULL AND ResolvedAt IS NOT NULL))
);
GO

-- @entity ModerationLog | Record of every moderation and administration action, including viewing the logs (CU-48 RN-04). Insert only.
-- @normalization Simple key; insert only.
CREATE TABLE dbo.ModerationLog (
    ModerationLogId      BIGINT IDENTITY(1,1) NOT NULL,  -- Record identifier.
    ModeratorId          INT            NOT NULL,  -- Account that acted: moderator, administrator or whoever tried to act without the role.
    Action               VARCHAR(30)    NOT NULL,  -- `REPORT_CLAIMED`, `REPORT_RELEASED`, `REPORT_RESOLVED`, `SANCTION_APPLIED`, `APPEAL_RESOLVED`, `ROLE_GRANTED`, `ROLE_REVOKED`, `LOG_VIEWED` or `UNAUTHORIZED_ATTEMPT`.
    AffectedAccountId    INT            NULL,  -- Account acted upon, if any.
    Detail               NVARCHAR(1000) NULL,  -- Reason, report, sanction or filters of the action.
    OccurredAt           DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of the action.
    CONSTRAINT PK_ModerationLog PRIMARY KEY (ModerationLogId),
    CONSTRAINT FK_ModerationLog_Moderator FOREIGN KEY (ModeratorId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account performs many logged actions (actor role).
    CONSTRAINT FK_ModerationLog_AffectedAccount FOREIGN KEY (AffectedAccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account is the target of many actions (affected role).
    CONSTRAINT CK_ModerationLog_Action CHECK (Action IN ('REPORT_CLAIMED','REPORT_RELEASED','REPORT_RESOLVED','SANCTION_APPLIED',
        'APPEAL_RESOLVED','ROLE_GRANTED','ROLE_REVOKED','LOG_VIEWED','UNAUTHORIZED_ATTEMPT'))
);
GO

-- @entity AccessLog | Record of every access attempt and every credentials change, successful or not. It never contains passwords or codes (CU-48 RN-05).
-- @normalization Simple key; insert only.
CREATE TABLE dbo.AccessLog (
    AccessLogId             BIGINT IDENTITY(1,1) NOT NULL,  -- Record identifier.
    AccountId               INT            NULL,  -- Account, if it was identified; null for attempts against accounts that do not exist.
    EnteredIdentifier       NVARCHAR(254)  NULL,  -- Nickname or email typed; the only trace of an attempt against an account that does not exist (CU-48 RN-06).
    Result                  VARCHAR(40)    NOT NULL,  -- One of the 26 results in the domain table of the CRUD analysis, for example `LOGIN_SUCCEEDED`, `WRONG_PASSWORD` or `ACCOUNT_DELETED`.
    IpAddress               VARCHAR(45)    NOT NULL,  -- Source network address.
    OccurredAt              DATETIME2(0)   NOT NULL DEFAULT (SYSUTCDATETIME()),  -- Moment of the attempt.
    CONSTRAINT PK_AccessLog PRIMARY KEY (AccessLogId),
    CONSTRAINT FK_AccessLog_Account FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId),  -- 1:N | An account accumulates many access records.
    CONSTRAINT CK_AccessLog_Result CHECK (Result IN ('LOGIN_SUCCEEDED','ACCOUNT_NOT_FOUND','ACCOUNT_LOCKED','WRONG_PASSWORD','ACCOUNT_PENDING','ACCOUNT_SANCTIONED',
        'TWO_FACTOR_FAILED','LOGIN_ABANDONED','SUSPENSION_EXPIRED','REGISTRATION_SUCCEEDED','REGISTRATION_REJECTED','REGISTRATION_MAIL_FAILED',
        'GUEST_CREATED','GUEST_LINKED','ACCOUNT_VERIFIED','PASSWORD_RESET_REQUESTED','PASSWORD_RESET_UNKNOWN_EMAIL',
        'PASSWORD_RESET_SUCCEEDED','VOLUNTARY_LOGOUT','REMOTE_LOGOUT','PASSWORD_CHANGED','EMAIL_CHANGE_REQUESTED',
        'EMAIL_CHANGE_CONFIRMED','TWO_FACTOR_ENABLED','TWO_FACTOR_DISABLED','ACCOUNT_DELETED'))
);
GO
CREATE INDEX IX_AccessLog_OccurredAt ON dbo.AccessLog (OccurredAt, IpAddress);
GO

/*---------------------------------------------------------------------
  8. Procedures for physical deletes
  The connection user has no DELETE permission. The only rows the
  application deletes in the core (mutes and blocks) are deleted with
  these procedures, which belong to dbo like the tables: thanks to
  ownership chaining, SQL Server does not check the caller's DELETE
  permission, only its EXECUTE permission.
---------------------------------------------------------------------*/

-- CU-18 FA-09, CU-25 FA-08: the room closes after inactivity, when the host leaves or when its match ends.
-- Who holds each seat is tracked by the game server in memory (D-24).
CREATE PROCEDURE dbo.usp_Room_Close
    @RoomId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Room SET Status = 'CLOSED' WHERE RoomId = @RoomId AND Status <> 'CLOSED';
    SELECT @@ROWCOUNT AS UpdatedRows;
END;
GO

-- CU-33 FA-07: unmute.
CREATE PROCEDURE dbo.usp_Mute_Delete
    @AccountId INT,
    @MutedId   INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Mute WHERE AccountId = @AccountId AND MutedId = @MutedId;
    SELECT @@ROWCOUNT AS DeletedRows;
END;
GO

-- CU-33 FA-07: unblock. It does not restore the deleted friendship (CU-33 RN-06).
CREATE PROCEDURE dbo.usp_Block_Delete
    @BlockerId INT,
    @BlockedId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Block WHERE BlockerId = @BlockerId AND BlockedId = @BlockedId;
    SELECT @@ROWCOUNT AS DeletedRows;
END;
GO

-- CU-33 steps 7 to 9: block. In the core it only records the block; the later phase
-- redefines the procedure to also delete the pair's friendship, requests and invitations.
-- If they share a room, the game server removes one of them in memory (D-24).
CREATE PROCEDURE dbo.usp_Block_Apply
    @BlockerId INT,
    @BlockedId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Block WHERE BlockerId = @BlockerId AND BlockedId = @BlockedId)
        INSERT INTO dbo.Block (BlockerId, BlockedId) VALUES (@BlockerId, @BlockedId);
END;
GO
