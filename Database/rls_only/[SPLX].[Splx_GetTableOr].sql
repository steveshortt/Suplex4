ALTER FUNCTION [SPLX].[Splx_GetTableOr](@tableName [nvarchar](max), @maskFieldName [nvarchar](max), @bitArrayLength [int])
RETURNS [binary](256) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [Splx_BitLib].[BinaryUtil].[GetTableOr]