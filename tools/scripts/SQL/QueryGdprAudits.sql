SET @created = DATE_ADD(CURRENT_TIMESTAMP(), INTERVAL CAST(-1.0 AS signed) day);

SELECT `g`.`Id`, `g`.`UnitOfWorkId`, `g`.`Created`, `g`.`Action`, `g`.`ActionData`, `g`.`UserId`, `u`.`Name` AS `UserName`, `g`.`UserProfileId`, `u0`.`Job` AS `UserProfileJob`, `p`.`Id` AS `DataHandlerId`, `p1`.`FirstName` AS `DataHandlerFirstName`, `p1`.`LastName` AS `DataHandlerLastName`, `g`.`DataHandlerPrivatePersonalIdentifier`, `e`.`Id` AS `UserProfileEducationalInstitutionId`, `e`.`Code` AS `UserProfileEducationalInstitutionCode`, `e`.`Name` AS `UserProfileEducationalInstitutionName`, `s`.`Id` AS `UserProfileSupervisorId`, `s`.`Code` AS `UserProfileSupervisorCode`, `s`.`Name` AS `UserProfileSupervisorName`, `t`.`Name` AS `RoleName`, `t`.`Id` AS `RoleId`, `g`.`DataOwnerPrivatePersonalIdentifier`, `g`.`DataOwnerId`, `p2`.`FirstName` AS `DataOwnerFirstName`, `p2`.`LastName` AS `DataOwnerLastName`, `p2`.`PrivatePersonalIdentifier` AS `DataOwnerPrivatePersonalIdentifier`, `g0`.`Type`, `g0`.`Value`, `g`.`EducationalInstitutionId`, `e0`.`Code` AS `EducationalInstitutionCode`, `e0`.`Name` AS `EducationalInstitutionName`, `g`.`SupervisorId`, `s0`.`Code` AS `SupervisorCode`, `s0`.`Name` AS `SupervisorName`
FROM `GdprAudits` AS `g`
LEFT JOIN `Users` AS `u` ON `g`.`UserId` = `u`.`Id`
LEFT JOIN `UserProfiles` AS `u0` ON `g`.`UserProfileId` = `u0`.`Id`
LEFT JOIN `EducationalInstitutions` AS `e` ON `u0`.`EducationalInstitutionId` = `e`.`Id`
LEFT JOIN `Supervisors` AS `s` ON `u0`.`SupervisorId` = `s`.`Id`
LEFT JOIN `PersonTechnicals` AS `p` ON `g`.`DataHandlerId` = `p`.`Id`
LEFT JOIN `PersonTechnicals` AS `p0` ON `g`.`DataOwnerId` = `p0`.`Id`
LEFT JOIN `EducationalInstitutions` AS `e0` ON `g`.`EducationalInstitutionId` = `e0`.`Id`
LEFT JOIN `Supervisors` AS `s0` ON `g`.`SupervisorId` = `s0`.`Id`
LEFT JOIN (
	SELECT `r0`.`Name`, `r`.`RolesId`, `r`.`UserProfilesId`, `r0`.`Id`
    FROM `RoleUserProfile` AS `r`
    INNER JOIN `Roles` AS `r0` ON `r`.`RolesId` = `r0`.`Id`
) AS `t` ON `u0`.`Id` = `t`.`UserProfilesId`
LEFT JOIN `Persons` AS `p1` ON `p`.`Id` = `p1`.`PersonTechnicalId`
LEFT JOIN `Persons` AS `p2` ON `p0`.`Id` = `p2`.`PersonTechnicalId`
LEFT JOIN `GdprAuditData` AS `g0` ON `g`.`Id` = `g0`.`GdprAuditId`
WHERE `g`.`Created` >= @created
ORDER BY `g`.`Id`, `u`.`Id`, `u0`.`Id`, `e`.`Id`, `s`.`Id`, `p`.`Id`, `p0`.`Id`, `e0`.`Id`, `s0`.`Id`, `t`.`RolesId`, `t`.`UserProfilesId`, `t`.`Id`, `p1`.`Id`, `p2`.`Id`