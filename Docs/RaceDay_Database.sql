--RaceDay Database Script
-- PROG6212 POE
--Koketso Modiselle 

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO



-- USER TABLE
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'Participant'
);
