ALTER FUNCTION [SPLX].[Splx_And](@sourceValue [binary](256), @compareValue [binary](256))
RETURNS [binary](256) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [Splx_BitLib].[BinaryUtil].[And]