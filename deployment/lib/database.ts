import { Construct } from 'constructs'
import { AttributeType, BillingMode, Table } from 'aws-cdk-lib/aws-dynamodb'
import { IGrantable } from 'aws-cdk-lib/aws-iam'
import { BackupPlan, BackupPlanRule, BackupResource } from 'aws-cdk-lib/aws-backup'
import { Environment } from './deployment-stack'
import { Bucket, BucketAccessControl } from 'aws-cdk-lib/aws-s3'

export class Database extends Construct {
  private readonly tables: Table[]
  private readonly buckets: Bucket[]

  constructor (scope: Construct, environment: Environment) {
    super(scope, 'database')

    this.tables = []

    const requirementsTable = new Table(this, 'requirements-table', {
      partitionKey: {
        name: 'Key',
        type: AttributeType.STRING
      },
      tableName: environment === Environment.Production ? 'Requirements' : '_preview-Requirements',
      pointInTimeRecovery: true,
      billingMode: BillingMode.PAY_PER_REQUEST,

    })
    this.tables.push(requirementsTable)

    requirementsTable.addGlobalSecondaryIndex({
      indexName: 'TenantUserMonthIndex',
      partitionKey: {
        name: 'TenantUserMonthKey',
        type: AttributeType.STRING
      }
    })

    requirementsTable.addGlobalSecondaryIndex({
      indexName: 'TenantMonthIndex',
      partitionKey: {
        name: 'TenantMonthKey',
        type: AttributeType.STRING
      }
    })

    const backupPlan = new BackupPlan(this, 'db-backup', {
      backupPlanName: environment === Environment.Production ? 'db-backup' : '_preview-db-backup',
      backupPlanRules: [BackupPlanRule.daily()]
    })
    backupPlan.addSelection('selection', {
      resources: this.tables.map(table => BackupResource.fromDynamoDbTable(table))
    })

    this.buckets = []

    const scheduleBucket = new Bucket(this, 'schedule-bucket', {
      bucketName: environment === Environment.Production ? 'schedule-storage-720500131615' : '_preview-schedule-storage-720500131615',
      accessControl: BucketAccessControl.PRIVATE,
      blockPublicAccess: { restrictPublicBuckets: true, blockPublicAcls: true, blockPublicPolicy: true, ignorePublicAcls: true }
    })

    this.buckets.push(scheduleBucket)
  }

  public grantAccess (grantee: IGrantable): void {
    this.tables.forEach(table => {
      table.grantReadWriteData(grantee)
      table.grant(grantee, 'dynamodb:DescribeTable')
    })

    this.buckets.forEach(bucket => {
      bucket.grantReadWrite(grantee)
    })
  }
}
