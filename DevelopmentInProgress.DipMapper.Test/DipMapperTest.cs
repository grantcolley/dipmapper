using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperTest
    {
        [TestMethod]
        public void DipMapper_GetSelectSql_ClassTest()
        {
            // Arrange

            // Act
            string fields = DipMapper.GetSelectSql<Activity>();

            // Assert
            Assert.AreEqual(fields, "SELECT Id, Name, Number, IsActive, ActivityType, Date, NullableDate FROM Activity");
        }

        [TestMethod]
        public void DipMapper_GetSelectSql_GenericClassReferenceTypeTest()
        {
            // Arrange

            // Act
            string fields = DipMapper.GetSelectSql<GenericActivity<Activity>>();

            // Assert
            Assert.AreEqual(fields, "SELECT Id, Name FROM Activity");
        }

        [TestMethod]
        public void DipMapper_GetSelectSql_GenericClassValueTypeTest()
        {
            // Arrange

            // Act
            string fields = DipMapper.GetSelectSql<GenericActivity<int>>();

            // Assert
            Assert.AreEqual(fields, "SELECT Id, Name, GenericProperty FROM Int32");
        }

        [TestMethod]
        public void DipMapper_GetWhereSql_NullParameters_Test()
        {
            // Arrange

            // Act
            string where = DipMapper.GetWhereSql(null);

            // Assert
            Assert.AreEqual(where, "");
        }

        [TestMethod]
        public void DipMapper_GetWhereSql_NoParameters_Test()
        {
            // Arrange

            // Act
            string where = DipMapper.GetWhereSql(new Dictionary<string, object>());

            // Assert
            Assert.AreEqual(where, "");
        }

        [TestMethod]
        public void DipMapper_GetWhereSql_HasGenericParameter_Test()
        {
            // Arrange
            var genericActivity = new GenericActivity<int>()
            {
                Id = 5,
                Name = "TestActivity",
                GenericProperty = 7
            };
            
            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", genericActivity.Id);
            parameters.Add("Name", genericActivity.Name);
            parameters.Add("GenericProperty", genericActivity.GenericProperty);

            // Act
            string where = DipMapper.GetWhereSql(parameters);

            // Assert
            Assert.AreEqual(where, "WHERE Id=5 AND Name='TestActivity' AND GenericProperty=7");
        }

        [TestMethod]
        public void DipMapper_GetWhereSql_HasParameters_Test()
        {
            // Arrange
            var activity = new Activity()
            {
                Id = 5,
                Name = "TestActivity",
                IsActive = true,
                ActivityType = ActivityTypeEnum.Private
            };

            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", activity.Id);
            parameters.Add("Name", activity.Name);
            parameters.Add("Date", activity.Date);
            parameters.Add("NullableDate", activity.NullableDate);
            parameters.Add("IsActive", activity.IsActive);
            parameters.Add("ActivityType", activity.ActivityType);

            // Act
            string where = DipMapper.GetWhereSql(parameters);

            // Assert
            Assert.AreEqual(where, "WHERE Id=5 AND Name='TestActivity' AND Date='01/01/0001 00:00:00' AND NullableDate is null AND IsActive=1 AND ActivityType=1");
        }
    }
}
