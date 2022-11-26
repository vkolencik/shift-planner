import { Construct } from 'constructs'
import { Alias, Code, CodeConfig, Function, ResourceBindOptions, Runtime } from 'aws-cdk-lib/aws-lambda'
import { CfnResource, Duration } from 'aws-cdk-lib'
import { Certificate, CertificateValidation } from 'aws-cdk-lib/aws-certificatemanager'
import { HostedZone } from 'aws-cdk-lib/aws-route53'
import { EndpointType, LambdaRestApi, SecurityPolicy } from 'aws-cdk-lib/aws-apigateway'
import { IGrantable, IPrincipal } from 'aws-cdk-lib/aws-iam'
import { Environment } from './deployment-stack'
import { RetentionDays } from 'aws-cdk-lib/aws-logs'

export class Backend extends Construct implements IGrantable {
  private readonly backendLambdaFunction: Function

  public get grantPrincipal (): IPrincipal {
    return this.backendLambdaFunction.grantPrincipal
  }

  public readonly backendDomain: string

  constructor (scope: Construct, environment: Environment) {
    super(scope, 'backend')

    const runningInTest = process.env.RUNNING_IN_TEST === 'true'

    this.backendLambdaFunction = new Function(this, 'backend-lambda-function', {
      functionName: environment === Environment.Production ? 'krtek-backend' : '_preview-krtek-backend',
      code: runningInTest ? dummyCode : Code.fromAsset('../backend/src/ShiftPlanner.Api/bin/Release/net6.0'),
      handler: 'ShiftPlanner.Api::ShiftPlanner.Api.LambdaEntryPoint::FunctionHandlerAsync',
      environment: {
        ASPNETCORE_ENVIRONMENT: environment === Environment.Production ? 'Production' : 'Staging'
      },
      runtime: Runtime.DOTNET_6,
      timeout: Duration.seconds(30),
     logRetention: RetentionDays.ONE_MONTH,
    })

    if (environment === Environment.Production) {
      const version = this.backendLambdaFunction.currentVersion
      const alias = new Alias(this, 'LambdaAlias', {
        aliasName: 'Provisioned',
        version,
        provisionedConcurrentExecutions: 2
      })
    }

    const hostedZoneId = 'Z10312824UKCHIHP3DBN'
    const cert = new Certificate(this, 'domain-certificate', {
      domainName: environment === Environment.Production ? '*.krtek.me' : '*.preview.krtek.me',
      validation: CertificateValidation.fromDns(HostedZone.fromHostedZoneId(this, 'hosted-zone-id', hostedZoneId))
    })

    this.backendDomain = environment === Environment.Production ? 'b.krtek.me' : 'b.preview.krtek.me'

    new LambdaRestApi(this, 'api-gateway', {
      handler: this.backendLambdaFunction,
      endpointTypes: [EndpointType.REGIONAL],
      restApiName: environment === Environment.Production ? 'krtek-api' : '_preview-krtek-api',
      domainName: {
        domainName: this.backendDomain,
        certificate: cert,
        endpointType: EndpointType.REGIONAL,
        securityPolicy: SecurityPolicy.TLS_1_2
      }
    })
  }
}

const dummyCode: Code = {
  bind (_: Construct): CodeConfig { return { s3Location: { bucketName: 'dummy', objectKey: 'dummy' } }},
  bindToResource (_resource: CfnResource, _options?: ResourceBindOptions): void {}
}
