USE rumis;

-- Mainîgo deklarâcija un inicializâcija
--
-- Pirms tiek laists skripts, nepiecieðams aizvietot sekojoðâs vçrtîbas
-- ar sistçmas videi atbilstoðâm vçrtîbâm.
SET @username = 'OpenDataReader';
SET @password = '';

-- Izpildâmâs darbîbas
START TRANSACTION;
	-- Lietotâja veidoðana
	SET @createUserSql = CONCAT('CREATE USER ', @username, '@\'%\' IDENTIFIED BY \'', @password, '\';');

	PREPARE createUserStatement FROM @createUserSql;

	EXECUTE createUserStatement;

	DEALLOCATE PREPARE createUserStatement;

    -- Tiesîbu pieðíirðana
	SET @grantPermissionsSql = CONCAT('GRANT SELECT ON rumis.open_data TO ', @username, '@\'%\';');

	PREPARE grantPermissionsStatement FROM @grantPermissionsSql;

	EXECUTE grantPermissionsStatement;

	DEALLOCATE PREPARE grantPermissionsStatement;

	FLUSH PRIVILEGES;

COMMIT;