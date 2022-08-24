import * as cdk from 'aws-cdk-lib'
import { Construct } from 'constructs'
import { Code, Function, Runtime } from 'aws-cdk-lib/aws-lambda'
import { ApiKeySourceType, EndpointType, LambdaRestApi } from 'aws-cdk-lib/aws-apigateway'

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const backend = new Function(this, 'backend-lambda-function', {
      code: Code.fromAsset('../backend/src/ShiftPlanner.Api/bin/Release/net6.0'),
      handler: 'ShiftPlanner.Api::ShiftPlanner.Api.LambdaEntryPoint::FunctionHandlerAsync',
      runtime: Runtime.DOTNET_6
    })

    const api = new LambdaRestApi(this, 'api-gateway', {
      handler: backend,
      endpointTypes: [EndpointType.REGIONAL],
      apiKeySourceType: ApiKeySourceType.HEADER,
    })
    api.addApiKey('dev-api-key', {
      apiKeyName: 'Dev API key',
      value: 'a8c46cac-4dcf-4e4d-a48c-9e7f4ec75623'
    })
  }
}
