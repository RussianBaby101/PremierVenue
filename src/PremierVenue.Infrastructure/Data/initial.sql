BEGIN TRANSACTION;
GO

CREATE TABLE [Amenities] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Icon] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Amenities] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [UserName] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Role] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiry] datetime2 NULL,
    [InvitationToken] nvarchar(max) NULL,
    [InvitationSentAt] datetime2 NULL,
    [InvitationExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Venues] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Province] nvarchar(100) NOT NULL,
    [PostalCode] nvarchar(20) NOT NULL,
    [Latitude] decimal(18,2) NOT NULL,
    [Longitude] decimal(18,2) NOT NULL,
    [Capacity] int NOT NULL,
    [BasePricePerDay] decimal(18,2) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [ThumbnailUrl] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsFeatured] bit NOT NULL CONSTRAINT [DF_Venues_IsFeatured] DEFAULT 0,
    [CustomAmenities] nvarchar(max) NOT NULL,
    [SupportedServices] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Venues] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] int NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Type] int NOT NULL,
    [IsRead] bit NOT NULL,
    [ActionUrl] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ReadAt] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Availabilities] (
    [Id] int NOT NULL IDENTITY,
    [VenueId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [IsAvailable] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Availabilities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Availabilities_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Bookings] (
    [Id] int NOT NULL IDENTITY,
    [ReferenceNumber] nvarchar(50) NOT NULL,
    [ClientId] int NOT NULL,
    [VenueId] int NOT NULL,
    [EventType] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [ExpectedGuests] int NOT NULL,
    [SpecialRequirements] nvarchar(2000) NOT NULL,
    [CateringRequested] bit NOT NULL,
    [StaffingSecurityRequested] bit NOT NULL,
    [SetupCleanupRequested] bit NOT NULL,
    [AdditionalServices] nvarchar(2000) NOT NULL,
    [EstimatedBudget] decimal(18,2) NOT NULL,
    [FinalQuote] decimal(18,2) NOT NULL,
    [DepositAmount] decimal(18,2) NOT NULL,
    [QuoteExpiresAt] datetime2 NULL,
    [CancellationPolicy] nvarchar(4000) NOT NULL,
    [CancellationPolicyCode] nvarchar(50) NOT NULL,
    [BalanceAmount] decimal(18,2) NOT NULL,
    [RefundAmount] decimal(18,2) NOT NULL,
    [RefundStatus] nvarchar(50) NOT NULL,
    [CancellationFeeAmount] decimal(18,2) NOT NULL,
    [CancellationFeeStatus] nvarchar(50) NOT NULL,
    [CancellationFeeDueAt] datetime2 NULL,
    [CancelledAt] datetime2 NULL,
    [Status] int NOT NULL,
    [InternalNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [ConfirmedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bookings_Users_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookings_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SavedVenues] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [VenueId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SavedVenues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SavedVenues_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SavedVenues_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VenueAmenities] (
    [Id] int NOT NULL IDENTITY,
    [VenueId] int NOT NULL,
    [AmenityId] int NOT NULL,
    [IsIncluded] bit NOT NULL,
    [AdditionalCost] decimal(18,2) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_VenueAmenities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VenueAmenities_Amenities_AmenityId] FOREIGN KEY ([AmenityId]) REFERENCES [Amenities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VenueAmenities_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VenueEventTypes] (
    [VenueId] int NOT NULL,
    [EventType] int NOT NULL,
    CONSTRAINT [PK_VenueEventTypes] PRIMARY KEY ([VenueId], [EventType]),
    CONSTRAINT [FK_VenueEventTypes_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VenuePhotos] (
    [Id] int NOT NULL IDENTITY,
    [VenueId] int NOT NULL,
    [Url] nvarchar(1000) NOT NULL,
    [Caption] nvarchar(500) NULL,
    [FileName] nvarchar(255) NULL,
    [ContentType] nvarchar(100) NULL,
    [Content] varbinary(max) NULL,
    [DisplayOrder] int NOT NULL,
    [IsPrimary] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_VenuePhotos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VenuePhotos_Venues_VenueId] FOREIGN KEY ([VenueId]) REFERENCES [Venues] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BookingDocuments] (
    [Id] int NOT NULL IDENTITY,
    [BookingId] int NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [Url] nvarchar(1000) NOT NULL,
    [DocumentType] int NOT NULL,
    [FileSize] bigint NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_BookingDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookingDocuments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Messages] (
    [Id] int NOT NULL IDENTITY,
    [BookingId] int NOT NULL,
    [SenderId] int NOT NULL,
    [ReceiverId] int NULL,
    [Content] nvarchar(2000) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ReadAt] datetime2 NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Messages_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Messages_Users_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Messages_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [BookingId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentType] int NOT NULL,
    [Status] int NOT NULL,
    [TransactionReference] nvarchar(200) NULL,
    [PaymentGatewayResponse] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ProcessedAt] datetime2 NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Tasks] (
    [Id] int NOT NULL IDENTITY,
    [BookingId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [Status] int NOT NULL,
    [Priority] int NOT NULL,
    [DueDate] datetime2 NULL,
    [AssignedToId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tasks_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tasks_Users_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Availabilities_VenueId_Date] ON [Availabilities] ([VenueId], [Date]);
GO

CREATE INDEX [IX_BookingDocuments_BookingId] ON [BookingDocuments] ([BookingId]);
GO

CREATE INDEX [IX_Bookings_ClientId] ON [Bookings] ([ClientId]);
GO

CREATE UNIQUE INDEX [IX_Bookings_ReferenceNumber] ON [Bookings] ([ReferenceNumber]);
GO

CREATE INDEX [IX_Bookings_VenueId] ON [Bookings] ([VenueId]);
GO

CREATE INDEX [IX_Messages_BookingId] ON [Messages] ([BookingId]);
GO

CREATE INDEX [IX_Messages_ReceiverId] ON [Messages] ([ReceiverId]);
GO

CREATE INDEX [IX_Messages_SenderId] ON [Messages] ([SenderId]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
GO

CREATE UNIQUE INDEX [IX_SavedVenues_UserId_VenueId] ON [SavedVenues] ([UserId], [VenueId]);
GO

CREATE INDEX [IX_SavedVenues_VenueId] ON [SavedVenues] ([VenueId]);
GO

CREATE INDEX [IX_Tasks_AssignedToId] ON [Tasks] ([AssignedToId]);
GO

CREATE INDEX [IX_Tasks_BookingId] ON [Tasks] ([BookingId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

CREATE INDEX [IX_VenueAmenities_AmenityId] ON [VenueAmenities] ([AmenityId]);
GO

CREATE INDEX [IX_VenueAmenities_VenueId] ON [VenueAmenities] ([VenueId]);
GO

CREATE INDEX [IX_VenuePhotos_VenueId] ON [VenuePhotos] ([VenueId]);
GO

COMMIT;
GO

