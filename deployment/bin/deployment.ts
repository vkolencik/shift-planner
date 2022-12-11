#!/usr/bin/env node
import 'source-map-support/register'
import * as cdk from 'aws-cdk-lib'
import { DeploymentStack, Environment } from '../lib/deployment-stack'

const app = new cdk.App()
const environment = process.env.CIRCLE_BRANCH === 'main' ? Environment.Production : Environment.Preview

new DeploymentStack(app, environment, { env: { region: 'eu-central-1', account: '720500131615' } })
