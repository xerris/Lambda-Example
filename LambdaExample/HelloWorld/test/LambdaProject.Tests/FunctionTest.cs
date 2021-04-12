using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Amazon.S3.Util;
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
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
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
            notificationRecord.S3.Bucket.Name = "gary";
            notificationRecord.S3.Object.Key = "test.csv";
            s3Event.Records.Add(notificationRecord);

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.S3FunctionHandler(s3Event, context);

            Assert.Contains("Processed message gary", logger.Buffer.ToString());
        }
    }
}