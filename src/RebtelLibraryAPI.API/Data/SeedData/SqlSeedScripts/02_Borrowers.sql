-- 02_Borrowers.sql - SQL Seed Script for Borrowers Table
-- Inserts 20+ diverse borrowers with varied registration dates and borrowing patterns
-- Data designed to showcase analytics capabilities with power users and casual borrowers

-- Power Users (High borrowing frequency - registered early, active for analytics)
INSERT INTO Borrowers (Id, FirstName, LastName, Email, Phone, RegistrationDate, MemberStatus, CreatedAt, UpdatedAt) VALUES
('22222222-2222-2222-2222-222222222201', 'Sarah', 'Johnson', 'sarah.johnson@email.com', '5551234567', DATEADD(day, -365, GETDATE()), 0, DATEADD(day, -365, GETDATE()), DATEADD(day, -365, GETDATE())),
('22222222-2222-2222-2222-222222222202', 'Michael', 'Chen', 'michael.chen@email.com', '5552345678', DATEADD(day, -340, GETDATE()), 0, DATEADD(day, -340, GETDATE()), DATEADD(day, -340, GETDATE())),
('22222222-2222-2222-2222-222222222203', 'Emma', 'Williams', 'emma.williams@email.com', '5553456789', DATEADD(day, -320, GETDATE()), 0, DATEADD(day, -320, GETDATE()), DATEADD(day, -320, GETDATE())),
('22222222-2222-2222-2222-222222222204', 'James', 'Martinez', 'james.martinez@email.com', '5554567890', DATEADD(day, -300, GETDATE()), 0, DATEADD(day, -300, GETDATE()), DATEADD(day, -300, GETDATE())),
('22222222-2222-2222-2222-222222222205', 'Olivia', 'Brown', 'olivia.brown@email.com', '5555678901', DATEADD(day, -280, GETDATE()), 0, DATEADD(day, -280, GETDATE()), DATEADD(day, -280, GETDATE())),

-- Regular Active Members (Medium borrowing frequency)
('22222222-2222-2222-2222-222222222206', 'David', 'Jones', 'david.jones@email.com', '5556789012', DATEADD(day, -250, GETDATE()), 0, DATEADD(day, -250, GETDATE()), DATEADD(day, -250, GETDATE())),
('22222222-2222-2222-2222-222222222207', 'Sophia', 'Garcia', 'sophia.garcia@email.com', '5557890123', DATEADD(day, -230, GETDATE()), 0, DATEADD(day, -230, GETDATE()), DATEADD(day, -230, GETDATE())),
('22222222-2222-2222-2222-222222222208', 'Robert', 'Miller', 'robert.miller@email.com', '5558901234', DATEADD(day, -210, GETDATE()), 0, DATEADD(day, -210, GETDATE()), DATEADD(day, -210, GETDATE())),
('22222222-2222-2222-2222-222222222209', 'Isabella', 'Davis', 'isabella.davis@email.com', '5559012345', DATEADD(day, -190, GETDATE()), 0, DATEADD(day, -190, GETDATE()), DATEADD(day, -190, GETDATE())),
('22222222-2222-2222-2222-222222222210', 'William', 'Rodriguez', 'william.rodriguez@email.com', '5550123456', DATEADD(day, -170, GETDATE()), 0, DATEADD(day, -170, GETDATE()), DATEADD(day, -170, GETDATE())),

-- Casual Borrowers (Low borrowing frequency - recent registrations)
('22222222-2222-2222-2222-222222222211', 'Ava', 'Wilson', 'ava.wilson@email.com', NULL, DATEADD(day, -140, GETDATE()), 0, DATEADD(day, -140, GETDATE()), DATEADD(day, -140, GETDATE())),
('22222222-2222-2222-2222-222222222212', 'Joseph', 'Anderson', 'joseph.anderson@email.com', '5551234568', DATEADD(day, -120, GETDATE()), 0, DATEADD(day, -120, GETDATE()), DATEADD(day, -120, GETDATE())),
('22222222-2222-2222-2222-222222222213', 'Mia', 'Taylor', 'mia.taylor@email.com', NULL, DATEADD(day, -100, GETDATE()), 0, DATEADD(day, -100, GETDATE()), DATEADD(day, -100, GETDATE())),
('22222222-2222-2222-2222-222222222214', 'Thomas', 'Thomas', 'thomas.thomas@email.com', '5552345679', DATEADD(day, -80, GETDATE()), 0, DATEADD(day, -80, GETDATE()), DATEADD(day, -80, GETDATE())),
('22222222-2222-2222-2222-222222222215', 'Charlotte', 'Moore', 'charlotte.moore@email.com', NULL, DATEADD(day, -60, GETDATE()), 0, DATEADD(day, -60, GETDATE()), DATEADD(day, -60, GETDATE())),

