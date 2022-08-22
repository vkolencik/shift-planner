import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { Code, Function, Runtime } from 'aws-cdk-lib/aws-lambda'

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    new Function(this, 'backend-lambda-function', {
      code: Code.fromAsset('../backend/src/ShiftPlanner.Api/bin/Release/net6.0'),
      handler: 'ShiftPlanner.Api::ShiftPlanner.Api.LambdaEntryPoint::FunctionHandlerAsync',
      runtime: Runtime.DOTNET_6
    })
  }
}
