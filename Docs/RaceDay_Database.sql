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

 ORGANISER TABLE
CREATE TABLE Organiser (
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    OrganisationName VARCHAR(100) NOT NULL,
    ContactNumber VARCHAR(20) NOT NULL UNIQUE,

    CONSTRAINT FK_Organiser_User
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);


-- PARTICIPANT TABLE
CREATE TABLE Participant (
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    DateOfBirth DATE NOT NULL,

    CONSTRAINT FK_Participant_User
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

- CATEGORY TABLE
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(50) NOT NULL UNIQUE,
    DistanceKM DECIMAL(5,2) NOT NULL
);

- EVENT TABLE
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    CategoryID INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(100) NOT NULL,
    RouteInformation VARCHAR(255),
    WeatherInformation VARCHAR(255),

    CONSTRAINT FK_Event_Organiser
    FOREIGN KEY (OrganiserID) REFERENCES Organiser(OrganiserID),

    CONSTRAINT FK_Event_Category
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);


-- ENROLLMENT TABLE
CREATE TABLE Enrollment (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    RegistrationDate DATE NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(30) NOT NULL DEFAULT 'Registered',

    CONSTRAINT FK_Enrollment_Participant
    FOREIGN KEY (ParticipantID) REFERENCES Participant(ParticipantID),

    CONSTRAINT FK_Enrollment_Event
    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);


