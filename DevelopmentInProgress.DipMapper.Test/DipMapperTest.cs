using System;
using System.Collections.Generic;
using System.Data;
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
        public void GetConnType_Odbc()
        {
            // Arrange
            var conn = new OdbcConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType.ToString(), DipMapper.ConnType.Odbc.ToString());
        }

        [TestMethod]
        public void GetConnType_Oledb()
        {
            // Arrange
            var conn = new OleDbConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType.ToString(), DipMapper.ConnType.OleDb.ToString());
        }

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
        public void GetSqlSelect_NoParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos, null);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity");
        }

        [TestMethod]
        public void GetSqlDelete_NoParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new List<IDbDataParameter>();

            // Act
            var deleteSql = DipMapper.GetSqlDelete<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM Activity");
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

        [TestMethod]
        public void DefaultDbHelper_AddDataParameter()
        {
            // Arrange
            var defaultDbHelper = new DipMapper.DefaultDbHelper();
            var command = new OleDbCommand();

            // Act
            defaultDbHelper.AddDataParameter(command, "Id", 3);
            defaultDbHelper.AddDataParameter(command, "Description", "Hello World");
            defaultDbHelper.AddDataParameter(command, "NullCheck", null);

            // Assert
            Assert.AreEqual(command.Parameters.Count, 3);
            Assert.AreEqual(command.Parameters[0].ParameterName, "Id");
            Assert.AreEqual(command.Parameters[0].Value, 3);
            Assert.AreEqual(command.Parameters[1].ParameterName, "Description");
            Assert.AreEqual(command.Parameters[1].Value, "Hello World");
            Assert.AreEqual(command.Parameters[2].ParameterName, "NullCheck");
            Assert.AreEqual(command.Parameters[2].Value, DBNull.Value);
        }

        [TestMethod]
        public void DefaultDbHelper_GetParameterPrefix()
        {
            // Arrange
            var defaultDbHelper = new DipMapper.DefaultDbHelper();

            // Act
            var prefix = defaultDbHelper.GetParameterPrefix();

            // Assert
            Assert.AreEqual(prefix, "");
        }

        [TestMethod]
        public void DefaultDbHelper_GetParameterPrefix_IsWhereClauseTrue()
        {
            // Arrange
            var defaultDbHelper = new DipMapper.DefaultDbHelper();

            // Act
            var prefix = defaultDbHelper.GetParameterPrefix(true);

            // Assert
            Assert.AreEqual(prefix, "p");
        }

        [TestMethod]
        public void DefaultDbHelper_GetSqlSelectWithIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var defaultDbHelper = new DipMapper.DefaultDbHelper();
            var insertSql = "INSERT INTO Activity (Name) VALUES (@Name)";

            // Act
            var selectWithIdentity = defaultDbHelper.GetSqlSelectWithIdentity<Activity>(insertSql, propertyInfos, "Id");

            // Assert
            Assert.AreEqual(insertSql, selectWithIdentity);
        }

        public void GetGenericParameters_No_Identity()
        {
            // Arrange
            string identity = "Id";
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

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
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, propertyInfos, identity, null);

            // Assert
            Assert.AreEqual(genericParameters.Count(), 7);
            Assert.AreEqual(genericParameters.ElementAt(0).Key, "Id");
            Assert.AreEqual(genericParameters.ElementAt(1).Key, "Name");
            Assert.AreEqual(genericParameters.ElementAt(2).Key, "Level");
            Assert.AreEqual(genericParameters.ElementAt(3).Key, "IsActive");
            Assert.AreEqual(genericParameters.ElementAt(4).Key, "Created");
            Assert.AreEqual(genericParameters.ElementAt(5).Key, "Updated");
            Assert.AreEqual(genericParameters.ElementAt(6).Key, "ActivityType");

            Assert.AreEqual(genericParameters.Count(), 7);
            Assert.AreEqual(genericParameters.ElementAt(0).Value, 3);
            Assert.AreEqual(genericParameters.ElementAt(1).Value, "Activity1");
            Assert.AreEqual(genericParameters.ElementAt(2).Value, 4);
            Assert.AreEqual(genericParameters.ElementAt(3).Value, true);
            Assert.AreEqual(genericParameters.ElementAt(4).Value, DateTime.Today);
            Assert.AreEqual(genericParameters.ElementAt(5).Value, null);
            Assert.AreEqual(genericParameters.ElementAt(6).Value, ActivityTypeEnum.Shared);
        }

        public void GetGenericParameters_Identity_String()
        {
            // Arrange
            string identity = "Id";
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            
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
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, propertyInfos, identity, null);

            // Assert
            Assert.AreEqual(genericParameters.Count(), 6);
            Assert.AreEqual(genericParameters.ElementAt(0).Key, "Name");
            Assert.AreEqual(genericParameters.ElementAt(1).Key, "Level");
            Assert.AreEqual(genericParameters.ElementAt(2).Key, "IsActive");
            Assert.AreEqual(genericParameters.ElementAt(3).Key, "Created");
            Assert.AreEqual(genericParameters.ElementAt(4).Key, "Updated");
            Assert.AreEqual(genericParameters.ElementAt(5).Key, "ActivityType");

            Assert.AreEqual(genericParameters.Count(), 6);
            Assert.AreEqual(genericParameters.ElementAt(0).Value, "Activity1");
            Assert.AreEqual(genericParameters.ElementAt(1).Value, 4);
            Assert.AreEqual(genericParameters.ElementAt(2).Value, true);
            Assert.AreEqual(genericParameters.ElementAt(3).Value, DateTime.Today);
            Assert.AreEqual(genericParameters.ElementAt(4).Value, null);
            Assert.AreEqual(genericParameters.ElementAt(5).Value, ActivityTypeEnum.Shared);
        }

        public void GetGenericParameters_Identity_IDbDataParameter()
        {
            // Arrange
            var activity = new Activity()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Level = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var identity = new OleDbParameter() { ParameterName = "Id", Value = activity.Id };

            // Act
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, propertyInfos, null, identity);

            // Assert
            Assert.AreEqual(genericParameters.Count(), 6);
            Assert.AreEqual(genericParameters.ElementAt(0).Key, "Name");
            Assert.AreEqual(genericParameters.ElementAt(1).Key, "Level");
            Assert.AreEqual(genericParameters.ElementAt(2).Key, "IsActive");
            Assert.AreEqual(genericParameters.ElementAt(3).Key, "Created");
            Assert.AreEqual(genericParameters.ElementAt(4).Key, "Updated");
            Assert.AreEqual(genericParameters.ElementAt(5).Key, "ActivityType");

            Assert.AreEqual(genericParameters.Count(), 6);
            Assert.AreEqual(genericParameters.ElementAt(0).Value, "Activity1");
            Assert.AreEqual(genericParameters.ElementAt(1).Value, 4);
            Assert.AreEqual(genericParameters.ElementAt(2).Value, true);
            Assert.AreEqual(genericParameters.ElementAt(3).Value, DateTime.Today);
            Assert.AreEqual(genericParameters.ElementAt(4).Value, null);
            Assert.AreEqual(genericParameters.ElementAt(5).Value, ActivityTypeEnum.Shared);
        }
    }
}
