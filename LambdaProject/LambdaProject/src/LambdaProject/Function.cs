using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.SQSEvents;
using Amazon;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld
{
    public class Function
    {
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.CACentral1;
        private static IAmazonS3 client;
        private static AmazonSQSClient sqsClient;
        private static AmazonDynamoDBClient dynamoDbClient;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            
            var sqsConfig = new AmazonSQSConfig();
            var dynamoConfig = new AmazonDynamoDBConfig();
            dynamoConfig.RegionEndpoint = RegionEndpoint.CACentral1;
            sqsConfig.RegionEndpoint = RegionEndpoint.CACentral1;
            sqsConfig.ServiceURL = "https://sqs.ca-central-1.amazonaws.com";
            client = new AmazonS3Client(bucketRegion);
            sqsClient = new AmazonSQSClient(sqsConfig);
            dynamoDbClient = new AmazonDynamoDBClient(dynamoConfig);
        }

        
        

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SQSFunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }
        
        public async Task S3FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessS3MessageAsync(message, context);
            }
            
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");
            await WriteMessage(message.Body);
            await sqsClient.DeleteQueueAsync(message.ReceiptHandle);
            await Task.CompletedTask;
        }
        private async Task ProcessS3MessageAsync(S3Event.S3EventNotificationRecord message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.S3.Bucket.Name}");

            var csv = await ReadObjectDataAsync(message.S3.Bucket.Name, message.S3.Object.Key);
            context.Logger.LogLine($"read csv");
            var mappedCsv = StripCSV(csv);
            context.Logger.LogLine($"mapped message {csv}");
            foreach (var row in mappedCsv)
            {
                context.Logger.LogLine($"sending message {row}");
                await PushToQueue(row);
            }
            context.Logger.LogLine($"Finished{message.S3.Bucket.Name}");
            await Task.CompletedTask;
        }
        
        public async Task<String> ReadObjectDataAsync(String bucketName, String keyName)
        {
            string responseBody = "";
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                    string contentType = response.Headers["Content-Type"];
                    Console.WriteLine("Object metadata, Title: {0}", title);
                    Console.WriteLine("Content type: {0}", contentType);

                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                }

            }
            catch (AmazonS3Exception e)
            {
                // If bucket or object does not exist
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
            }
            return responseBody;
        }

        public IEnumerable<String> StripCSV(String stringCsv)
        {
            var csvRows = stringCsv.Split("\r\n");
            var csvRowsList = csvRows.OfType<string>().ToList();
            csvRowsList.RemoveAt(0);
            var returnItems = csvRowsList.Select(row =>
            {
                if (row == "")
                {
                    return "END-ROW";
                }
                var splitRow = row.Split(",");
                return splitRow[0] + "-" + splitRow[1];
            });
            
            return returnItems;
        }
        
        public async Task<SendMessageResponse> PushToQueue(String row)
        {
            var sendMessageRequest = new SendMessageRequest()
            {
                QueueUrl = "https://sqs.ca-central-1.amazonaws.com/370365354210/InfrastructureStack-Queue4A7E3555-1T8MO6ROKLJ7Y",
                MessageBody = row
            };
            return await sqsClient.SendMessageAsync(sendMessageRequest);
        }
        
        public async Task<DeleteMessageResponse> DeletemFromQueue(String row)
        {
            var sendMessageRequest = new DeleteMessageRequest()
            {
                QueueUrl = "https://sqs.ca-central-1.amazonaws.com/370365354210/InfrastructureStack-Queue4A7E3555-1T8MO6ROKLJ7Y",
                ReceiptHandle = row
            };
            return await sqsClient.DeleteMessageAsync(sendMessageRequest);
        }
        
        public async Task<PutItemResponse> WriteMessage(String message)
        {
            var splitMessage = message.Split("-");
            var request = new PutItemRequest
            {
                TableName = "InfrastructureStack-landingTableA236B929-NOTBFFWOA45Q",
                Item = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue {S = splitMessage[0]}},
                    {"color", new AttributeValue {S = splitMessage[1]}}
                }
            };
            return await dynamoDbClient.PutItemAsync(request);
        }
    }
}