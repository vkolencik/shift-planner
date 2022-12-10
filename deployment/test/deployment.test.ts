import * as cdk from 'aws-cdk-lib'
import { Template } from 'aws-cdk-lib/assertions'
import * as Deployment from '../lib/deployment-stack'
import { Environment } from '../lib/deployment-stack'

const productionParams = {
  stackName: 'KrtekStack',
  requirementsTableName: 'Requirements',
  scheduleS3BucketName: 'schedule-storage-720500131615',
  backendFunctionName: 'krtek-backend',
  backendCertificate: '*.krtek.me',
  backendUrl: 'b.krtek.me',
  lambdaEnvVar: 'Production',
  syntheticCanaryName: 'krtek-live-check' as string | null,
  dbBackupPlanName: 'db-backup'
}

type EnvironmentParams = typeof productionParams

const previewParams: EnvironmentParams = {
    stackName: 'KrtekStackPreview',
    requirementsTableName: '_preview-Requirements',
    scheduleS3BucketName: 'preview-schedule-storage-720500131615',
    backendFunctionName: '_preview-krtek-backend',
    backendCertificate: '*.preview.krtek.me',
    backendUrl: 'b.preview.krtek.me',
    lambdaEnvVar: 'Staging',
    syntheticCanaryName: null,
    dbBackupPlanName: '_preview-db-backup'
  }

;[
  { environment: Environment.Preview, environmentParams: previewParams },
  { environment: Environment.Production, environmentParams: productionParams },
].forEach(({ environment, environmentParams }) => {
    it(`Contains resources for environment ${environment}`, () => {
      process.env.RUNNING_IN_TEST = 'true'

      const app = new cdk.App()
      // WHEN
      const stack = new Deployment.DeploymentStack(app, environment, { env: { region: 'eu-central-1' } })
      expect(stack.stackName).toEqual(environmentParams.stackName)

      // THEN
      const template = Template.fromStack(stack)

      template.hasResourceProperties('AWS::DynamoDB::Table', {
        TableName: environmentParams.requirementsTableName,
        GlobalSecondaryIndexes: [
          { IndexName: 'TenantUserMonthIndex' },
          { IndexName: 'TenantMonthIndex' },
        ],
        PointInTimeRecoverySpecification: { PointInTimeRecoveryEnabled: true }
      })
      template.hasResourceProperties('AWS::Backup::BackupPlan', { BackupPlan: { BackupPlanName: environmentParams.dbBackupPlanName } })

      template.hasResourceProperties('AWS::S3::Bucket', {
        BucketName: environmentParams.scheduleS3BucketName,
        PublicAccessBlockConfiguration: {
          RestrictPublicBuckets: true
        }
      })

      template.hasResourceProperties('AWS::Lambda::Function', {
        FunctionName: environmentParams.backendFunctionName,
        Environment: {
          Variables: {
            ASPNETCORE_ENVIRONMENT: environmentParams.lambdaEnvVar
          }
        }
      })

      /*
            if (environmentParams.syntheticCanaryName !== null) {
              template.hasResourceProperties('AWS::Synthetics::Canary', {
                Name: environmentParams.syntheticCanaryName
              })
            }
      */

      template.hasResourceProperties('AWS::CertificateManager::Certificate', { DomainName: environmentParams.backendCertificate })
      template.hasResourceProperties('AWS::ApiGateway::DomainName', { DomainName: environmentParams.backendUrl })
    })
  }
)
