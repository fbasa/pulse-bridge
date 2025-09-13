﻿namespace PulseBridge.Infrastructure;

public class SqlTemplates
{
    public const string ClaimJobs = @"
                SET XACT_ABORT ON;
                BEGIN TRAN;
                DECLARE @claimed TABLE(JobId bigint, JobType nvarchar(64), Payload nvarchar(max), Attempts int);
                WITH cte(JobId,JobType,Payload,Status,Attempts,LockedAt,LockedBy) AS (
                  SELECT TOP (25) JobId, JobType, Payload,Status,Attempts,LockedAt,LockedBy
                  FROM [dbo].[QRTZ_JobQueue] WITH (READPAST, UPDLOCK, ROWLOCK)
                  WHERE Status = 0 AND AvailableAt <= SYSUTCDATETIME()
                  ORDER BY JobId
                )
                UPDATE cte
                   SET Status=1, Attempts=Attempts+1, LockedAt=SYSUTCDATETIME(), LockedBy=@worker
                OUTPUT inserted.JobId, inserted.JobType, inserted.Payload, inserted.Attempts INTO @claimed;
                SELECT JobId, JobType, Payload, Attempts FROM @claimed;
                COMMIT;
            ";
    public const string RequeWithBackoff = @"
                UPDATE [dbo].[QRTZ_JobQueue]
                SET Status = 0,
                    AvailableAt = DATEADD(SECOND,
                                            CASE WHEN @attempts > 10 THEN 300 ELSE CONVERT(int, POWER(2, @attempts)) END,
                                            SYSUTCDATETIME()),
                    LastError = LEFT(@error, 4000),
                    LockedBy = NULL,
                    LockedAt = NULL
                WHERE JobId = @jobId;
            ";
    public const string MarkDispatched = @"
                UPDATE [dbo].[QRTZ_JobQueue]
                SET DispatchedAt = SYSUTCDATETIME()
                WHERE JobId = @jobId;
            ";
    public const string MarkJobCompleted = @"
                UPDATE [dbo].[QRTZ_JobQueue]
                SET Status=2, LockedAt=NULL, LockedBy=NULL, LastError=NULL
                WHERE JobId = @jobId;
            ";

    public const string GetSignalRJobs = @"
                SELECT JobId, JobType, Payload, Status
                FROM [dbo].[QRTZ_JobQueue]
                WHERE JobType = 'SignalR'
                ORDER BY JobId DESC
                OFFSET 0 ROWS FETCH NEXT 25 ROWS ONLY";

    public const string InsertSignalRJob = @"INSERT INTO [dbo].[QRTZ_JobQueue]([JobType],[Payload],[Status],[Attempts],[AvailableAt])VALUES('SignalR','Test payload',0,0,GETUTCDATE())";
}
