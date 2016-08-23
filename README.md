# dipmapper

[![Build status](https://ci.appveyor.com/api/projects/status/rhnnr0xn7j8i5ayf?svg=true)](https://ci.appveyor.com/project/grantcolley/dipmapper)

DipMapper is a lightweight object mapper that extends IDbConnection allowing you to map data to your objects (and vice versa) in a clean and easy way.

## Example usage:

### Inserting a record
```C#
            var read = new Activity()
            {
                Name = "Read",
                Level = 1,
                IsActive = true,
                Created = DateTime.Today,
                Updated = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared,
            };
                
            using (var conn = new SqlConnection(connectionString))
            {
                // Insert a record passing in the identity field name.
                read = conn.Insert(read, "Id");
            }
```

### Select a single record
```C#
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activity = conn.Single<Activity>(parameters);
            }
```

### Select many records
```C#
            var parameters = new Dictionary<string, object>() { { "IsActive", true } };
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activities = conn.Select<Activity>(parameters);
            }
```

### Update a record
```C#
            read.Name = "Read Only";
            
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };
            var skipFieldsOnUpdate = new[] { "Id" };

            using (var conn = new SqlConnection(connectionString))
            {
                // Specify which fields to skip when updating e.g. identity columns.
                conn.Update(read, parameters, skipFieldsOnUpdate);
            }
```

### Delete a record
```C#
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Delete<Activity>(parameters);
            }
```

### Execute SQL
```C#
            var sql = "SELECT * FROM Activity WHERE IsActive = 1;";

            using (var conn = new SqlConnection(connectionString))
            {
                var activities = conn.ExecuteSql<Activity>(sql);
            }
```

### Execute Stored Procedure
```C#
            var parameters = new Dictionary<string, object>() { { "IsActive", true } };

            using (var conn = new SqlConnection(connectionString))
            {
                var activities = activities = conn.ExecuteProcedure<Activity>("GetActivities", parameters);
            }
```

### Execute Scalar
```C#
            using (var conn = new SqlConnection(connectionString))
            {
                // SQL
                var activityName = conn.ExecuteScalar("SELECT Name FROM Activity WHERE Id = 123");
                
                // Stored procedure
                var count = conn.ExecuteScalar("GetActivityCount", parameters, CommandType.StoredProcedure);
            }
```

### Execute Non Query
```C#
            using (var conn = new SqlConnection(connectionString))
            {
                // SQL
                conn.ExecuteNonQuery("UPDATE Activity SET IsActive = 1;");
                
                // Stored procedure
                conn.ExecuteNonQuery("ResetAllActivities", parameters, CommandType.StoredProcedure);
            }
```

## IDbConnection Extensions
```C# 
Single<T>(Dictionary<string, object> parameters, IDbTransaction transaction, bool closeAndDisposeConnection)
```

```C#Select<T>(Dictionary<string, object> parameters, IDbTransaction transaction, bool closeAndDisposeConnection, bool optimiseObjectCreation)```

`Insert<T>(T target, string identityField, IEnumerable<string> skipOnCreateFields, IDbTransaction transaction, bool closeAndDisposeConnection)`

`Update<T>(T target, Dictionary<string, object> parameters, IEnumerable<string> skipOnUpdateFields, IDbTransaction transaction, bool closeAndDisposeConnection)`

`Delete<T>(Dictionary<string, object> parameters, IDbTransaction transaction, bool closeAndDisposeConnection)`

`ExecuteNonQuery(string sql, Dictionary<string, object> parameters, CommandType commandType, IDbTransaction transaction, bool closeAndDisposeConnection)`

`ExecuteScalar(string sql, Dictionary<string, object> parameters, CommandType commandType, IDbTransaction transaction, bool closeAndDisposeConnection)`

`ExecuteSql<T>(string sql, IDbTransaction transaction, bool closeAndDisposeConnection, bool optimiseObjectCreation)`

`ExecuteProcedure<T>(string procedureName, Dictionary<string, object> parameters, IDbTransaction transaction, bool closeAndDisposeConnection, bool optimiseObjectCreation)`

## Parameters


1. Nuget package
2. Cache dynamic methods for optimisation
3. Current support for SQL Server only
4. Unsupported fields
5. Table name for generic classes
6. GetSqlWhereAssignment - null string treatment
7. Describe parameters
        
