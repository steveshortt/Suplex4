ALTER FUNCTION [SPLX].[Splx_Not](@compareValue [binary](256))
RETURNS [binary](256) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [Splx_BitLib].[BinaryUtil].[Not]