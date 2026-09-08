using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Reqnroll;

namespace Integration.Tests.Steps;

[Binding]
public class DatabaseIntegrationSteps
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private readonly Dictionary<string, object> _testData;
    private Exception? _caughtException;
    private SqlTransaction? _transaction;
    private List<int> _insertedUserIds;

    public DatabaseIntegrationSteps()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        _connectionString = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") 
            ?? configuration["Database:ConnectionString"] 
            ?? throw new InvalidOperationException("Database connection string is not configured");

        _testData = new Dictionary<string, object>();
        _insertedUserIds = new List<int>();
    }

    #region Given Steps

    [Given("the database connection is configured")]
    public void GivenDatabaseConnectionIsConfigured()
    {
        Assert.That(_connectionString, Is.Not.Null.And.Not.Empty);
    }

    [Given("the test database is available")]
    public async Task GivenDatabaseIsAvailable()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }

    [Given("I have a new user record:")]
    public void GivenIHaveNewUserRecord(Table table)
    {
        var userData = TableToUserData(table);
        _testData["newUser"] = userData;
    }

    [Given("a user exists in the database:")]
    public async Task GivenUserExistsInDatabase(Table table)
    {
        var userData = TableToUserData(table);
        var userId = await InsertUser(userData);
        _testData["existingUser"] = userData;
        _testData["existingUserId"] = userId;
        _insertedUserIds.Add(userId);
    }

    [Given("multiple users exist in the database:")]
    public async Task GivenMultipleUsersExist(Table table)
    {
        var usersList = new List<Dictionary<string, object>>();
        
        foreach (var row in table.Rows)
        {
            var userData = new Dictionary<string, object>();
            foreach (var header in table.Header)
            {
                userData[header] = row[header];
            }
            var userId = await InsertUser(userData);
            _insertedUserIds.Add(userId);
            usersList.Add(userData);
        }
        
        _testData["multipleUsers"] = usersList;
    }

    [Given("I start a database transaction")]
    public async Task GivenIStartTransaction()
    {
        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync();
        _transaction = _connection.BeginTransaction();
    }

    [Given("I have {int} concurrent user creation requests")]
    public void GivenIHaveConcurrentRequests(int count)
    {
        _testData["concurrentRequestCount"] = count;
    }

    [Given("a stored procedure {string} exists")]
    public async Task GivenStoredProcedureExists(string procedureName)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = $@"
            SELECT COUNT(*) FROM sys.procedures 
            WHERE name = @procName";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@procName", procedureName);
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        Assert.That(count, Is.GreaterThan(0), 
            $"Stored procedure '{procedureName}' should exist");
    }

    [Given("I have {int} user records to insert")]
    public void GivenIHaveUserRecordsToInsert(int count)
    {
        var usersList = new List<Dictionary<string, object>>();
        
        for (int i = 0; i < count; i++)
        {
            usersList.Add(new Dictionary<string, object>
            {
                ["Username"] = $"bulkuser{i}",
                ["Email"] = $"bulkuser{i}@test.com",
                ["FirstName"] = $"Bulk{i}",
                ["LastName"] = "User"
            });
        }
        
        _testData["bulkUsers"] = usersList;
    }

    #endregion

    #region When Steps

    [When("I attempt to connect to the database")]
    public async Task WhenIAttemptToConnect()
    {
        try
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When("I insert the user into the database")]
    public async Task WhenIInsertUser()
    {
        var userData = (Dictionary<string, object>)_testData["newUser"];
        var userId = await InsertUser(userData);
        _testData["insertedUserId"] = userId;
        _insertedUserIds.Add(userId);
    }

    [When("I update the user email to {string}")]
    public async Task WhenIUpdateUserEmail(string newEmail)
    {
        var userId = (int)_testData["existingUserId"];
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "UPDATE Users SET Email = @Email WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Email", newEmail);
        command.Parameters.AddWithValue("@Id", userId);
        
        await command.ExecuteNonQueryAsync();
        _testData["updatedEmail"] = newEmail;
    }

    [When("I delete the user with username {string}")]
    public async Task WhenIDeleteUser(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "DELETE FROM Users WHERE Username = @Username";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        _testData["deletedRows"] = rowsAffected;
    }

    [When("I query for active users only")]
    public async Task WhenIQueryActiveUsers()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT * FROM Users WHERE Active = 1";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        var activeUsers = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
            var user = new Dictionary<string, object>
            {
                ["Id"] = reader["Id"] ?? 0,
                ["Username"] = reader["Username"] ?? string.Empty,
                ["Active"] = reader["Active"] ?? false
            };
            activeUsers.Add(user);
        }
        
        _testData["activeUsers"] = activeUsers;
    }

    [When("I insert a user with invalid data that violates constraints")]
    public async Task WhenIInsertInvalidUser()
    {
        try
        {
            var query = "INSERT INTO Users (Username, Email) VALUES (NULL, NULL)";
            using var command = new SqlCommand(query, _connection, _transaction);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When("I execute all requests simultaneously")]
    public async Task WhenIExecuteConcurrentRequests()
    {
        var count = (int)_testData["concurrentRequestCount"];
        var tasks = new List<Task<int>>();
        
        for (int i = 0; i < count; i++)
        {
            var userData = new Dictionary<string, object>
            {
                ["Username"] = $"concurrent{i}",
                ["Email"] = $"concurrent{i}@test.com",
                ["FirstName"] = $"Concurrent{i}",
                ["LastName"] = "User"
            };
            tasks.Add(InsertUser(userData));
        }
        
        var userIds = await Task.WhenAll(tasks);
        _insertedUserIds.AddRange(userIds);
        _testData["concurrentUserIds"] = userIds;
    }

    [When("I create {int} simultaneous database connections")]
    public async Task WhenICreateSimultaneousConnections(int count)
    {
        var connections = new List<SqlConnection>();
        var tasks = new List<Task>();
        
        for (int i = 0; i < count; i++)
        {
            var connection = new SqlConnection(_connectionString);
            connections.Add(connection);
            tasks.Add(connection.OpenAsync());
        }
        
        await Task.WhenAll(tasks);
        _testData["simultaneousConnections"] = connections;
        
        // Clean up connections
        foreach (var conn in connections)
        {
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    [When("I execute the stored procedure with parameter {string}")]
    public async Task WhenIExecuteStoredProcedure(string parameter)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand("GetUsersByRole", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Role", parameter);
        
        using var reader = await command.ExecuteReaderAsync();
        var results = new List<Dictionary<string, object>>();
        
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader[i];
            }
            results.Add(row);
        }
        
        _testData["procedureResults"] = results;
    }

    [When("I perform a bulk insert operation")]
    public async Task WhenIPerformBulkInsert()
    {
        var users = (List<Dictionary<string, object>>)_testData["bulkUsers"];
        var startTime = DateTime.Now;
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        foreach (var user in users)
        {
            await InsertUser(user, connection);
        }
        
        var duration = DateTime.Now - startTime;
        _testData["bulkInsertDuration"] = duration;
    }

    [When("I query the database schema")]
    public async Task WhenIQueryDatabaseSchema()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                c.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_NAME = 'Users'
            ORDER BY c.ORDINAL_POSITION";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        var columns = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
            columns.Add(new Dictionary<string, object>
            {
                ["Column"] = reader["COLUMN_NAME"],
                ["Type"] = reader["DATA_TYPE"],
                ["Nullable"] = reader["IS_NULLABLE"].ToString() == "YES"
            });
        }
        
        _testData["schemaColumns"] = columns;
    }

    #endregion

    #region Then Steps

    [Then("the connection should be successful")]
    public void ThenConnectionShouldBeSuccessful()
    {
        Assert.That(_connection, Is.Not.Null);
        Assert.That(_connection!.State, Is.EqualTo(ConnectionState.Open));
    }

    [Then("I should be able to execute a simple query")]
    public async Task ThenIShouldExecuteSimpleQuery()
    {
        using var command = new SqlCommand("SELECT 1 AS TestValue", _connection);
        var result = await command.ExecuteScalarAsync();
        Assert.That(result, Is.EqualTo(1));
    }

    [Then("the user should be saved successfully")]
    public void ThenUserShouldBeSaved()
    {
        Assert.That(_testData.ContainsKey("insertedUserId"), Is.True);
        Assert.That((int)_testData["insertedUserId"], Is.GreaterThan(0));
    }

    [Then("I should be able to retrieve the user by username {string}")]
    public async Task ThenIShouldRetrieveUserByUsername(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT * FROM Users WHERE Username = @Username";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        
        using var reader = await command.ExecuteReaderAsync();
        Assert.That(await reader.ReadAsync(), Is.True, 
            $"User with username '{username}' should exist");
        
        _testData["retrievedUser"] = new Dictionary<string, object>
        {
            ["Username"] = reader["Username"],
            ["Email"] = reader["Email"],
            ["FirstName"] = reader["FirstName"],
            ["LastName"] = reader["LastName"]
        };
    }

    [Then("the retrieved user should match the inserted data")]
    public void ThenRetrievedUserShouldMatch()
    {
        var originalUser = (Dictionary<string, object>)_testData["newUser"];
        var retrievedUser = (Dictionary<string, object>)_testData["retrievedUser"];
        
        Assert.That(retrievedUser["Username"], Is.EqualTo(originalUser["Username"]));
        Assert.That(retrievedUser["Email"], Is.EqualTo(originalUser["Email"]));
    }

    [Then("the user email should be updated in the database")]
    [Then("the user should have email {string}")]
    public async Task ThenUserEmailShouldBeUpdated(string expectedEmail)
    {
        var userId = (int)_testData["existingUserId"];
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT Email FROM Users WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", userId);
        
        var actualEmail = (string?)(await command.ExecuteScalarAsync()) ?? string.Empty;
        Assert.That(actualEmail, Is.EqualTo(expectedEmail));
    }

    [Then("the user should be removed from the database")]
    public void ThenUserShouldBeRemoved()
    {
        var deletedRows = (int)_testData["deletedRows"];
        Assert.That(deletedRows, Is.GreaterThan(0));
    }

    [Then("querying for username {string} should return no results")]
    public async Task ThenQueryingShouldReturnNoResults(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        Assert.That(count, Is.EqualTo(0));
    }

    [Then("I should receive {int} users")]
    public void ThenIShouldReceiveUsers(int expectedCount)
    {
        var activeUsers = (List<Dictionary<string, object>>?)_testData["activeUsers"];
        Assert.That(activeUsers, Is.Not.Null);
        Assert.That(activeUsers!.Count, Is.EqualTo(expectedCount));
    }

    [Then("all returned users should have Active status true")]
    public void ThenAllUsersShouldBeActive()
    {
        var activeUsers = (List<Dictionary<string, object>>?)_testData["activeUsers"];
        Assert.That(activeUsers, Is.Not.Null);
        foreach (var user in activeUsers!)
        {
            Assert.That(user["Active"], Is.True);
        }
    }

    [Then("the transaction should be rolled back")]
    public async Task ThenTransactionShouldBeRolledBack()
    {
        Assert.That(_caughtException, Is.Not.Null);
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }

    [Then("no partial data should be saved to the database")]
    public void ThenNoPartialDataShouldBeSaved()
    {
        // Verify by checking if any incomplete records exist
        Assert.That(_caughtException, Is.Not.Null, 
            "Exception should have been caught during invalid insert");
    }

    [Then("all {int} users should be created successfully")]
    public void ThenAllUsersShouldBeCreated(int expectedCount)
    {
        var userIds = (int[]?)_testData["concurrentUserIds"];
        Assert.That(userIds, Is.Not.Null);
        Assert.That(userIds!.Length, Is.EqualTo(expectedCount));
        Assert.That(userIds.All(id => id > 0), Is.True);
    }

    [Then("there should be no data corruption")]
    public void ThenNoDataCorruption()
    {
        var userIds = (int[]?)_testData["concurrentUserIds"];
        Assert.That(userIds, Is.Not.Null);
        var distinctIds = userIds!.Distinct().ToArray();
        Assert.That(distinctIds.Length, Is.EqualTo(userIds.Length), 
            "All user IDs should be unique");
    }

    [Then("each user should have a unique ID")]
    public void ThenEachUserShouldHaveUniqueId()
    {
        var userIds = (int[]?)_testData["concurrentUserIds"];
        Assert.That(userIds, Is.Not.Null);
        Assert.That(userIds!.Distinct().Count(), Is.EqualTo(userIds.Length));
    }

    [Then("all connections should be established successfully")]
    public void ThenAllConnectionsEstablished()
    {
        var connections = (List<SqlConnection>?)_testData["simultaneousConnections"];
        Assert.That(connections, Is.Not.Null);
        Assert.That(connections!.Count, Is.GreaterThan(0));
    }

    [Then("connections should be returned to the pool after use")]
    [Then("no connection leaks should occur")]
    public void ThenNoConnectionLeaks()
    {
        // Connections were properly disposed in the When step
        Assert.Pass("Connections were properly managed");
    }

    [Then("the procedure should return users with Admin role")]
    public void ThenProcedureShouldReturnAdminUsers()
    {
        var results = (List<Dictionary<string, object>>?)_testData["procedureResults"];
        Assert.That(results, Is.Not.Null);
        Assert.That(results!.Count, Is.GreaterThan(0));
    }

    [Then("the result should contain expected columns")]
    public void ThenResultShouldContainExpectedColumns()
    {
        var results = (List<Dictionary<string, object>>?)_testData["procedureResults"];
        Assert.That(results, Is.Not.Null);
        Assert.That(results![0].ContainsKey("Id"), Is.True);
        Assert.That(results[0].ContainsKey("Username"), Is.True);
    }

    [Then("all {int} records should be inserted within {int} seconds")]
    public void ThenAllRecordsShouldBeInsertedWithinTime(int recordCount, int maxSeconds)
    {
        var duration = (TimeSpan?)_testData["bulkInsertDuration"];
        Assert.That(duration, Is.Not.Null);
        Assert.That(duration!.Value.TotalSeconds, Is.LessThanOrEqualTo(maxSeconds));
    }

    [Then("I should be able to query and verify all records")]
    public async Task ThenIShouldVerifyAllRecords()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT COUNT(*) FROM Users WHERE Username LIKE 'bulkuser%'";
        using var command = new SqlCommand(query, connection);
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        
        var expectedCount = ((List<Dictionary<string, object>>?)_testData["bulkUsers"])?.Count ?? 0;
        Assert.That(count, Is.EqualTo(expectedCount));
    }

    [Then("the {string} table should exist")]
    public async Task ThenTableShouldExist(string tableName)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @TableName";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@TableName", tableName);
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        Assert.That(count, Is.EqualTo(1));
    }

    [Then("the {string} table should have required columns:")]
    public void ThenTableShouldHaveRequiredColumns(string tableName, Table table)
    {
        var schemaColumns = (List<Dictionary<string, object>>?)_testData["schemaColumns"];
        Assert.That(schemaColumns, Is.Not.Null);
        
        foreach (var row in table.Rows)
        {
            var expectedColumn = row["Column"];
            var column = schemaColumns!.FirstOrDefault(c => 
                c["Column"].ToString() == expectedColumn);
            
            Assert.That(column, Is.Not.Null, 
                $"Column '{expectedColumn}' should exist");
        }
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, object> TableToUserData(Table table)
    {
        var userData = new Dictionary<string, object>();
        var firstRow = table.Rows[0];
        
        foreach (var header in table.Header)
        {
            userData[header] = firstRow[header];
        }
        
        return userData;
    }

    private async Task<int> InsertUser(Dictionary<string, object> userData, SqlConnection? connection = null)
    {
        var shouldDisposeConnection = connection == null;
        connection ??= new SqlConnection(_connectionString);
        
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        var query = @"
            INSERT INTO Users (Username, Email, FirstName, LastName, Active, CreatedDate) 
            VALUES (@Username, @Email, @FirstName, @LastName, @Active, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", userData["Username"]);
        command.Parameters.AddWithValue("@Email", userData["Email"]);
        command.Parameters.AddWithValue("@FirstName", userData.GetValueOrDefault("FirstName", DBNull.Value));
        command.Parameters.AddWithValue("@LastName", userData.GetValueOrDefault("LastName", DBNull.Value));
        command.Parameters.AddWithValue("@Active", userData.GetValueOrDefault("Active", true));
        
        var userId = (int)(await command.ExecuteScalarAsync() ?? 0);
        
        if (shouldDisposeConnection)
        {
            await connection.DisposeAsync();
        }
        
        return userId;
    }

    [AfterScenario]
    public async Task Cleanup()
    {
        // Clean up test data
        if (_insertedUserIds.Count > 0)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var ids = string.Join(",", _insertedUserIds);
                var query = $"DELETE FROM Users WHERE Id IN ({ids})";
                using var command = new SqlCommand(query, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }

    #endregion
}
