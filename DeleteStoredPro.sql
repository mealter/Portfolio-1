CREATE PROCEDURE  [dbo].[udpDelStudent] (@id int, @err varchar(1000) OUT)
AS
BEGIN
	BEGIN TRY
	Delete from [dbo].[Student] where [StudentId] = @id;
	SET @err = ''
	RETURN (@@ROWCOUNT)
	END TRY
	BEGIN CATCH
		set @err = ERROR_MESSAGE()
		return (-1)
	end catch
end
