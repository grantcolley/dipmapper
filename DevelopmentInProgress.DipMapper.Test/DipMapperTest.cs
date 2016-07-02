using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperTest
    {
        [TestMethod]
        public void GetFieldsTest()
        {
            // Arrange

            // Act
            string fields = DipMapper.GetSelectFields<TestDapperClass>();

            // Assert
            Assert.AreEqual(fields, "Id, Name, Date");
        }
    }
}
