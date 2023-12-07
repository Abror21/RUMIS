USE rumis;

-- Main�go deklar�cija un inicializ�cija
--
-- Pirms tiek laists skripts, nepiecie�ams aizvietot sekojo��s v�rt�bas
-- ar sist�mas videi atbilsto��m v�rt�b�m.
SET @username = 'OpenDataReader';
SET @password = '';

-- Izpild�m�s darb�bas
START TRANSACTION;
	-- Lietot�ja veido�ana
	SET @createUserSql = CONCAT('CREATE USER ', @username, '@\'%\' IDENTIFIED BY \'', @password, '\';');

	PREPARE createUserStatement FROM @createUserSql;

	EXECUTE createUserStatement;

	DEALLOCATE PREPARE createUserStatement;

    -- Ties�bu pie��ir�ana
	SET @grantPermissionsSql = CONCAT('GRANT SELECT ON rumis.open_data TO ', @username, '@\'%\';');

	PREPARE grantPermissionsStatement FROM @grantPermissionsSql;

	EXECUTE grantPermissionsStatement;

	DEALLOCATE PREPARE grantPermissionsStatement;

	FLUSH PRIVILEGES;

COMMIT;