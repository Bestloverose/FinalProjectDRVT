# Sprint 1

## Create the emails table

```sql
CREATE TABLE Emails (
    EmailID INT IDENTITY(1,1) PRIMARY KEY,
    EmailSubject NVARCHAR(255),
    EmailMessage NVARCHAR(MAX),
    EmailDate DATETIME,
    EmailIsRead BIT,
    EmailSender NVARCHAR(255),
    EmailReceiver NVARCHAR(255)
);
```
