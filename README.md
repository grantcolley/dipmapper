# dipmapper

[![Build status](https://ci.appveyor.com/api/projects/status/rhnnr0xn7j8i5ayf?svg=true)](https://ci.appveyor.com/project/grantcolley/dipmapper)

[NuGet package](https://www.nuget.org/packages/DipMapper/).

DipMapper is a lightweight object mapper that extends IDbConnection allowing you to map data to your objects (and vice versa) in a clean and easy way.

####Table of Contents
* [Example Usage](#example-usage)  
  * [Inserting a record](#inserting-a-record)  
    * [Sql Server Insert](#sql-server-insert)   
    * [Oracle Insert](#oracle-insert)   
    * [MySql Insert](#mysql-insert)   
    * [Insert specified fields only](#insert-specified-fields-only)

  * [Selecting records](#selecting-records)
    * [Select a single record](#select-a-single-record)  
    * [Select many records](#select-many-records)  

  * [Updating records](#updating-records)
    * [Update a record with Identity parameter](#update-a-record-with-identity-parameter)  
    * [Update specified fields only](#update-specified-fields-only)   

  * [Delete a record](#delete-a-record)  
  
  * [Execute SQL statement](#execute-sql-statement)  
  
  * [Execute Stored Procedure](#execute-stored-procedure)  
  
  * [Execute Scalar](#execute-scalar)  
  
  * [Execute Non Query](#execute-non-query)   

* [IDbConnection Extensions](#idbconnection-extensions)

* [Parameter Description and Usage](#parameter-description-and-usage)

* [Rermarks](#rermarks)
  * [Unsupported Fields](#unsupported-fields)
  * [Generic Classes](#generic-classes)
  * [Building the WHERE Clause](#building-the-where-clause)

## Example usage:
For the most part usage is the same across different databases. Some differences, however, are highlighted in the examples below. For example Sql Server and MySql support auto incrementing identity columns, whereas in Oracle (up to 11g) they are generated by a sequence separate from the insert statement.

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
            
            // Insert retuns the object fully populated  
            // including the auto-generated identifier.
            Assert.AreEqual(read.Id, 1);
            Assert.AreEqual(read.Name, "Read");
            Assert.AreEqual(read.Level, 1);
            Assert.AreEqual(read.IsActive, true);
            Assert.AreEqual(read.Created, DateTime.Today);
            Assert.AreEqual(read.Updated, DateTime.Today);
            Assert.AreEqual(read.ActivityType, ActivityTypeEnum.Shared);
```
*SQL generated*
```sql
            INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) 
            VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);
            SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType 
            FROM Activity WHERE Id = SCOPE_IDENTITY();
```

#### Oracle Insert
```C#
            var read = new Activity()
            {
                Name = "Read",
                Status = 1,
                IsActive = true,
                Created = DateTime.Today,
                Updated = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared,
            };
            
            using (var conn = new OracleConnection(connectionString))
            {
                // Generate the identity from a sequence.
                read.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"Activity_seq\".NEXTVAL FROM DUAL"));
                read = conn.Insert(read);
            }
            
            Assert.AreEqual(read.Id, 1);
            Assert.AreEqual(read.Name, "Read");
            Assert.AreEqual(read.Status, 1);
            Assert.AreEqual(read.IsActive, true);
            Assert.AreEqual(read.Created, DateTime.Today);
            Assert.AreEqual(read.Updated, DateTime.Today);
            Assert.AreEqual(read.ActivityType, ActivityTypeEnum.Shared);
```
*SQL generated*
```sql
            INSERT INTO ActivityOra (Id, Name, Status, IsActive, Created, Updated, ActivityType) 
            VALUES (:Id, :Name, :Status, :IsActive, :Created, :Updated, :ActivityType)
```

#### MySql Insert
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
                
            using (var conn = new MySqlConnection(connectionString))
            {
                // Insert a record passing in the identity field name.
                read = conn.Insert(read, "Id");
            }
            
            // Insert retuns the object fully populated
            // including the auto-generated identifier.
            Assert.AreEqual(read.Id, 1);
            Assert.AreEqual(read.Name, "Read");
            Assert.AreEqual(read.Level, 1);
            Assert.AreEqual(read.IsActive, true);
            Assert.AreEqual(read.Created, DateTime.Today);
            Assert.AreEqual(read.Updated, DateTime.Today);
            Assert.AreEqual(read.ActivityType, ActivityTypeEnum.Shared);
```
*SQL generated*
```sql
            INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) 
            VALUES (?Name, ?Level, ?IsActive, ?Created, ?Updated, ?ActivityType);
            SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType 
            FROM Activity WHERE Id = LAST_INSERT_ID();
```

#### Insert specified fields only
When passing a list of parameters with the Insert call, only those parameters will be inserted.
This is useful when you want to avoid inserting values into fields where it is preferable to use 
Default values applied to inserted records by the database table instead. Note the sql generated below.
```C#
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() {ParameterName = "Name", Value = read.Name});
            parameters.Add(new SqlParameter() { ParameterName = "Level", Value = read.Level });
                
            using (var conn = new MySqlConnection(connectionString))
            {
                read = conn.Insert(read, "Id", parameters);
            }
```
*SQL generated*
```sql
            INSERT INTO Activity (Name, Level) 
            VALUES (@Name, @Level);
            SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType 
            FROM Activity WHERE Id = SCOPE_IDENTITY();
```

### Selecting records
#### Select a single record
```C#
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "Id", Value = 1 });
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activity = conn.Single<Activity>(parameters);
            }
```
*SQL generated*
```sql
SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@pId;
```

#### Select many records
```C#
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "IsActive", Value = true });
            
            using (var conn = new SqlConnection(connectionString))
            {
                var activities = conn.Select<Activity>(parameters);
            }
```
*SQL generated*
```sql
SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE IsActive=@pIsActive;
```

### Updating records
#### Update a record with Identity parameter
When updating with a parameter for the identity of the record, that identity field will be excluded from the update statement.
```C#
            read.Name = "Read Only";
            
            var parameter = new SqlParameter() {ParameterName = "Id", Value = read.Id};

            using (var conn = new SqlConnection(connectionString))
            {
                // Specify which fields to skip when updating e.g. identity column or read-only fields.
                conn.Update(read, parameter);
            }
```
*SQL generated*
```sql
UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, 
       Created=@Created, Updated=@Updated, ActivityType=@ActivityType 
WHERE  Id=@pId;
```

#### Update specified fields only
When updating specified fields only, two parameter lists are used. One for the fields to be updated and one for the where clause.
```C#
            var updateParameters = new List<SqlParameter>();
            updateParameters.Add(new SqlParameter() { ParameterName = "IsActive", Value = false });

            var whereParameters = new List<SqlParameter>();
            whereParameters.Add(new SqlParameter() { ParameterName = "Id", Value = read.Id });

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Update(read, updateParameters, whereParameters);
            }
```
*SQL generated*
```sql
UPDATE Activity SET IsActive=@IsActive WHERE Id=@pId
```

### Delete a record
```C#
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "Id", Value = read.Id });
            
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Delete<Activity>(parameters);
            }
```

*SQL generated*
```sql
DELETE FROM Activity WHERE Id=@pId;
```

### Execute SQL statement
```C#
            var sql = "SELECT * FROM Activity WHERE IsActive = 1;";

            using (var conn = new SqlConnection(connectionString))
            {
                var activities = conn.ExecuteSql<Activity>(sql);
            }
```

### Execute Stored Procedure
```C#
            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter() { ParameterName = "IsActive", Value = true });

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
T Single<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, 
                        IDbTransaction transaction = null)                        
```
```C#
IEnumerable<T> Select<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, 
                        IDbTransaction transaction = null, bool optimiseObjectCreation = false)
```
```C#
T Insert<T>(this IDbConnection conn, T target, IDbTransaction transaction = null)
```
```C#
T Insert<T>(this IDbConnection conn, T target, IEnumerable<IDbDataParameter> insertParameters, 
                        IDbTransaction transaction = null)
```
```C#
T Insert<T>(this IDbConnection conn, T target, string identityField, IDbTransaction transaction = null)
```
```C#
T Insert<T>(this IDbConnection conn, T target, string identityField, 
                        IEnumerable<IDbDataParameter> insertParameters, 
                        IDbTransaction transaction = null)
```
```C#
int Update<T>(this IDbConnection conn, T target, IDbDataParameter identity, IDbTransaction transaction = null)
```
```C#
int Update<T>(this IDbConnection conn, T target, IEnumerable<IDbDataParameter> updateParameters, 
                        IEnumerable<IDbDataParameter> whereClauseParameters, IDbTransaction transaction = null)
```
```C#
int Delete<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, 
                        IDbTransaction transaction = null)
```
```C#
int ExecuteNonQuery(this IDbConnection conn, string sql, IEnumerable<IDbDataParameter> parameters = null, 
                        CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
```
```C#
object ExecuteScalar(this IDbConnection conn, string sql, IEnumerable<IDbDataParameter> parameters = null, 
                        CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
```
```C#
IEnumerable<T> ExecuteSql<T>(this IDbConnection conn, string sql, IDbTransaction transaction = null, 
                        bool optimiseObjectCreation = false)
```
```C#
IEnumerable<T> ExecuteProcedure<T>(this IDbConnection conn, string procedureName, 
                        IEnumerable<IDbDataParameter> parameters = null, IDbTransaction transaction = null, 
                        bool optimiseObjectCreation = false)
```

## Parameter Description and Usage
- **IEnumerable\<IDbDataParameter> parameters**. Parameter list

- **IEnumerable\<IDbDataParameter> insertParameters**. Parameter list for insert fields.

- **IEnumerable\<IDbDataParameter> updateParameters**. Parameter list for update fields.

- **IEnumerable\<IDbDataParameter> whereClauseParameters**. Parameter list for where clause.

- **IDbDataParameter identity**. Parameter for the identity field.

- **IDbTransaction transaction**. Transaction support.

- **string identityField**. The identity field which is excluded from the SQL generated for the *INSERT* statement.

- **bool optimiseObjectCreation**. A flag to indicate whether to use a *DynamicMethod* emitting IL to create objects of a given type for the results of a query. The *DynamicMethod* delegate is cached for re-use and can offer better performance when creating objects for large recordsets of a specified type. If false (default) then `Activator.CreateInstance<T>()` is used instead for object creation.

- **T target**. The target object to update or insert.

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
