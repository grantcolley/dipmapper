using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperMySqlTest
    {
        [TestMethod]
        public void GetConnType_MsSql()
        {
            // Arrange
            var conn = new MySqlConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType.ToString(), DipMapper.ConnType.MySql.ToString());
        }

        [TestMethod]
        public void DefaultDbHelper_AddDataParameter()
        {
            // Arrange
            var defaultDbHelper = new DipMapper.DefaultDbHelper();
            var command = new MySqlCommand();

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
        public void MsSqlHelper_GetParameterName()
        {
            // Arrange
            var msSqlHelper = new DipMapper.MySqlHelper();

            // Act
            var prefix = msSqlHelper.GetParameterName("Id");

            // Assert
            Assert.AreEqual(prefix, "?Id");
        }

        [TestMethod]
        public void MsSqlHelper_GetParameterName_IsWhereClauseTrue()
        {
            // Arrange
            var msSqlHelper = new DipMapper.MySqlHelper();

            // Act
            var prefix = msSqlHelper.GetParameterName("Id", true);

            // Assert
            Assert.AreEqual(prefix, "?pId");
        }

        [TestMethod]
        public void MsSqlHelper_GetParameterName_Prefix()
        {
            // Arrange
            var msSqlHelper = new DipMapper.MySqlHelper();

            // Act
            var prefix = msSqlHelper.GetParameterName("?Id");

            // Assert
            Assert.AreEqual(prefix, "?Id");
        }

        [TestMethod]
        public void MsSqlHelper_GetParameterName_Prefix_IsWhereClauseTrue()
        {
            // Arrange
            var msSqlHelper = new DipMapper.MySqlHelper();

            // Act
            var prefix = msSqlHelper.GetParameterName("?Id", true);

            // Assert
            Assert.AreEqual(prefix, "?pId");
        }

        [TestMethod]
        public void DefaultDbHelper_GetSqlSelectWithIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var msSqlHelper = new DipMapper.MySqlHelper();
            var insertSql = "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (?Name, ?Level, ?IsActive, ?Created, ?Updated, ?ActivityType)";

            // Act
            var selectWithIdentity = msSqlHelper.GetSqlSelectWithIdentity<Activity>(insertSql, propertyInfos, "Id");

            // Assert
            Assert.AreEqual(selectWithIdentity, insertSql + ";SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = LAST_INSERT_ID();");
        }

        public void GetGenericParameters_Identity_SqlDataParameter()
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
            var identity = new MySqlParameter() { ParameterName = "Id", Value = activity.Id };

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

        [TestMethod]
        public void GetSqlSelect_WithParameter()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Id", Value = 3 });

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=?pId");
        }

        [TestMethod]
        public void GetSqlWhereClause_MultipleParameters()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Id", Value = 3 });
            parameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            parameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            // Act
            var sqlSelect = DipMapper.GetSqlWhereClause(connType, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, " WHERE Id=?pId AND IsActive=?pIsActive AND ActivityType=?pActivityType");
        }

        [TestMethod]
        public void GetSqlDelete_WithParameter()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Id", Value = 3 });

            // Act
            var deleteSql = DipMapper.GetSqlDelete<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM Activity WHERE Id=?pId");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipIdentityFieldOnly_GetSql()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields<Activity>(connType, propertyInfos, "Id", null);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (?Name, ?Level, ?IsActive, ?Created, ?Updated, ?ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Name", Value = "Hello World" });
            parameters.Add(new MySqlParameter() { ParameterName = "Level", Value = 1 });
            parameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            parameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields<Activity>(connType, propertyInfos, "Id", parameters);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, ActivityType) VALUES (?Name, ?Level, ?IsActive, ?ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsert_NoAutoIdentity()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(connType, propertyInfos, null, null);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Id, Name, Level, IsActive, Created, Updated, ActivityType) VALUES (?Id, ?Name, ?Level, ?IsActive, ?Created, ?Updated, ?ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsert_WithIdentity()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(connType, propertyInfos, "Id", null);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (?Name, ?Level, ?IsActive, ?Created, ?Updated, ?ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = LAST_INSERT_ID();");
        }

        [TestMethod]
        public void GetSqlInsert_SkipFields()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Name", Value = "Hello World" });
            parameters.Add(new MySqlParameter() { ParameterName = "Level", Value = 1 });
            parameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            parameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(connType, propertyInfos, "Id", parameters);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, ActivityType) VALUES (?Name, ?Level, ?IsActive, ?ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = LAST_INSERT_ID();");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipIdentityOnUpdateOnly_GetSql()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameter = new MySqlParameter() { ParameterName = "Id", Value = "1" };

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, null, parameter);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=?Name, Level=?Level, IsActive=?IsActive, Created=?Created, Updated=?Updated, ActivityType=?ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Name", Value = "Hello World" });
            parameters.Add(new MySqlParameter() { ParameterName = "Level", Value = 1 });
            parameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            parameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, parameters, null);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=?Name, Level=?Level, IsActive=?IsActive, ActivityType=?ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdate_WithoutParameters()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, null, null, null);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Id=?Id, Name=?Name, Level=?Level, IsActive=?IsActive, Created=?Created, Updated=?Updated, ActivityType=?ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdate_WithoutIdentity()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var identity = new MySqlParameter() { ParameterName = "Id", Value = 1 };
            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, null, null, identity);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=?Name, Level=?Level, IsActive=?IsActive, Created=?Created, Updated=?Updated, ActivityType=?ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdate_WithUpdateParametersOnly()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var updateParameters = new List<MySqlParameter>();
            updateParameters.Add(new MySqlParameter() { ParameterName = "Name", Value = "Hello World" });
            updateParameters.Add(new MySqlParameter() { ParameterName = "Level", Value = 1 });
            updateParameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            updateParameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, updateParameters, null, null);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=?Name, Level=?Level, IsActive=?IsActive, ActivityType=?ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdate_WithWhereClauseParametersOnly()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var whereClauseParameters = new List<MySqlParameter>();
            whereClauseParameters.Add(new MySqlParameter() { ParameterName = "Id", Value = 5 });

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, null, whereClauseParameters, null);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Id=?Id, Name=?Name, Level=?Level, IsActive=?IsActive, Created=?Created, Updated=?Updated, ActivityType=?ActivityType WHERE Id=?pId");
        }

        [TestMethod]
        public void GetSqlUpdate_WithUpdateParametersAndWhereParameters()
        {
            // Arrange
            var conn = new MySqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();

            var parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter() { ParameterName = "Name", Value = "Hello World" });
            parameters.Add(new MySqlParameter() { ParameterName = "Level", Value = 1 });
            parameters.Add(new MySqlParameter() { ParameterName = "IsActive", Value = true });
            parameters.Add(new MySqlParameter() { ParameterName = "ActivityType", Value = 2 });

            var whereClauseParameters = new List<MySqlParameter>();
            whereClauseParameters.Add(new MySqlParameter() { ParameterName = "Id", Value = 5 });

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, whereClauseParameters, null);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=?Name, Level=?Level, IsActive=?IsActive, ActivityType=?ActivityType WHERE Id=?pId");
        }
    }
}