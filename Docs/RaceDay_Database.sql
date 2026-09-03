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

 RESULT TABLE
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,

    CONSTRAINT FK_Result_Enrollment
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);

-- SAMPLE DATA INSERTS
-- USERS
INSERT INTO Users 
(FirstName, LastName, Email, PasswordHash, Role)
VALUES
('Thabo','Mokoena','thabo@raceday.co.za','hash123','Organiser'),
('Lerato','Nkosi','lerato@raceday.co.za','hash456','Organiser'),
('Kabelo','Dube','kabelo@email.com','hash789','Participant'),
('Amahle','Mthembu','amahle@email.com','hash321','Participant');


-- ORGANISERS
INSERT INTO Organiser
(UserID, OrganisationName, ContactNumber)
VALUES
(1,'Pretoria Marathon Club','0123456789'),
(2,'Run Gauteng Events','0119876543');


-- PARTICIPANTS
INSERT INTO Participant
(UserID, PhoneNumber, DateOfBirth)
VALUES
(3,'0721112222','2002-05-15'),
(4,'0733334444','2001-09-20');


-- CATEGORIES
INSERT INTO Category
(CategoryName, DistanceKM)
VALUES
('5KM Fun Run',5),
('10KM Race',10),
('Half Marathon',21.1);


-- EVENTS
INSERT INTO Event
(OrganiserID, CategoryID, EventName, EventDate, Location, RouteInformation, WeatherInformation)
VALUES
(1,1,'Pretoria Spring Run','2026-09-05',
'Pretoria West','City route','Sunny'),

(1,2,'Gauteng 10K Challenge','2026-10-10',
'Centurion','Park route','Cloudy'),

(2,3,'Capital Half Marathon','2026-11-15',
'Pretoria CBD','Highway route','Warm');


