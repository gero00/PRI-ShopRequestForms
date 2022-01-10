USE AFODOM_Test
GO

CREATE PROCEDURE AddFacilitiesWONumbers AS

DECLARE @WONumberText varchar(100)
DECLARE @WONumberInt int
DECLARE @WOid uniqueidentifier

/*
Add Facilities WO Numbers
Last WO Number in previous data from Drupal for Facilities is 5864
*/

SET @WONumberInt = 5864

DECLARE cur CURSOR FOR
SELECT ID FROM dbo.FacilitiesWorkRequest

OPEN cur

FETCH NEXT FROM cur into @WOid;

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @WONumberInt = @WONumberInt + 1
	SET @WONumberText = CAST(@WONumberInt as varchar(100))

	UPDATE dbo.FacilitiesWorkRequest  
		SET WONumber = @WONumberText
		WHERE ID = @WOid

	FETCH NEXT FROM cur INTO @WOid;
END

CLOSE cur;
DEALLOCATE cur;


