INSERT INTO #TempUsers VALUES('Micro90001')

MERGE [Users] AS [Target]
USING 
	(SELECT [Name] FROM #TempUsers) AS [Source]
	ON [Target].[Name] = [Source].[Name]
WHEN MATCHED THEN
	UPDATE SET [Target].[Name] = [Source].[Name]
WHEN NOT MATCHED THEN
	INSERT ([Name]) VALUES ([Source].[Name]);