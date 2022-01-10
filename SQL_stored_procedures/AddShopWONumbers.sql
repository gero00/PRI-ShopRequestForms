USE AFODOM_Test
GO

CREATE PROCEDURE AddShopWONumbers AS

DECLARE @WONumberText varchar(100)
DECLARE @WONumberInt int
DECLARE @WOid int

/*
Add ShopRequest WO Numbers
Last WO Number in previous data from Drupal for Shop is 5862
*/
SET @WONumberInt = 5862

DECLARE cur CURSOR FOR
SELECT ID FROM dbo.ShopRequest

OPEN cur

FETCH NEXT FROM cur into @WOid;

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @WONumberInt = @WONumberInt + 1
	SET @WONumberText = CAST(@WONumberInt as varchar(100))

	UPDATE dbo.ShopRequest 
		SET WONumber = @WONumberText
		WHERE ID = @WOid

	FETCH NEXT FROM cur INTO @WOid;
END

CLOSE cur;
DEALLOCATE cur;