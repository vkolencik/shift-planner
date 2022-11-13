import * as cdk from 'aws-cdk-lib'
import { Construct } from 'constructs'
import { Backend } from './backend'
import { Database } from './database'

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props)

    new Backend(this, 'backend')
    new Database(this, 'database')
  }
}
