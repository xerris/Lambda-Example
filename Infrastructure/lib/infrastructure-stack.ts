import * as sns from '@aws-cdk/aws-sns';
import * as subs from '@aws-cdk/aws-sns-subscriptions';
import * as lambda from '@aws-cdk/aws-lambda';
import * as s3 from '@aws-cdk/aws-s3';
import * as ecr from '@aws-cdk/aws-ecr';
import * as cdk from '@aws-cdk/core';
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
  }
}
