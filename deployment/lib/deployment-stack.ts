import * as cdk from 'aws-cdk-lib'
import { Construct } from 'constructs'
import { Backend } from './backend'
import { Database } from './database'
import { Monitoring } from './monitoring'

export enum Environment {
  Production = 'prod',
  Preview = 'prev'
}

export class DeploymentStack extends cdk.Stack {
  constructor(scope: Construct, environment: Environment, props?: cdk.StackProps) {
    super(scope, environment === Environment.Production ? 'KrtekStack' : 'KrtekStackPreview', props)

    const backend = new Backend(this, environment)
    const database = new Database(this, environment)
    database.grantAccess(backend)

    const monitoring = new Monitoring(this, environment, backend, database)
  }
}

