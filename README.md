# Lambda-Example

Pre-requisites:
  - Install and configure AWS CLI: https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-install.html
  - Install and configure AWS CDK: https://docs.aws.amazon.com/cdk/latest/guide/cli.html

Deploy Lambdas:
  - Make sure you are in the "Infrastructure" Directory
  - run: cdk deploy
  - After Infra Update:
    - https://github.com/xerris/Lambda-Example/blob/main/LambdaProject/LambdaProject/src/LambdaProject/Function.cs
    - Function.cs Line 158 168: Update with SQS Queue URL. Found in console here:
    - <img width="1743" alt="Screen Shot 2021-04-13 at 10 05 56 AM" src="https://user-images.githubusercontent.com/2738455/114584372-d8da7400-9c3f-11eb-83cc-d9c51518b257.png">

    - Function.cs Line 179: Update with dynamoDB table name. Found in console here:
    - <img width="1613" alt="Screen Shot 2021-04-13 at 10 06 24 AM" src="https://user-images.githubusercontent.com/2738455/114584441-e8f25380-9c3f-11eb-8ba5-228e828bea83.png">
    - run: "cdk deploy" again to redeploy your lambda with the updated strings
  - Take test.csv and upload to your s3 bucket and let it populate your dynamoDb table
