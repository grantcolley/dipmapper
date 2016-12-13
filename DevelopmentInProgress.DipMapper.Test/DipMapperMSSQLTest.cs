using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperMSSQLTest
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
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(connType, null);

            // Assert
            Assert.AreEqual(sqlWhereClause, string.Empty);
        }

        [TestMethod]
        public void GetSqlWhereClause_WithParameters_WhereClause()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
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
            var sqlWhereClause = DipMapper.GetSqlWhereClause(connType, parameters);

            // Assert
            Assert.AreEqual(sqlWhereClause, " WHERE Id=@pId AND Name=@pName AND Level=@pLevel AND IsActive=@pIsActive AND Created=@pCreated AND Updated is @pUpdated AND ActivityType=@pActivityType");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipIdentityFieldOnly_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Updated", "Id" };            

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

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
        [ExpectedException(typeof(NotSupportedException))]
        public void GetIdentitySql_SCOPE_IDENTITY_NotSupportedException()
        {
            // Arrange
            var conn = new OleDbConnection();
            var connType = DipMapper.GetConnType(conn);

            // Act
            var identitySql = DipMapper.GetIdentitySql(connType);

            // Assert
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipIdentityOnUpdateOnly_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id" };

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id", "Created", "Updated"};

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType");
        }
        
        [TestMethod]
        public void GetSqlSelect_NoParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity;");
        }

        [TestMethod]
        public void GetSqlSelect_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 3}};

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@pId;");
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
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>();
            var skipFields = new List<string>();

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Id=@Id, Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType;");
        }

        [TestMethod]
        public void GetSqlUpdate_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 5}};
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetSqlUpdate_SkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id", "Created", "Updated"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetSqlDelete_WithoutParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>();

            // Act
            var deleteSql = DipMapper.GetSqlDelete<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM Activity;");        
        }

        [TestMethod]
        public void GetSqlDelete_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var deleteSql = DipMapper.GetSqlDelete<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM Activity WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetExtendedParameters_ParametersOnly()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 1);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosParametersSkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id"};

            var activity = new Activity()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Level = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 7);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, "@Level");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, "@IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, "@Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, "@Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, "@ActivityType");
            Assert.AreEqual(extendedParameters.ElementAt(6).Key, "@pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosSkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id" };

            var activity = new Activity()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Level = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 6);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, "@Level");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, "@IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, "@Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, "@Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, "@ActivityType");
        }

        [TestMethod]
        public void New_ActivatorCreateInstance()
        {
            // Arrange
            var newT = DipMapper.New<Activity>(false);

            // Act
            var activity = newT();
            activity.Name = "Test";

            // Assert
            Assert.AreEqual(activity.Name, "Test");
        }

        [TestMethod]
        public void New_DynamicMethod()
        {
            // Arrange
            var newT = DipMapper.New<Activity>(true);

            // Act
            var activity = newT();
            activity.Name = "Test";

            // Assert
            Assert.AreEqual(activity.Name, "Test");
        }

        [TestMethod]
        public void New_DynamicMethod_Cached()
        {
            // Arrange
            var newActivity1 = DipMapper.New<Activity>(true);
            var newGenericActivity1 = DipMapper.New<GenericActivity<int>>(true);
            var newActivity2 = DipMapper.New<Activity>(true);
            var newGenericActivity2 = DipMapper.New<GenericActivity<int>>(true);
            var newGenericActivity3 = DipMapper.New<GenericActivity<Activity>>(true);

            // Act
            var activity1 = newActivity1();
            activity1.Name = "Activity1";

            var genericActivity1 = newGenericActivity1();
            genericActivity1.Name = "GenericActivity1";

            var activity2 = newActivity2();
            activity2.Name = "Activity2";

            var genericActivity2 = newGenericActivity2();
            genericActivity2.Name = "GenericActivity2";

            var genericActivity3 = newGenericActivity3();
            genericActivity3.Name = "GenericActivity3";

            // Assert
            Assert.AreEqual(activity1.Name, "Activity1");
            Assert.AreEqual(activity2.Name, "Activity2");
            Assert.AreEqual(genericActivity1.Name, "GenericActivity1");
            Assert.AreEqual(genericActivity2.Name, "GenericActivity2");
            Assert.AreEqual(genericActivity3.Name, "GenericActivity3");
        }
    }
}
