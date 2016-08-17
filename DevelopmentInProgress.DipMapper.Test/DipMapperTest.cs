using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperTest
    {
        [TestMethod]
        public void GetPropertyInfos_ExcludeUnsupportedProperties_IncludeGenericProperty()
        {
            // Arrange

            // Act
            var propertyInfos = DipMapper.GetPropertyInfos<GenericActivity<Int32>>();

            // Assert
            Assert.AreEqual(propertyInfos.Count(), 8);
            Assert.AreEqual(propertyInfos.ElementAt(0).Name, "Id");
            Assert.AreEqual(propertyInfos.ElementAt(1).Name, "Name");
            Assert.AreEqual(propertyInfos.ElementAt(2).Name, "Level");
            Assert.AreEqual(propertyInfos.ElementAt(3).Name, "IsActive");
            Assert.AreEqual(propertyInfos.ElementAt(4).Name, "Created");
            Assert.AreEqual(propertyInfos.ElementAt(5).Name, "Updated");
            Assert.AreEqual(propertyInfos.ElementAt(6).Name, "ActivityType");
            Assert.AreEqual(propertyInfos.ElementAt(7).Name, "GenericProperty");
        }

        [TestMethod]
        public void GetPropertyInfos_ExcludeUnsupportedProperties_ExcludeGenericProperty()
        {
            // Arrange

            // Act
            var propertyInfos = DipMapper.GetPropertyInfos<GenericActivity<Activity>>();

            // Assert
            Assert.AreEqual(propertyInfos.Count(), 7);
            Assert.AreEqual(propertyInfos.ElementAt(0).Name, "Id");
            Assert.AreEqual(propertyInfos.ElementAt(1).Name, "Name");
            Assert.AreEqual(propertyInfos.ElementAt(2).Name, "Level");
            Assert.AreEqual(propertyInfos.ElementAt(3).Name, "IsActive");
            Assert.AreEqual(propertyInfos.ElementAt(4).Name, "Created");
            Assert.AreEqual(propertyInfos.ElementAt(5).Name, "Updated");
            Assert.AreEqual(propertyInfos.ElementAt(6).Name, "ActivityType");
        }

        [TestMethod]
        public void GetSqlTableName_NonGenericClass_TestPasses()
        {
            // Arrange

            // Act
            var tableName = DipMapper.GetSqlTableName<Activity>();

            // Assert
            Assert.AreEqual(tableName, "Activity");
        }

        [TestMethod]
        public void GetSqlTableName_GenericClass_TestPasses()
        {
            // Arrange

            // Act
            var tableName = DipMapper.GetSqlTableName<GenericActivity<Activity>>();

            // Assert
            Assert.AreEqual(tableName, "Activity");
        }

        [TestMethod]
        public void GetSqlSelectFields_NonGenericClass_TestPasses()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var selectFields = DipMapper.GetSqlSelectFields(propertyInfos);

            // Assert
            Assert.AreEqual(selectFields, "Id, Name, Level, IsActive, Created, Updated, ActivityType");
        }

        [TestMethod]
        public void GetSqlSelectFields_GenericClass_IncludeGenericProperty()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<GenericActivity<Int32>>();

            // Act
            var selectFields = DipMapper.GetSqlSelectFields(propertyInfos);

            // Assert
            Assert.AreEqual(selectFields, "Id, Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty");
        }

        [TestMethod]
        public void GetSqlWhereAssignment_TestNull_NullValueTest()
        {
            // Arrange

            // Act
            var assignment = DipMapper.GetSqlWhereAssignment(null);

            // Assert
            Assert.AreEqual(assignment, " is ");
        }

        [TestMethod]
        public void GetSqlWhereAssignment_TestNull_EmptyStringTest()
        {
            // Arrange

            // Act
            var assignment = DipMapper.GetSqlWhereAssignment("");

            // Assert
            Assert.AreEqual(assignment, " is ");
        }

        [TestMethod]
        public void GetSqlWhereAssignment_TestValue_TestPasses()
        {
            // Arrange

            // Act
            var assignment = DipMapper.GetSqlWhereAssignment(1);

            // Assert
            Assert.AreEqual(assignment, "=");
        }

        [TestMethod]
        public void GetSqlWhereClause_NoParameters_EmptyWhereClause()
        {
            // Arrange

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(null);

            // Assert
            Assert.AreEqual(sqlWhereClause, string.Empty);
        }

        [TestMethod]
        public void GetSqlWhereClause_WithParameters_WhereClause()
        {
            // Arrange
            var genericActivity = new GenericActivity<Activity>() {Name = "Activity"};

            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", genericActivity.Id);
            parameters.Add("Name", genericActivity.Name);
            parameters.Add("Level", genericActivity.Level);
            parameters.Add("IsActive", genericActivity.IsActive);
            parameters.Add("Created", genericActivity.Created);
            parameters.Add("Updated", genericActivity.Updated);
            parameters.Add("ActivityType", genericActivity.ActivityType);

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(parameters);

            // Assert
            Assert.AreEqual(sqlWhereClause, " WHERE Id=@_Id AND Name=@_Name AND Level=@_Level AND IsActive=@_IsActive AND Created=@_Created AND Updated is @_Updated AND ActivityType=@_ActivityType");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipIdentityFieldOnly_GetSql()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Updated", "Id" };            

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @ActivityType)");
        }

        [TestMethod]
        public void GetConnType_GetForSqlConn()
        {
            // Arrange
            var conn = new SqlConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType, DipMapper.ConnType.MSSQL);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetConnType_GetForOleDbConnection_NotImplementedException()
        {
            // Arrange
            var conn = new OleDbConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType, DipMapper.ConnType.MSSQL);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetConnType_GetForOdbcConnection_NotImplementedException()
        {
            // Arrange
            var conn = new OdbcConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType, DipMapper.ConnType.MSSQL);
        }

        [TestMethod]
        public void GetIdentitySql_SCOPE_IDENTITY()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);

            // Act
            var identitySql = DipMapper.GetIdentitySql(connType);

            // Assert
            Assert.AreEqual(identitySql, "SCOPE_IDENTITY()");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipIdentityOnUpdateOnly_GetSql()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id" };

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id", "Created", "Updated"};

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType");
        }
        
        [TestMethod]
        public void GetSqlSelect_NoParameters()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(propertyInfos);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity;");
        }

        [TestMethod]
        public void GetSqlSelect_WithParameters()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 3}};

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(propertyInfos, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@_Id;");
        }

        [TestMethod]
        public void GetSqlInsert_NoIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Id, Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Id, @Name, @Level, @IsActive, @Created, @Updated, @ActivityType);");
        }

        [TestMethod]
        public void GetSqlInsert_WithIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipIdentity = new List<string>() {"Id"};

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "Id", skipIdentity);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = SCOPE_IDENTITY();");
        }

        [TestMethod]
        public void GetSqlInsert_SkipFields()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() {"Id", "Created", "Updated"};

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "Id", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, ActivityType) VALUES (@Name, @Level, @IsActive, @ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = SCOPE_IDENTITY();");
        }

        [TestMethod]
        public void GetSqlUpdate_WithoutParameters()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>();
            var skipFields = new List<string>();

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Id=@Id, Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType;");
        }

        [TestMethod]
        public void GetSqlUpdate_WithParameters()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 5}};
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@_Id;");
        }

        [TestMethod]
        public void GetSqlUpdate_SkipFields()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id", "Created", "Updated"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType WHERE Id=@_Id;");
        }

        [TestMethod]
        public void xx()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
