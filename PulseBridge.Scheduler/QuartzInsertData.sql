
INSERT INTO [dbo].[QRTZ_JobQueue]
           ([JobType]
           ,[Payload]
           ,[Status]
           ,[Attempts]
           ,[AvailableAt]
           ,[LockedAt]
           ,[LockedBy]
           ,[DispatchedAt]
           ,[LastError])
     VALUES
           ('SignalR'
           ,'Test payload'
           ,0
           ,0
           ,GETUTCDATE()
           ,null
           ,null
           ,null
           ,null
           )



