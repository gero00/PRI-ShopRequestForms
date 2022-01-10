ALTER PROCEDURE BACKUP_MOVES AS 
BEGIN
	DECLARE @job_query AS varchar(MAX)
	DECLARE @items_query AS varchar(MAX)

	SET @job_query = 'select * into dbo.MoveJob_backup from dbo.MoveJob'; 
	EXEC (@job_query)

	SET @items_query = 'select * into dbo.MoveItems_backup from dbo.MoveItems'; 
	EXEC (@items_query)

	PRINT 'Move tables backed up'

END