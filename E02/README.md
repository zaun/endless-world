# Server Setup

The primary goal is to setup a simple server to generate map data that can be used by a client to display a terrain.

### Amazon Web Services

The server will run on AWS. You will need and account in order to deploy this in a public setting for multipule people to acces. You do not need an AWS account while developing and testing local on your computer.

### The server

The server will be contained in the server directory. The basic file structure will be as follows:

* server/etc - location of helper files
* server/lambda - location of the AWS lambda function
* server/local - location of a local development server

### Goals

Add the ability to have octaves when generating heightmaps to provide a little more interesting data rather than strict noise. Add the ability to returned stepped heightmaps.
