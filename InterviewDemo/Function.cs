using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InterviewDemo;

public class Function
{
    private readonly HttpClient _httpClient;
    private readonly DynamoDBCache _cache;

    public Function()
    {
        _httpClient = new HttpClient();
        _cache = new DynamoDBCache(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and returns both the upper and lower case version of the string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(string address, ILambdaContext context)
    {
        string? apiKey = Environment.GetEnvironmentVariable("GoogleApiKey");
        string? apiUrl = Environment.GetEnvironmentVariable("GoogleApiUrl");
        string? tableName = Environment.GetEnvironmentVariable("DynamoDBTableName");

        int cacheDuration = Int32.Parse(Environment.GetEnvironmentVariable("DynamoDBCacheDuration") ?? "30");

        if (!areEnvVariablesSet(apiKey, apiUrl, tableName))
        {
            throw new Exception("Required environment variables are not set.");
        }

        var cachedResponse = await _cache.GetCachedResponse(address, tableName);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            return cachedResponse;
        }

        var requestUrl = $"{apiUrl}{Uri.EscapeDataString(address)}&key={apiKey}";

        var googleResponse = await FetchDataFromGoogle(requestUrl);

        if (!string.IsNullOrEmpty(googleResponse))
        {
            await _cache.StoreResponseInCache(tableName,
                                              address,
                                              googleResponse,
                                              TimeSpan.FromDays(cacheDuration));
        }

        return googleResponse;
    }

    private async Task<string> FetchDataFromGoogle(string apiUrl) 
        => await _httpClient.GetStringAsync(apiUrl);

    private bool areEnvVariablesSet(string? apiKey,
                                    string? apiUrl,
                                    string? tableName)
        => !string.IsNullOrEmpty(apiKey)
        && !string.IsNullOrEmpty(apiUrl)
        && !string.IsNullOrEmpty(tableName);
}