-- =============================================
-- Author:		林建生
-- Create date: 2010-12-17
-- Description:	删除记录
-- =============================================
CREATE PROCEDURE [dbo].[Table_Delete]
@TableName varchar(20), --要查询的表名
@IDName varchar(20),    --要查询条件的字段名
@IDValues varchar(200)  --要查询条件的字段名的值
 AS 
	 BEGIN
		DECLARE @SQL VARCHAR(300)
		SET @SQL=N'DELETE ' + @TableName + N' where ' + @IDName + N' in(' + @IDValues + N')'
		EXEC(@SQL)
		return @@rowcount
	 END

GO

	 -- =============================================
-- Author:		林建生
-- Create date: 2010-12-30
-- Description:	查询有多少记录
-- =============================================
CREATE procedure [dbo].[Table_Count] 
(
@TableName varchar(50), --要查询的表名
@Where varchar (5000)     --要查询的条件
)
as
DECLARE @SQL VARCHAR(1500)       --拼接的SQL
DECLARE @TableWhere varchar (5000) --查询的表明和查询条件

if(@where<>'')
begin
	SET @TableWhere = @TableName + N' where ' + @Where
end
else
begin
	SET @TableWhere = @TableName
end

SET @SQL=N'select count(*) as count from '+ @TableWhere
EXEC(@SQL)

GO

-- =============================================
-- Author:		林建生
-- Create date: 2010-12-30
-- Description:	分页存储过程SQL2005及以上通用
CREATE procedure [dbo].[Table_List] 
(
@TableName varchar(20), --要查询的表名
@Fields varchar(500),   --要查询的字段名称
@Where varchar (5000),    --要查询的条件
@OrderBy varchar(200),   --排序
@PageSize int,          --显示条数
@PageIndex int          --页数
)
as
DECLARE @SQL VARCHAR(1500)       --拼接的SQL
DECLARE @TableWhere varchar (5000) --查询的表明和查询条件
DECLARE @StartNumber VARCHAR(10) --第几条数据开始
DECLARE @EndNumber VARCHAR(10)   --第几条数据结束
DECLARE @Row VARCHAR(100)        --显示第几条到第几条

if(@where<>'')
begin
	SET @TableWhere = @TableName + N' where ' + @Where
end
else
begin
	SET @TableWhere = @TableName
end
IF(@PageSize = 0)
BEGIN
	SET @Row = N'' 
END
ELSE
BEGIN
	SET @StartNumber =(@PageIndex-1)*@PageSize+1
	SET @EndNumber =(@PageIndex-1)*@PageSize+@PageSize
	SET @Row = N' where Row between '+ @StartNumber + N' and '+ @EndNumber
END
SET @SQL=N'with temptbl as (SELECT ROW_NUMBER() OVER (ORDER BY '+@OrderBy + N')AS Row, '+@Fields +N' from '+@TableWhere + N') SELECT * FROM temptbl'+ @Row
EXEC(@SQL)

GO

-- =============================================
-- Author:		林建生
-- Create date: 2015-12-30
-- Description:	通用SQL分页

CREATE PROCEDURE [dbo].[sys_Page_v2]  
@PCount int output,    --总页数输出  
@RCount int output,    --总记录数输出  
@sys_Table nvarchar(100),    --查询表名  
@sys_Key varchar(50),        --主键  
@sys_Fields nvarchar(500),    --查询字段  
@sys_Where nvarchar(3000),    --查询条件  
@sys_Order nvarchar(100),    --排序字段  
@sys_Begin int,        --开始位置  
@sys_PageIndex int,        --当前页数  
@sys_PageSize int        --页大小  
AS  
SET NOCOUNT ON  
SET ANSI_WARNINGS ON  
IF @sys_PageSize < 0 OR @sys_PageIndex < 0  
BEGIN          
RETURN  
END  
DECLARE @new_where1 NVARCHAR(3000)  
DECLARE @new_order1 NVARCHAR(100)  
DECLARE @new_order2 NVARCHAR(100)  
DECLARE @Sql NVARCHAR(4000)  
DECLARE @SqlCount NVARCHAR(4000)  
DECLARE @Top int  
if(@sys_Begin <=0)  
    set @sys_Begin=0  
else  
    set @sys_Begin=@sys_Begin-1  
IF ISNULL(@sys_Where,'') = ''  
    SET @new_where1 = ' '  
ELSE  
    SET @new_where1 = ' WHERE ' + @sys_Where  
IF ISNULL(@sys_Order,'') <> ''   
BEGIN  
    SET @new_order1 = ' ORDER BY ' + Replace(@sys_Order,'desc','')  
    SET @new_order1 = Replace(@new_order1,'asc','desc')  
    SET @new_order2 = ' ORDER BY ' + @sys_Order  
END  
ELSE  
BEGIN  
    SET @new_order1 = ' ORDER BY ID DESC'  
    SET @new_order2 = ' ORDER BY ID ASC'  
END  
SET @SqlCount = 'SELECT @RCount=COUNT(1),@PCount=CEILING((COUNT(1)+0.0)/'  
            + CAST(@sys_PageSize AS NVARCHAR)+') FROM ' + @sys_Table + @new_where1  
EXEC SP_EXECUTESQL @SqlCount,N'@RCount INT OUTPUT,@PCount INT OUTPUT',  
               @RCount OUTPUT,@PCount OUTPUT  
IF @sys_PageIndex > CEILING((@RCount+0.0)/@sys_PageSize)    --如果输入的当前页数大于实际总页数,则把实际总页数赋值给当前页数  
BEGIN  
    SET @sys_PageIndex =  CEILING((@RCount+0.0)/@sys_PageSize)  
END  
set @sql = 'select '+ @sys_fields +' from ' + @sys_Table + ' w1 '  
    + ' where '+ @sys_Key +' in ('  
        +'select top '+ ltrim(str(@sys_PageSize)) +' ' + @sys_Key + ' from '  
        +'('  
            +'select top ' + ltrim(STR(@sys_PageSize * @sys_PageIndex + @sys_Begin)) + ' ' + @sys_Key + ' FROM '  
        + @sys_Table + @new_where1 + @new_order2   
        +') w ' + @new_order1  
    +') ' + @new_order2  
print(@sql)  
Exec(@sql)  

GO 