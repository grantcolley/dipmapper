using System;
using System.Collections.Generic;
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
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity;");
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
        [ExpectedException(typeof(NotSupportedException))]
        public void GetSqlInsert_Oledb()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.OleDb, propertyInfos, "", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Id, Name, Level, IsActive, Created, Updated, ActivityType) VALUES (Id, Name, Level, IsActive, Created, Updated, ActivityType);");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetSqlInsert_Odbc()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.Odbc, propertyInfos, "", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Id, Name, Level, IsActive, Created, Updated, ActivityType) VALUES (Id, Name, Level, IsActive, Created, Updated, ActivityType);");
        }
    }
}
