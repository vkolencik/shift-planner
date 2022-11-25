import * as cdk from 'aws-cdk-lib'
import { Template } from 'aws-cdk-lib/assertions'
import * as Deployment from '../lib/deployment-stack'

it('Contains resources', () => {
  const app = new cdk.App()
  // WHEN
  const stack = new Deployment.DeploymentStack(app, 'MyTestStack', { env: { region: 'eu-central-1' } })
  // THEN
  const template = Template.fromStack(stack)

  template.hasResourceProperties('AWS::DynamoDB::Table', {
    TableName: 'Requirements',
    GlobalSecondaryIndexes: [
      { IndexName: 'TenantUserMonthIndex' },
      { IndexName: 'TenantMonthIndex' },
    ],
    PointInTimeRecoverySpecification: { PointInTimeRecoveryEnabled: true }
  })
  template.hasResource('AWS::Backup::BackupPlan', {})

  template.hasResourceProperties('AWS::Lambda::Function', {FunctionName: 'krtek-backend'})

  template.hasResourceProperties('AWS::CertificateManager::Certificate', {DomainName: '*.krtek.me'})
  template.hasResourceProperties('AWS::ApiGateway::DomainName', {DomainName: 'b.krtek.me'})

  expect(1).toBe(2)
})
