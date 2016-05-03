USE K2;

BEGIN
	DELETE FROM dbo.AM_SecurableType;
	DELETE FROM dbo.AM_PermissionType;

	INSERT INTO dbo.AM_SecurableType (ID, Name, [Description], Sort_Order, Value, Active, Created_By, Created_Date)
	SELECT '4A315F51-113F-48D2-BE36-6C881C3BF093','View','Secure a SmartForm View component',1,1,1,'System', GETDATE() UNION
	SELECT 'FA16F979-7E5E-4BD0-843B-BB0D8C26390D','Form','Secure a SmartForm Form component',2,2,1,'System', GETDATE() UNION
	SELECT 'A2751B52-D37E-4AF1-B8BD-A11CAB668A87','Views and Forms','Secure SmartForm View and Form components',3,-1,1,'System', GETDATE();

	INSERT INTO dbo.AM_PermissionType (ID, Name, [Description], Sort_Order, Value, Active, Created_By, Created_Date)
	SELECT '12050CC5-8B17-4002-86B4-21799E263A85','View','View access to a SmartForm component',1,1,1,'System', GETDATE() UNION
	SELECT '199C6E98-B515-44EE-92C1-29FFC27DCFA9','Create','Create access for a new SmartForm component',2,2,1,'System', GETDATE() UNION
	SELECT 'A603AD9B-ADE6-4B73-97CB-DCCB20580059','Edit','Edit access for an existing SmartForm component',3,3,1,'System', GETDATE() UNION
	SELECT 'D40602F2-A146-4FC4-9814-5C92129C5468','Delete','Delete access for an existing SmartForm component',4,4,1,'System', GETDATE() UNION
	SELECT '912DCB2E-42E8-4D59-ABC6-2FF92376FCBF','Full','Full access for any SmartForm component',5,-1,1,'System', GETDATE();

	SELECT * FROM dbo.AM_SecurableType;
	SELECT * FROM dbo.AM_PermissionType;
END;


