import { Construct } from 'constructs'
import { AttributeType, BillingMode, Table } from 'aws-cdk-lib/aws-dynamodb'
import { IGrantable } from 'aws-cdk-lib/aws-iam'

export class Database extends Construct {
  private readonly tables: Table[]

  constructor (scope: Construct, id: string) {
    super(scope, id)

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
  }

  public grantAccess (grantee: IGrantable): void {
    this.tables.forEach(table => {
      table.grantReadWriteData(grantee)
      table.grant(grantee, 'dynamodb:DescribeTable')
    })
  }

}
