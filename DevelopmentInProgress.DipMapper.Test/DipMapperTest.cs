using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperTest
    {
        [TestMethod]
        public void DipMapper_GetSelectSql_Test()
        {
            // Arrange

            // Act
            string fields = DipMapper.GetSelectSql<Activity>();

            // Assert
            Assert.AreEqual(fields, "SELECT Id, Name, Number, Date, NullableDate FROM Activity");
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
        public void DipMapper_GetWhereSql_HasParameters_Test()
        {
            // Arrange
            var activity = new Activity() {Id = 5, Name = "TestActivity"};
            
            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", activity.Id);
            parameters.Add("Name", activity.Name);
            parameters.Add("Date", activity.Date);
            parameters.Add("NullableDate", activity.NullableDate);

            // Act
            string where = DipMapper.GetWhereSql(parameters);

            // Assert
            Assert.AreEqual(where, "WHERE Id=5 AND Name='TestActivity' AND Date='01/01/0001 00:00:00' AND NullableDate=null");
        }
    }
}
