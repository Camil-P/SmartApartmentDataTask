using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace InterviewDemo;
public class DynamoDBCache
{
    private readonly IAmazonDynamoDB _dynamoDBClient;

    public DynamoDBCache(IAmazonDynamoDB dynamoDBClient)
    {
        _dynamoDBClient = dynamoDBClient;
    }

    public async Task<string?> GetCachedResponse(string address, string tableName)
    {
        var request = new GetItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>
        {
            {"Address", new AttributeValue { S = address }}
        }
        };

        var response = await _dynamoDBClient.GetItemAsync(request);

        if (responseNotValid(response))
        {
            return null;
        }

        return response.Item["Response"].S;
    }

    public async Task StoreResponseInCache(string tableName, string address, string response, TimeSpan cacheDuration)
    {
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                {"Address", new AttributeValue { S = address }},
                {"Response", new AttributeValue { S = response }},
                {"ExpirationTimestamp", new AttributeValue { S = DateTime.Now.Add(cacheDuration).ToString("o") }}
            }
        };

        await _dynamoDBClient.PutItemAsync(request);
    }

    private bool responseNotValid(GetItemResponse response) 
        => response.Item is null 
        || !response.Item.ContainsKey("Response") 
        || !response.Item.ContainsKey("ExpirationTimestamp") 
        || DateTime.Parse(response.Item["ExpirationTimestamp"].S) < DateTime.Now;
}