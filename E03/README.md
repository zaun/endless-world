# Server Setup

Add the ability to apply filters to the generated noise data

### Amazon Web Services

The server will run on AWS. You will need and account in order to deploy this in a public setting for multipule people to acces. You do not need an AWS account while developing and testing local on your computer.

### The server

The server will be contained in the server directory. The basic file structure will be as follows:

* server/etc - location of helper files
* server/lambda - location of the AWS lambda function
* server/local - location of a local development server

### Goals

Add a borderSize option to return a little extra heightmap than is needed. This will be used in the future when generating 3D meshes.
