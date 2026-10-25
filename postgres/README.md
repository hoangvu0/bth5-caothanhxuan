# PostgreSQL

Run this script in PostgreSQL to create the case-study databases:

```sql
\i createdatabases.sql
```

After data is added, export backups for submission with `pg_dump`, for example:

```powershell
pg_dump -U postgres -F c -f backups/auth_service.backup auth_service
```
