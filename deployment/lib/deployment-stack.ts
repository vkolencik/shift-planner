import * as cdk from 'aws-cdk-lib'
import { Construct } from 'constructs'
import { Code, Function, Runtime } from 'aws-cdk-lib/aws-lambda'
import { EndpointType, LambdaRestApi, SecurityPolicy } from 'aws-cdk-lib/aws-apigateway'
import { Certificate, CertificateValidation } from 'aws-cdk-lib/aws-certificatemanager'
import { HostedZone } from 'aws-cdk-lib/aws-route53'
import { Duration } from 'aws-cdk-lib'

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const backend = new Function(this, 'backend-lambda-function', {
      code: Code.fromAsset('../backend/src/ShiftPlanner.Api/bin/Release/net6.0'),
      handler: 'ShiftPlanner.Api::ShiftPlanner.Api.LambdaEntryPoint::FunctionHandlerAsync',
      runtime: Runtime.DOTNET_6,
      timeout: Duration.seconds(15)
    })

    const hostedZoneId = 'Z10312824UKCHIHP3DBN'
    const cert = new Certificate(this, 'domain-certificate', {
      domainName: '*.krtek.me',
      validation: CertificateValidation.fromDns(HostedZone.fromHostedZoneId(this, 'hosted-zone-id', hostedZoneId))
    })

    new LambdaRestApi(this, 'api-gateway', {
      handler: backend,
      endpointTypes: [EndpointType.REGIONAL],
      domainName: {
        domainName: 'b.krtek.me',
        certificate: cert,
        endpointType: EndpointType.REGIONAL,
        securityPolicy: SecurityPolicy.TLS_1_2
      }
    })
  }
}
