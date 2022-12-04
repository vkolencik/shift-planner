import { Construct } from 'constructs'
import { Alias, Code, CodeConfig, Function, ResourceBindOptions, Runtime } from 'aws-cdk-lib/aws-lambda'
import { CfnResource, Duration } from 'aws-cdk-lib'
import { Certificate, CertificateValidation } from 'aws-cdk-lib/aws-certificatemanager'
import { HostedZone } from 'aws-cdk-lib/aws-route53'
import { EndpointType, LambdaRestApi, SecurityPolicy } from 'aws-cdk-lib/aws-apigateway'
import { IGrantable, IPrincipal, PolicyStatement } from 'aws-cdk-lib/aws-iam'
import { Environment } from './deployment-stack'
import { RetentionDays } from 'aws-cdk-lib/aws-logs'
import { ApplicationLoadBalancedFargateService } from 'aws-cdk-lib/aws-ecs-patterns'
import { Cluster, ContainerDefinition, ContainerImage, ContainerImageConfig } from 'aws-cdk-lib/aws-ecs'
import { CfnInternetGateway, SubnetType, Vpc } from 'aws-cdk-lib/aws-ec2'

export class Backend extends Construct implements IGrantable {
  private readonly backendLambdaFunction: Function
  public readonly service: ApplicationLoadBalancedFargateService

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

    this.backendLambdaFunction.addToRolePolicy(new PolicyStatement({
      resources: ['arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP'],
      actions: ['secretsmanager:GetSecretValue']
    }))

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

    const vpc = new Vpc(this, 'vpc2', {
      vpcName: '_preview-krtek-vpc',
      natGateways: 0,
      vpnGateway: true
    })

    const cluster = new Cluster(this, 'cluster2', {
      clusterName: '_preview-krtek-cluster',
      vpc
    })

    this.service = new ApplicationLoadBalancedFargateService(this, 'service2', {
      domainName: 'b2.preview.krtek.me',
      domainZone: HostedZone.fromHostedZoneAttributes(this, `hosted-zone`, {
        zoneName: 'preview.krtek.me',
        hostedZoneId: hostedZoneId
      }),
      certificate: cert,
      publicLoadBalancer: true,
      desiredCount: 2,
      cluster: cluster,
      serviceName: '_preview-krtek',
      taskImageOptions: {
        // `deployment/cdk.out` must be ignored in `.dockerignore`
        image: runningInTest ? new DummyImage() : ContainerImage.fromAsset('../backend'),
        environment: {
          ASPNETCORE_ENVIRONMENT: environment === Environment.Production ? 'Production' : 'Staging'
        }
      }
    })

    this.service.taskDefinition.taskRole.addToPrincipalPolicy(new PolicyStatement({
      resources: ['arn:aws:secretsmanager:eu-central-1:720500131615:secret:auth0-mgmt-api-key-vdXHOP'],
      actions: ['secretsmanager:GetSecretValue']
    }))
  }
}

const dummyCode: Code = {
  bind (_: Construct): CodeConfig { return { s3Location: { bucketName: 'dummy', objectKey: 'dummy' } }},
  bindToResource (_resource: CfnResource, _options?: ResourceBindOptions): void {}
}

class DummyImage extends ContainerImage {
  bind (scope: Construct, containerDefinition: ContainerDefinition): ContainerImageConfig {
    return {
      imageName: 'dummy'
    }
  }
}
