import { Construct } from 'constructs'
import { AttributeType, Table } from 'aws-cdk-lib/aws-dynamodb'

export class Database extends Construct {
  constructor (scope: Construct, id: string) {
    super(scope, id)

    const requirementsTable = new Table(this, 'requirements-table', {
      partitionKey: {
        name: 'Key',
        type: AttributeType.STRING
      },
      tableName: 'Requirements'
    })

    requirementsTable.addGlobalSecondaryIndex({
      indexName: 'UserMonthIndex',
      partitionKey: {
        name: 'TenantUserMonthKey',
        type: AttributeType.STRING
      }
    })

    requirementsTable.addGlobalSecondaryIndex({
      indexName: 'UserMonthIndex',
      partitionKey: {
        name: 'TenantMonthKey',
        type: AttributeType.STRING
      }
    })
  }
}
