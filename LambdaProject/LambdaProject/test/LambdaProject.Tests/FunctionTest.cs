using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Amazon.S3.Util;
using Amazon.SQS.Model;
using HelloWorld;

namespace HelloWorld.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "3-MAROON"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.SQSFunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message 3-MAROON", logger.Buffer.ToString());
        }
        
        [Fact]
        public async Task Tests3EventLambdaFunction()
        {
            var s3Event = new S3Event()
            {
                Records = new List<S3Event.S3EventNotificationRecord>()
                
            };
            var notificationRecord = new S3EventNotification.S3EventNotificationRecord();
            notificationRecord.S3 = new S3EventNotification.S3Entity();
            notificationRecord.S3.Bucket = new S3EventNotification.S3BucketEntity();
            notificationRecord.S3.Object = new S3EventNotification.S3ObjectEntity();
            notificationRecord.S3.Bucket.Name = "lambdaproject--xerris";
            notificationRecord.S3.Object.Key = "test.csv";
            s3Event.Records.Add(notificationRecord);

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.S3FunctionHandler(s3Event, context);

            Assert.Contains("Processed message lambdaproject--xerris", logger.Buffer.ToString());
        }

        [Fact]
        public async Task TestReadObjectDataAsync()
        {
            var function = new Function();
            var output = await function.ReadObjectDataAsync("lambdaproject--xerris", "test.csv");
            Assert.Equal(output, "ID,COLOR,,,,,\r\n1,BLUE,,,,,\r\n2,RED,,,,,\r\n3,GREEN,,,,,\r\n4,RED,,,,,\r\n");
        }
        
        [Fact]
        public async Task TestStripCSV()
        {
            var function = new Function();
            var output = function.StripCSV("ID,COLOR,,,,,\r\n1,BLUE,,,,,\r\n2,RED,,,,,\r\n3,GREEN,,,,,\r\n4,RED,,,,,\r\n");
            List<String> lst = new List<String>();
            lst.Add("1-BLUE");
            lst.Add("2-RED");
            lst.Add("3-GREEN");
            lst.Add("4-RED");
            lst.Add("END-ROW");
            Assert.Equal(output, lst);
        }
        
        [Fact]
        public async Task TestPushToQueue()
        {
            var function = new Function();
            var output = await function.PushToQueue("1-BLUE");
            Assert.Equal(output.HttpStatusCode, HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task TestWriteMessage()
        {
            var function = new Function();
            var output = await function.WriteMessage("1-BLUE");
            Assert.Equal(output.HttpStatusCode, HttpStatusCode.OK);
        }
    }
}