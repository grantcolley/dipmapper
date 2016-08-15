using System;
using System.Collections.Generic;
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
        public void xx()
        {
            // Arrange

            // Act

            // Assert
        }

    }
}
