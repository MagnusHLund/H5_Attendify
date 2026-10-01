# Backend entity relationship diagram

This diagram reflects the EF Core model snapshot and configurations in `src/WebApi/Common/Persistence`. The database is PostgreSQL. Primary keys, foreign keys, and uniquely indexed columns are marked. Every table inherits `CreatedAt`, `CreatedBy`, `UpdatedAt`, and `UpdatedBy` from the shared `Auditable` base class; these columns are listed on each table because they are present in every table in the database.

```mermaid
erDiagram
    EDUCATIONAL_INSTITUTES {
        uuid Id PK
        string Name UK
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    USERS {
        int Id PK
        uuid EducationalInstituteId FK
        string Email UK
        string PasswordHash
        string EncryptedStudentId
        boolean AttendanceEnabled
        boolean IsDeleted
        datetime DeletedAt
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    FACIAL_PROFILES {
        int Id PK
        int UserId FK,UK
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    FACIAL_EMBEDDINGS {
        int Id PK
        int FacialProfileId FK
        bytes EncryptedEmbedding
        bytes Nonce
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    ATTENDANCES {
        int Id PK
        int UserId
        date AttendanceDate
        time ArrivedAt
        string Classroom
        int Status
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    STUDENT_ACCESS_CODES {
        int Id PK
        int UserId FK
        string AccessCodeHash
        datetime GeneratedAt
        date GenerationDate
        datetime ExpiresAt
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    REFRESH_TOKENS {
        int Id PK
        int UserId FK
        uuid TokenFamilyId
        bytes TokenHash UK
        datetime ExpiresAt
        datetime RevokedAt
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    PASSWORD_RESET_TOKENS {
        int Id PK
        int UserId FK,UK
        bytes SecurityCodeHash
        datetime ExpiresAt
        int FailedAttempts
        datetime ConsumedAt
        datetime CreatedAt
        string CreatedBy
        datetime UpdatedAt "nullable"
        string UpdatedBy "nullable"
    }

    EDUCATIONAL_INSTITUTES ||--o{ USERS : enrolls
    USERS ||--|| FACIAL_PROFILES : has
    FACIAL_PROFILES ||--o{ FACIAL_EMBEDDINGS : contains
    USERS ||--o{ STUDENT_ACCESS_CODES : receives
    USERS ||--o{ REFRESH_TOKENS : owns
    USERS ||--o{ PASSWORD_RESET_TOKENS : requests
    USERS ||..o{ ATTENDANCES : "UserId logical association only"
```

`FacialProfiles.UserId` is unique, so each profile belongs to exactly one user and each user has at most one profile. The model marks the relationship required; application registration creates a profile for the user. `PasswordResetTokens.UserId` is also unique, allowing at most one reset token per user. Student access codes have a unique composite index on `(UserId, GenerationDate)`, and refresh-token hashes are unique.

`Attendances.UserId` is stored as an integer but the EF model does not configure it as a foreign key to `Users`; its association is logical and is drawn with a dotted edge. `ActiveUserLock` is an update lock on the user row inside a transaction, not a separate table. The former `AttendanceRecords` and `AttendanceDetections` tables are removed from the model; the forward migration drops them.
