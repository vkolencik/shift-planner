import { Construct } from 'constructs'
import { Code, CodeConfig, Function, ResourceBindOptions, Runtime } from 'aws-cdk-lib/aws-lambda'
import { CfnResource, Duration } from 'aws-cdk-lib'
import { Certificate, CertificateValidation } from 'aws-cdk-lib/aws-certificatemanager'
import { HostedZone } from 'aws-cdk-lib/aws-route53'
import { EndpointType, LambdaRestApi, SecurityPolicy } from 'aws-cdk-lib/aws-apigateway'
import { IGrantable, IPrincipal } from 'aws-cdk-lib/aws-iam'

export class Backend extends Construct implements IGrantable {
  private backendLambdaFunction: Function

  public get grantPrincipal (): IPrincipal {
    return this.backendLambdaFunction.grantPrincipal
  }

  constructor (scope: Construct, id: string) {
    super(scope, id)

    const runningInTest = process.env.RUNNING_IN_TEST === 'true'

    this.backendLambdaFunction = new Function(this, 'backend-lambda-function', {
      functionName: 'krtek-backend',
      code: runningInTest ? dummyCode : Code.fromAsset('../backend/src/ShiftPlanner.Api/bin/Release/net6.0'),
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
      handler: this.backendLambdaFunction,
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

const dummyCode: Code = {
  bind (scope: Construct): CodeConfig { return { s3Location: { bucketName: 'dummy', objectKey: 'dummy' } }},
  bindToResource (_resource: CfnResource, _options?: ResourceBindOptions): void {}
}
