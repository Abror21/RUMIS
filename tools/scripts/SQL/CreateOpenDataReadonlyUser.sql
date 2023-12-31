USE rumis;

-- Mainīgo deklarācija un inicializācija
--
-- Pirms tiek laists skripts, nepieciešams aizvietot sekojošās vērtības
-- ar sistēmas videi atbilstošām vērtībām.
SET @username = 'OpenDataReader';
SET @password = '';

-- Izpildāmās darbības
START TRANSACTION;
	-- Lietotāja veidošana
	SET @createUserSql = CONCAT('CREATE USER ', @username, '@\'%\' IDENTIFIED BY \'', @password, '\';');

	PREPARE createUserStatement FROM @createUserSql;

	EXECUTE createUserStatement;

	DEALLOCATE PREPARE createUserStatement;

    -- Tiesību piešķiršana
	SET @grantPermissionsSql = CONCAT('GRANT SELECT ON rumis.open_data TO ', @username, '@\'%\';');

	PREPARE grantPermissionsStatement FROM @grantPermissionsSql;

	EXECUTE grantPermissionsStatement;

	DEALLOCATE PREPARE grantPermissionsStatement;

	FLUSH PRIVILEGES;

COMMIT;