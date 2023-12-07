
START TRANSACTION;

-- YOU MUST SET PROPER VALUES HERE BEFORE RUNNING THIS SCRIPT
SET @adminUserId = 'ddb8b243-f1f1-4cf6-b8c3-633060626648'; 

SET @personTechnicalId = UUID();

INSERT INTO PersonTechnicals (Id, UserId, Created, Modified, CreatedById, ModifiedById)
VALUES (@personTechnicalId, @adminUserId, NOW(), NOW(), '00000000-1111-0000-0000-000000000000', '00000000-1111-0000-0000-000000000000');

INSERT INTO rumis.Persons
(Id, FirstName, LastName, PrivatePersonalIdentifier, ActiveFrom, PersonTechnicalId, Created, Modified, CreatedById, ModifiedById, BirthDate)
VALUES(UUID(), 'Admin', 'Admin', '00000000000', NOW(), @personTechnicalId, NOW(), NOW(), '00000000-1111-0000-0000-000000000000', '00000000-1111-0000-0000-000000000000', NOW());

COMMIT;
