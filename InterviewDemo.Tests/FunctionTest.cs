using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

namespace InterviewDemo.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestFunctionHandler()
    {
        // Arrange
        var function = new Function();
        var context = new TestLambdaContext();
        string address = "70 Vanderbilt Ave, New York, NY 10017, United States";

        Environment.SetEnvironmentVariable("GoogleApiKey", "AIzaSyDl0gzaalSU3iTxETsdeOhhmu_icZw1o3E");
        Environment.SetEnvironmentVariable("GoogleApiUrl", "https://maps.googleapis.com/maps/api/geocode/json?address=");
        Environment.SetEnvironmentVariable("DynamoDBTableName", "GeocodeCache");
        Environment.SetEnvironmentVariable("DynamoDBCacheDuration", "30");

        // Act
        var response = await function.FunctionHandler(address, context);

        // Assert
        Assert.NotNull(response);
    }
}