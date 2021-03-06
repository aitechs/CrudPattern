SET NOCOUNT ON;
DECLARE @objName NVARCHAR(100);
DECLARE @parameterCount INT;
DECLARE @errMsg VARCHAR(100);

DECLARE @fields	    nvarchar(max) = '';

SET @objName = 'Person';


SELECT dbo.sysobjects.name AS ObjName,
       dbo.sysobjects.xtype AS ObjType,
       dbo.syscolumns.name AS ColName,
       dbo.syscolumns.colorder AS ColOrder,
       dbo.syscolumns.length AS ColLen,
       dbo.syscolumns.colstat AS ColKey,
       dbo.systypes.xtype
INTO #t_obj

FROM dbo.syscolumns
     INNER JOIN dbo.sysobjects ON dbo.syscolumns.id = dbo.sysobjects.id
     INNER JOIN dbo.systypes ON dbo.syscolumns.xtype = dbo.systypes.xtype
WHERE(dbo.sysobjects.name = @objName)
     AND (dbo.systypes.status <> 1)
--*ORDER BY 
    --dbo.sysobjects.name, 
    --dbo.syscolumns.colorder;

SET @parameterCount = ( SELECT COUNT(*) FROM #t_obj );


IF(@parameterCount < 1)
    SET @errMsg = 'No Parameters/Fields found for '+@objName;

IF(@errMsg IS NULL)
    BEGIN
        DECLARE @source_name NVARCHAR, @source_type VARCHAR, @col_name NVARCHAR(100), @col_order INT, @col_type VARCHAR(20), @col_len INT, @col_key INT, @col_xtype INT, @col_redef VARCHAR(20);
        DECLARE cur CURSOR
        FOR
            SELECT *
            FROM #t_obj order by ColOrder;
        OPEN cur;
	   
	   -- Perform the first fetch.
        FETCH NEXT FROM cur INTO @source_name, @source_type, @col_name, @col_order, @col_len, @col_key, @col_xtype;
  
	   -- Check @@FETCH_STATUS to see if there are any more rows to fetch.
        WHILE @@FETCH_STATUS = 0
        BEGIN
			
			SET @fields = CONCAT(@fields, 'item.' + @col_name +' = record.' +@col_name, char(13))

			FETCH NEXT FROM cur INTO @source_name, @source_type, @col_name, @col_order, @col_len, @col_key, @col_xtype;	 	   
		END;
	   

	   PRINT @fields;
	   



        CLOSE cur;
        DEALLOCATE cur;
END;

IF(LEN(@errMsg) > 0)
    PRINT @errMsg;

DROP TABLE #t_obj;
SET NOCOUNT ON;
GO
SET QUOTED_IDENTIFIER OFF;
GO
SET ANSI_NULLS ON;
GO