CREATE TABLE Title (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    external_id NVARCHAR(255),
    title_name NVARCHAR(255) NOT NULL,
    year INT NOT NULL,
    type NVARCHAR(50) NOT NULL CHECK (type IN ('movie', 'tv')),
    last_updated DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE StreamingService (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    region NVARCHAR(50) NULL
);

CREATE TABLE [User] (
    ASPNetIdentity_id UNIQUEIDENTIFIER PRIMARY KEY,
    username NVARCHAR(255) NOT NULL UNIQUE,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(512) NOT NULL,
    first_name NVARCHAR(255) NOT NULL,
    last_name NVARCHAR(255) NOT NULL,
    streaming_services_id UNIQUEIDENTIFIER NULL,
    recently_viewed_show_id UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_User_StreamingService FOREIGN KEY (streaming_services_id) 
        REFERENCES StreamingService(id) ON DELETE SET NULL,
    CONSTRAINT FK_User_Title FOREIGN KEY (recently_viewed_show_id) 
        REFERENCES Title(id) ON DELETE SET NULL
);
