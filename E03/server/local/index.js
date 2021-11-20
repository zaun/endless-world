/**
 * A simple Express based web server that will mimic AWS API Gateway
 * to call the lambda function. This lets the developer run a local
 * server to develop and test a lambda function.
 */

/* eslint-disable no-console */
const express = require('express');
const lambda = require('../lambda');
const _ = require('lodash');

const apiLocalPort = 7000;

const app = express();
app.use(express.urlencoded());
app.use(express.json({ limit: '2mb' }));
app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  res.header('Access-Control-Allow-Methods', '*');
  res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept, Authorization');
  if (req.method === 'OPTIONS') {
    res.sendStatus(200);
  } else {
    next();
  }
});

app.all('*', (req, res) => {
  process.stdout.write(`??? ${req.method} ${req.originalUrl}`);
  // console.log(req);
  lambda.handler({
    path: req._parsedUrl.pathname,
    headers: req.headers,
    httpMethod: req.method,
    pathParameters: req.params,
    queryStringParameters: req.query,
    body: JSON.stringify(req.body),
  }).then((result) => {
    const data = _.cloneDeep(result);
    process.stdout.clearLine();
    process.stdout.cursorTo(0);
    if (data.statusCode === 400) {
      console.log(`${data.statusCode} ${req.method} ${req.originalUrl} ${data.body}`);
    } else {
      console.log(`${data.statusCode} ${req.method} ${req.originalUrl}`);
    }

    res.status(data.statusCode).set(data.headers).send(data.body);
    // setTimeout(() => {
    //   res.status(data.statusCode).set(data.headers).send(data.body);
    // }, Math.floor(Math.random() * 2000) + 200);
  }).catch((err) => {
    const data = _.cloneDeep(err);
    data.statusCode = data.statusCode || 500;
    data.body = data.body || err ? err.message + '\n' + err.stack : 'Unknown Error';

    process.stdout.clearLine();
    process.stdout.cursorTo(0);
    if (data.statusCode === 400) {
      console.log(`${data.statusCode} ${req.method} ${req.originalUrl} ${data.body}`);
    } else {
      console.log(`${data.statusCode} ${req.method} ${req.originalUrl}`);
    }
    res.status(data.statusCode).set(data.headers).send(data.body);
  });
});

const server = app.listen(apiLocalPort, () => {
  console.log(`API on port ${server.address().port}`);
});
