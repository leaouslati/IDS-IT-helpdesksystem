-- ============================================================
-- IT Help Desk & Ticketing Management System
-- Database Schema - SQL Server
-- ============================================================

-- ============================================================
-- 1. Role
-- ============================================================
CREATE TABLE Role (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(255) NULL
);

-- ============================================================
-- 2. User
-- ============================================================
CREATE TABLE [User] (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    RoleId         INT           NOT NULL,
    FirstName      NVARCHAR(100) NOT NULL,
    LastName       NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash   NVARCHAR(255) NOT NULL,
    IsActive       BIT           NOT NULL DEFAULT 1,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE(),
    LastLoginAt    DATETIME      NULL,
    ProfilePicture NVARCHAR(255) NULL,

    CONSTRAINT FkUser_RoleId FOREIGN KEY (RoleId) REFERENCES Role(Id)
);

-- ============================================================
-- 3. Category
-- ============================================================
CREATE TABLE Category (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(255) NULL
);

-- ============================================================
-- 4. Priority
-- ============================================================
CREATE TABLE Priority (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    PriorityName  NVARCHAR(50) NOT NULL,
    PriorityLevel INT          NOT NULL
);

-- ============================================================
-- 5. TicketStatus
-- ============================================================
CREATE TABLE TicketStatus (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    StatusName  NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(255) NULL
);

-- ============================================================
-- 6. Ticket
-- ============================================================
CREATE TABLE Ticket (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    TicketNumber     NVARCHAR(20)   NOT NULL UNIQUE,
    Title            NVARCHAR(255)  NOT NULL,
    Description      NVARCHAR(MAX)  NOT NULL,
    CategoryId       INT            NOT NULL,
    PriorityId       INT            NOT NULL,
    StatusId         INT            NOT NULL,
    CreatedByUserId  INT            NOT NULL,
    AssignedToUserId INT            NULL,
    AssignedByUserId INT            NULL,
    AssignedAt       DATETIME       NULL,
    IsEscalated      BIT            NOT NULL DEFAULT 0,
    CreatedAt        DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt        DATETIME       NULL,
    ResolvedAt       DATETIME       NULL,

    CONSTRAINT FkTicket_CategoryId       FOREIGN KEY (CategoryId)       REFERENCES Category(Id),
    CONSTRAINT FkTicket_PriorityId       FOREIGN KEY (PriorityId)       REFERENCES Priority(Id),
    CONSTRAINT FkTicket_StatusId         FOREIGN KEY (StatusId)         REFERENCES TicketStatus(Id),
    CONSTRAINT FkTicket_CreatedByUserId  FOREIGN KEY (CreatedByUserId)  REFERENCES [User](Id),
    CONSTRAINT FkTicket_AssignedToUserId FOREIGN KEY (AssignedToUserId) REFERENCES [User](Id),
    CONSTRAINT FkTicket_AssignedByUserId FOREIGN KEY (AssignedByUserId) REFERENCES [User](Id)
);

-- ============================================================
-- 7. TicketComment
-- ============================================================
CREATE TABLE TicketComment (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TicketId        INT            NOT NULL,
    CreatedByUserId INT            NOT NULL,
    Content         NVARCHAR(MAX)  NOT NULL,
    IsInternal      BIT            NOT NULL DEFAULT 0,
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FkTicketComment_TicketId        FOREIGN KEY (TicketId)        REFERENCES Ticket(Id),
    CONSTRAINT FkTicketComment_CreatedByUserId FOREIGN KEY (CreatedByUserId) REFERENCES [User](Id)
);

-- ============================================================
-- 8. TicketAttachment
-- ============================================================
CREATE TABLE TicketAttachment (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    TicketId   INT            NOT NULL,
    UploadedBy INT            NOT NULL,
    FileName   NVARCHAR(255)  NOT NULL,
    FilePath   NVARCHAR(500)  NOT NULL,
    FileSize   INT            NOT NULL,
    FileType   NVARCHAR(50)   NOT NULL,
    UploadedAt DATETIME       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FkTicketAttachment_TicketId   FOREIGN KEY (TicketId)   REFERENCES Ticket(Id),
    CONSTRAINT FkTicketAttachment_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES [User](Id)
);

-- ============================================================
-- 9. Notification
-- ============================================================
CREATE TABLE Notification (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT            NOT NULL,
    TicketId  INT            NOT NULL,
    Message   NVARCHAR(500)  NOT NULL,
    IsRead    BIT            NOT NULL DEFAULT 0,
    CreatedAt DATETIME       NOT NULL DEFAULT GETDATE(),
    ReadAt    DATETIME       NULL,

    CONSTRAINT FkNotification_UserId   FOREIGN KEY (UserId)   REFERENCES [User](Id),
    CONSTRAINT FkNotification_TicketId FOREIGN KEY (TicketId) REFERENCES Ticket(Id)
);

-- ============================================================
-- 10. ActivityLog
-- ============================================================
CREATE TABLE ActivityLog (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    PerformedByUserId INT            NOT NULL,
    TicketId          INT            NULL,
    Action            NVARCHAR(100)  NOT NULL,
    OldValue          NVARCHAR(255)  NULL,
    NewValue          NVARCHAR(255)  NULL,
    LoggedAt          DATETIME       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FkActivityLog_PerformedByUserId FOREIGN KEY (PerformedByUserId) REFERENCES [User](Id),
    CONSTRAINT FkActivityLog_TicketId          FOREIGN KEY (TicketId)          REFERENCES Ticket(Id)
);

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles
INSERT INTO Role (RoleName, Description) VALUES
('Admin',            'Full system access'),
('IT Support Agent', 'Manage and resolve tickets'),
('Employee',         'Create and track tickets'),
('Manager',          'Monitor team tickets and reports');

-- Categories
INSERT INTO Category (CategoryName, Description) VALUES
('Hardware',       'Hardware related issues'),
('Software',       'Software related issues'),
('Network',        'Network related issues'),
('Email',          'Email related issues'),
('Access Request', 'Access and permission requests'),
('Other',          'Other issues');

-- Priorities
INSERT INTO Priority (PriorityName, PriorityLevel) VALUES
('Low',      1),
('Medium',   2),
('High',     3),
('Critical', 4);

-- Statuses
INSERT INTO TicketStatus (StatusName, Description) VALUES
('Open',        'Ticket has been submitted'),
('In Progress', 'Ticket is being worked on'),
('Pending',     'Waiting for more information'),
('Resolved',    'Issue has been resolved');


-- ============================================================
-- INITIAL ADMIN USER
-- ============================================================
-- Password is 'Admin@123' hashed with bcrypt
-- Change this password immediately after first login

INSERT INTO [User] (RoleId, FirstName, LastName, Email, PasswordHash, IsActive, CreatedAt)
VALUES (
    1,
    'System',
    'Admin',
    'admin@helpdesk.com',
    '$2a$12$eImiTXuWVxfM37uY4JANjQ==.hashed.placeholder.change.this',
    1,
    GETDATE()
);
