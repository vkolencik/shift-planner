import * as cdk from 'aws-cdk-lib'
import { Construct } from 'constructs'
import { Backend } from './backend'
import { Database } from './database'

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    const backend = new Backend(this, 'backend')
    const database = new Database(this, 'database')

    database.grantAccess(backend)
  }
}
