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

1. Nuget package
2. Appveyor
3. Documentation
4. Unsupported fields
5. Table name for generic classes
6. GetSqlWhereAssignment - null string treatment
        
