

/* Add WONumber & RequestDate columns to all tables */
ALTER TABLE dbo.FacilitiesWorkRequest ADD WONumber varchar(100), RequestDate DATE

ALTER TABLE dbo.ShopRequest ADD WONumber varchar(100), RequestDate DATE
ALTER TABLE dbo.MoveJob ADD WONumber varchar(100), RequestDate DATE

/* ALTER TABLE statements to give "from" attributes to individual item records*/
ALTER TABLE dbo.MoveItems ADD FromBuilding varchar(200) 
ALTER TABLE dbo.MoveItems ADD FromBuildingID varchar(25)
ALTER TABLE dbo.MoveItems ADD FromRoom varchar(200)

/*ALTER TABLE statements for correction field sizes*/
ALTER TABLE dbo.MoveItems ALTER COLUMN MoveType varchar(50)
ALTER TABLE dbo.MoveItems ALTER COLUMN Custodian varchar(100)
ALTER TABLE dbo.FacilitiesWorkRequest ALTER COLUMN RoomNumber varchar(200)