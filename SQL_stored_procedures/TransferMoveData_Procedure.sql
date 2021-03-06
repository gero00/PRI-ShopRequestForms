USE [AFODOM_Test]
GO

ALTER PROCEDURE [dbo].[TransferMoveData] AS

DECLARE @from_building varchar(200)
DECLARE @from_buildingid varchar(25)
DECLARE @from_room varchar (100)
DECLARE @job_id int

DECLARE cur CURSOR FOR
SELECT FromBuilding, BuildingID, FromRoom, id FROM dbo.MoveJob

OPEN cur

FETCH NEXT FROM cur into @from_building, @from_buildingid, @from_room, @job_id;

WHILE @@FETCH_STATUS = 0
BEGIN
	UPDATE dbo.MoveItems  
		SET FromBuilding = @from_building, FromBuildingID = @from_buildingid, FromRoom = @from_room
		WHERE JobID = @job_id;
	PRINT ('Record for JobID ' + CAST(@job_id as varchar(5) ) + ' was updated.')

	FETCH NEXT FROM cur INTO @from_building, @from_buildingid, @from_room, @job_id;
END

/*
ALTER TABLE dbo.MOCK_DATA DROP COLUMN deleteme2
ALTER TABLE dbo.MOCK_DATA DROP COLUMN deleteme3
ALTER TABLE dbo.MOCK_DATA DROP COLUMN deleteme4
*/

CLOSE cur;
DEALLOCATE cur;





