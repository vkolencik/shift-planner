import {Construct} from 'constructs'
import {AttributeType, BillingMode, Table} from 'aws-cdk-lib/aws-dynamodb'
import {IGrantable} from 'aws-cdk-lib/aws-iam'
import {BackupPlan, BackupPlanRule, BackupResource} from 'aws-cdk-lib/aws-backup'
import { Environment } from './deployment-stack'

export class Database extends Construct {
  private readonly tables: Table[]

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
  }

  public grantAccess(grantee: IGrantable): void {
    this.tables.forEach(table => {
      table.grantReadWriteData(grantee)
      table.grant(grantee, 'dynamodb:DescribeTable')
    })
  }

}