-- Recently Registered (New members for edge case testing)
('22222222-2222-2222-2222-222222222216', 'Christopher', 'Jackson', 'christopher.jackson@email.com', '5553456780', DATEADD(day, -45, GETDATE()), 0, DATEADD(day, -45, GETDATE()), DATEADD(day, -45, GETDATE())),
('22222222-2222-2222-2222-222222222217', 'Amelia', 'Martin', 'amelia.martin@email.com', NULL, DATEADD(day, -30, GETDATE()), 0, DATEADD(day, -30, GETDATE()), DATEADD(day, -30, GETDATE())),
('22222222-2222-2222-2222-222222222218', 'Daniel', 'Lee', 'daniel.lee@email.com', '5554567891', DATEADD(day, -20, GETDATE()), 0, DATEADD(day, -20, GETDATE()), DATEADD(day, -20, GETDATE())),
('22222222-2222-2222-2222-222222222219', 'Harper', 'Perez', 'harper.perez@email.com', NULL, DATEADD(day, -15, GETDATE()), 0, DATEADD(day, -15, GETDATE()), DATEADD(day, -15, GETDATE())),

-- Edge Case Borrowers (For testing various scenarios)
('22222222-2222-2222-2222-222222222220', 'Evelyn', 'Thompson', 'evelyn.thompson@email.com', '5555678902', DATEADD(day, -10, GETDATE()), 0, DATEADD(day, -10, GETDATE()), DATEADD(day, -10, GETDATE())),
('22222222-2222-2222-2222-222222222221', 'Andrew', 'White', 'andrew.white@email.com', NULL, DATEADD(day, -7, GETDATE()), 0, DATEADD(day, -7, GETDATE()), DATEADD(day, -7, GETDATE())),
('22222222-2222-2222-2222-222222222222', 'Abigail', 'Harris', 'abigail.harris@email.com', '5556789013', DATEADD(day, -5, GETDATE()), 0, DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE())),

-- Additional diverse borrowers for analytics testing
('22222222-2222-2222-2222-222222222223', 'Joshua', 'Clark', 'joshua.clark@email.com', NULL, DATEADD(day, -90, GETDATE()), 0, DATEADD(day, -90, GETDATE()), DATEADD(day, -90, GETDATE())),
('22222222-2222-2222-2222-222222222224', 'Emily', 'Lewis', 'emily.lewis@email.com', '5557890124', DATEADD(day, -70, GETDATE()), 0, DATEADD(day, -70, GETDATE()), DATEADD(day, -70, GETDATE())),
('22222222-2222-2222-2222-222222222225', 'Ryan', 'Robinson', 'ryan.robinson@email.com', NULL, DATEADD(day, -50, GETDATE()), 0, DATEADD(day, -50, GETDATE()), DATEADD(day, -50, GETDATE()));

-- Note on MemberStatus values:
-- 0 = Active
-- 1 = Inactive
-- 2 = Suspended
-- All borrowers start as Active (0) for demonstration purposes

-- Registration Date Pattern:
-- Power Users: Registered 9-12 months ago (high engagement potential)
-- Regular Members: Registered 5-8 months ago (moderate engagement)
-- Casual Users: Registered 1-4 months ago (newer, testing engagement)
-- Recently Registered: Registered within the last 2 months (new members)

-- Phone Number Strategy:
-- Some borrowers have phone numbers, others don't (NULL) to test edge cases
-- All phone numbers follow US format without special characters (normalized by domain logic)