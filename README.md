# dipmapper

[![Build status](https://ci.appveyor.com/api/projects/status/rhnnr0xn7j8i5ayf?svg=true)](https://ci.appveyor.com/project/grantcolley/dipmapper)

[NuGet package](https://www.nuget.org/packages/DipMapper/).

DipMapper is a lightweight object mapper that extends IDbConnection allowing you to map data to your objects (and vice versa) in a clean and easy way.

####Table of Contents
[Example Usage](#example-usage)  
  [Inserting a record](#inserting-a-record)   
  [Sql Server Insert](#sql-server-insert) 
  [Select a single record](#select-a-single-record)  
  [Select many records](#select-many-records)  
  [Update a record](#update-a-record)  
  [Delete a record](#delete-a-record)  
  [Execute SQL](#execute-sql)  
  [Execute Stored Procedure](#execute-stored-procedure)  
  [Execute Scalar](#execute-scalar)  
  [Execute Non Query](#execute-non-uery)

## Example usage:

### Inserting a record
#### Sql Server Insert
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
            
            // Insert retuns the object fully populated including  
            // auto-generated identifier and other default value.
            Assert.AreEqual(read.Id, 1)
```
*SQL generated*
```sql
            // SQL generated
            INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) 
            VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);
            SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType 
            FROM Activity WHERE Id = SCOPE_IDENTITY();
```

### Select a single record
```C#
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activity = conn.Single<Activity>(parameters);
            }
```

*SQL generated*
```sql
SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@pId;
```

### Select many records
```C#
            var parameters = new Dictionary<string, object>() { { "IsActive", true } };
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activities = conn.Select<Activity>(parameters);
            }
```

*SQL generated*
```sql
SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE IsActive=@pIsActive;
```

### Update a record
```C#
            read.Name = "Read Only";
            
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };
            var skipFieldsOnUpdate = new[] { "Id" };

            using (var conn = new SqlConnection(connectionString))
            {
                // Specify which fields to skip when updating e.g. identity column or read-only fields.
                conn.Update(read, parameters, skipFieldsOnUpdate);
            }
```

*SQL generated*
```sql
UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@pId;
```

### Delete a record
```C#
            var parameters = new Dictionary<string, object>() { { "Id", 123 } };

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Delete<Activity>(parameters);
            }
```

*SQL generated*
```sql
DELETE FROM Activity WHERE Id=@pId;
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
                var activities = conn.ExecuteProcedure<Activity>("GetActivities", parameters);
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
DipMapper provides the following extentions to IDbConnection.

```C# 
T Single<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, 
                        IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
```

```C#
IEnumerable<T> Select<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, 
                        IDbTransaction transaction = null, bool closeAndDisposeConnection = false,
                        bool optimiseObjectCreation = false)
```

```C#
T Insert<T>(this IDbConnection conn, T target, string identityField, IEnumerable<string> 
                        skipOnCreateFields = null, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false)
```

```C#
int Update<T>(this IDbConnection conn, T target, Dictionary<string, object> parameters = null, 
                        IEnumerable<string> skipOnUpdateFields = null, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false)
```

```C#
int Delete<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, 
                        IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
```

```C#
int ExecuteNonQuery(this IDbConnection conn, string sql, Dictionary<string, object> parameters = null, 
                        CommandType commandType = CommandType.Text, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false)
```

```C#
object ExecuteScalar(this IDbConnection conn, string sql, Dictionary<string, object> parameters = null, 
                        CommandType commandType = CommandType.Text, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false)
```

```C#
IEnumerable<T> ExecuteSql<T>(this IDbConnection conn, string sql, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false)
```

```C#
IEnumerable<T> ExecuteProcedure<T>(this IDbConnection conn, string procedureName, 
                        Dictionary<string, object> parameters = null, IDbTransaction transaction = null, 
                        bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false)
```

## Parameter Description and Usage
- **Dictionary\<string, object> parameters**.List of key value pairs where the key is the field name.  

- **IDbTransaction transaction**. Transaction support.

- **bool closeAndDisposeConnection**. Indicates whether to close the connection and dispose it on completion of database execution. False by default. Typically used when the connection is not created within a `using` block. Do not set to true when using a transaction.

- **bool optimiseObjectCreation**. A flag to indicate whether to use a *DynamicMethod* emitting IL to create objects of a given type for the results of a query. The *DynamicMethod* delegate is cached for re-use and can offer better performance when creating objects for large recordsets of a specified type. If false (default) then `Activator.CreateInstance<T>()` is used instead for object creation.

- **T target**. The target object to update or insert.
 
- **string identityField**. The identity field which is excluded from the SQL generated for the *INSERT* statement. 

- **IEnumerable\<string> skipOnCreateFields**. Additional fields to exclude from the SQL generated not insert the *INSERT* statement. Typically these would be fields where default values set by the database is preferred.

- **IEnumerable\<string> skipOnUpdateFields**. Fields to exclude from the SQL generated for the *UPDATE* statement. Typically these will be read-only fields that shouldn't be updated.
 
- **string sql**. SQL statement or stored procedure name, depending on the specified command type.

- **CommandType commandType**. Indicates whether to execute a SQL statement or stored procedure.

## Rermarks
###Unsupported Fields
DipMapper uses reflection to generate the SQL statements for the desired action. It will skip non-public properties and properties that are either classes (except for strings), interfaces, lists or arrays.

*For example*
```C#
    public class Activity<T>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Level { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public ActivityTypeEnum ActivityType { get; set; }

        // Supported by DipMapper only if T is a value type.
        public T GenericProperty { get; set; }

        // Not supported by DipMapper.
        public Activity<T> ParentActivity { get; set; }
        public IEnumerable<Activity<T>> Activities_1 { get; set; }
        public IList<Activity<T>> Activities_2 { get; set; }
        public T[] GroupIds { get; set; }
        internal string Description { get; set; }
        protected int AssociatedActivityId { get; set; }
        private int ParentActivityId { get; set; }
    }
    
    var parameters = new Dictionary<string, object>() { { "Id", 123 } };
            
    using (var conn = new SqlConnection(connectionString))
    {
        var admin = conn.Single<Activity<Admin>>(parameters);
    }
    
    // The following sql is generated.
    SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Admin WHERE Id=@pId;
```

### Generic Classes
When working with a generic class the table name will be the specified type.

*For example*
```C#
    var admin = conn.Single<Activity<Admin>>(parameters);
    SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Admin WHERE Id=@pId;
    
    // Unfortunately, this can also backfire. 
    var admin = conn.Single<Activity<Int32>>(parameters);
    SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty FROM Int32 WHERE Id=@pId;    
```

### Building the WHERE Clause
Paremeters with values are assigned using `=` whereas null parameters are assigned using `is`.

> **_NOTE:_**
> Empty string values will be treated as null. 

```C#
    var parameters = new Dictionary<string, object>() { { "IsActive", true } };
    var activities = conn.Select<Activity>(parameters);
    SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE IsActive=@pIsActive;

    var parameters = new Dictionary<string, object>() { { "Updated", null } };
    var activities = conn.Select<Activity>(parameters);
    SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Updated is NULL;
```
