ALTER FUNCTION [SPLX].[Splx_ContainsOne](@sourceValue [binary](16), @compareValue [binary](16))
RETURNS [bit] WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [Splx_BitLib].[BinaryUtil].[ContainsOne]