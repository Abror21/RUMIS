UPDATE IdentityUserLogins 
SET PasswordHash = NULL 
WHERE UserName = 'admin'