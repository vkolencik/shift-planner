import { Construct } from 'constructs'
import { AttributeType, BillingMode, Table } from 'aws-cdk-lib/aws-dynamodb'
import { IGrantable } from 'aws-cdk-lib/aws-iam'
import { BackupPlan, BackupPlanRule, BackupResource } from 'aws-cdk-lib/aws-backup'
import { Schedule } from 'aws-cdk-lib/aws-events'
import { Duration } from 'aws-cdk-lib'

export class Database extends Construct {
  private readonly tables: Table[]

  constructor (scope: Construct, id: string) {
    super(scope, id)

    this.tables = []

    const requirementsTable = new Table(this, 'requirements-table', {
      partitionKey: {
        name: 'Key',
        type: AttributeType.STRING
      },
      tableName: 'Requirements',
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
      backupPlanName: 'db-backup',
      backupPlanRules: [
        new BackupPlanRule({
          scheduleExpression: Schedule.rate(Duration.days(1)),
          deleteAfter: Duration.days(30)
        })
      ]
    })
    backupPlan.addSelection('selection', {
      resources: this.tables.map(table => BackupResource.fromDynamoDbTable(table))
    })
  }

  public grantAccess (grantee: IGrantable): void {
    this.tables.forEach(table => {
      table.grantReadWriteData(grantee)
      table.grant(grantee, 'dynamodb:DescribeTable')
    })
  }

}
