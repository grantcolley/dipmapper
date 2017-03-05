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
            var genericActivity = DynamicTypeHelper.Get<GenericActivity<Int32>>();

            // Act
            var propertyInfos = genericActivity.SupportedProperties;

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
            var genericActivity = DynamicTypeHelper.Get<GenericActivity<Activity>>();

            // Act
            var propertyInfos = genericActivity.SupportedProperties;

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
            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();

            // Act
            var selectFields = DipMapper.GetSqlSelectFields(dynamicTypeHelper);

            // Assert
            Assert.AreEqual(selectFields, "Id, Name, Level, IsActive, Created, Updated, ActivityType");
        }

        [TestMethod]
        public void GetSqlSelectFields_GenericClass_IncludeGenericProperty()
        {
            // Arrange
            var dynamicTypeHelper = DynamicTypeHelper.Get<GenericActivity<Int32>>();

            // Act
            var selectFields = DipMapper.GetSqlSelectFields(dynamicTypeHelper);

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
            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, dynamicTypeHelper, null);

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
        public void TypeHelper_CreateInstance()
        {
            // Arrange
            var activityHelper = DynamicTypeHelper.Get<Activity>();

            // Act
            var activity = activityHelper.CreateInstance();
            activityHelper.SetValue(activity, "Name", "Test");
            var name = activityHelper.GetValue(activity, "Name");

            // Assert
            Assert.AreEqual(activity.Name, "Test");
            Assert.AreEqual(activity.Name, name);
        }

        [TestMethod]
        public void TypeHelper_Cached()
        {
            // Arrange
            var activityHelper = DynamicTypeHelper.Get<Activity>();
            var genericActivityHelper = DynamicTypeHelper.Get<GenericActivity<int>>();

            var activityHelper2 = DynamicTypeHelper.Get<Activity>();
            var genericActivityHelper2 = DynamicTypeHelper.Get<GenericActivity<int>>();

            var activity1 = activityHelper.CreateInstance();
            var genericActivity1 = genericActivityHelper.CreateInstance();
            var activity2 = activityHelper.CreateInstance();
            var genericActivity2 = genericActivityHelper.CreateInstance();
            var genericActivity3 = genericActivityHelper.CreateInstance();

            // Act
            activityHelper.SetValue(activity1, "Name", "Activity1");

            genericActivityHelper.SetValue(genericActivity1, "Name", "GenericActivity1");

            activityHelper2.SetValue(activity2, "Name", "Activity2");

            genericActivityHelper2.SetValue(genericActivity2, "Name", "GenericActivity2");

            genericActivity3.Name = "GenericActivity3";

            // Assert
            Assert.IsTrue(DynamicTypeHelper.cache.ContainsKey(typeof(Activity)));
            Assert.IsTrue(DynamicTypeHelper.cache.ContainsKey(typeof(GenericActivity<int>)));
            Assert.AreSame(activityHelper, activityHelper2);
            Assert.AreEqual(genericActivityHelper, genericActivityHelper2);
            Assert.AreEqual(activity1.Name, "Activity1");
            Assert.AreEqual(activity2.Name, "Activity2");
            Assert.AreEqual(genericActivity1.Name, "GenericActivity1");
            Assert.AreEqual(genericActivity2.Name, "GenericActivity2");
            Assert.AreEqual(genericActivity3.Name, "GenericActivity3");

            // TODO: Uncomment when running test in isolation. 
            // TODO: Comment out when running alongside other tests.
            //Assert.AreEqual(DynamicTypeHelper.cache.Count, 2);
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
            var prefix = defaultDbHelper.GetParameterName("Id");

            // Assert
            Assert.AreEqual(prefix, "Id");
        }

        [TestMethod]
        public void DefaultDbHelper_GetParameterPrefix_IsWhereClauseTrue()
        {
            // Arrange
            var defaultDbHelper = new DipMapper.DefaultDbHelper();

            // Act
            var prefix = defaultDbHelper.GetParameterName("Id", true);

            // Assert
            Assert.AreEqual(prefix, "pId");
        }

        [TestMethod]
        public void DefaultDbHelper_GetSqlSelectWithIdentity()
        {
            // Arrange
            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();
            var defaultDbHelper = new DipMapper.DefaultDbHelper();
            var insertSql = "INSERT INTO Activity (Name) VALUES (@Name)";

            // Act
            var selectWithIdentity = defaultDbHelper.GetSqlSelectWithIdentity<Activity>(insertSql, dynamicTypeHelper, "Id");

            // Assert
            Assert.AreEqual(insertSql, selectWithIdentity);
        }

        public void GetGenericParameters_No_Identity()
        {
            // Arrange
            string identity = "Id";
            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();

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
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, dynamicTypeHelper, identity, null);

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
            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();
            
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
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, dynamicTypeHelper, identity, null);

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

            var dynamicTypeHelper = DynamicTypeHelper.Get<Activity>();
            var identity = new OleDbParameter() { ParameterName = "Id", Value = activity.Id };

            // Act
            var genericParameters = DipMapper.GetGenericParameters<Activity>(activity, dynamicTypeHelper, null, identity);

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
