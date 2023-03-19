import { Construct } from 'constructs'
import { Backend } from './backend'
import { Database } from './database'
import { Environment } from './deployment-stack'

export class Monitoring extends Construct {
  constructor (scope: Construct, environment: Environment, backend: Backend, database: Database) {
    super(scope, 'monitoring')

/*
    const canary = new Canary(this, 'LivenessCheckCanary', {
      canaryName: environment === Environment.Production ? 'krtek-live-check' : '_p-krtek-live-check',
      schedule: Schedule.rate(Duration.minutes(5)),
      test: Test.custom({
        code: Code.fromAsset(path.join(__dirname, 'canary')),
        handler: 'index.handler',
      }),
      runtime: Runtime.SYNTHETICS_NODEJS_PUPPETEER_3_8,
      environmentVariables: {
        backendHost: backend.backendDomain,
      },
    })

    new Alarm(this, 'FailedCanaryAlarm', {
      evaluationPeriods: 1,
      metric: canary.metricFailed(),
      threshold: 1,
      comparisonOperator: ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
      treatMissingData: TreatMissingData.BREACHING
    })
*/
  }
}
