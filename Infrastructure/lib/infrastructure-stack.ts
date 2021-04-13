import * as sns from '@aws-cdk/aws-sns';
import * as subs from '@aws-cdk/aws-sns-subscriptions';
import * as lambda from '@aws-cdk/aws-lambda';
import * as s3 from '@aws-cdk/aws-s3';
import * as sqs from '@aws-cdk/aws-sqs';
import * as cdk from '@aws-cdk/core';
import * as dynamodb from '@aws-cdk/aws-dynamodb';
import * as lambdaSources from '@aws-cdk/aws-lambda-event-sources';
const path = require('path');

export class InfrastructureStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props);


    const bucket = new s3.Bucket(this, `lambdaProjectBucket`, {
      bucketName: `lambdaproject--xerris`,
      publicReadAccess: true
    });

    const s3Handler = new lambda.DockerImageFunction(this, 's3Handler',{
      functionName: 's3Handler',
      code: lambda.DockerImageCode.fromImageAsset(path.join(__dirname, '../../LambdaProject'), {
      cmd: [ "LambdaProject::LambdaProject.Function::S3FunctionHandler"]
      })      
    });

    const s3eventSource = s3Handler.addEventSource(new lambdaSources.S3EventSource(bucket, {
      events: [ s3.EventType.OBJECT_CREATED, s3.EventType.OBJECT_REMOVED ]
    }));

    const queue = new sqs.Queue(this, 'Queue');

    queue.grantSendMessages(s3Handler);

    const sqsHandler = new lambda.DockerImageFunction(this, 'sqsHandler',{
      functionName: 'sqsHandler',
      code: lambda.DockerImageCode.fromImageAsset(path.join(__dirname, '../../LambdaProject'), {
      cmd: [ "LambdaProject::LambdaProject.Function::SQSFunctionHandler"]
      })      
    });

    const eventSource = sqsHandler.addEventSource(new lambdaSources.SqsEventSource(queue));

    const table = new dynamodb.Table(this, 'landingTable', {
      partitionKey: { name: 'id', type: dynamodb.AttributeType.STRING }
    });

    table.grantReadWriteData(sqsHandler);
    queue.grantConsumeMessages(sqsHandler);
  }
}
