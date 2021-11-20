const UrlPattern = require('url-pattern');
const chunk = require('./chunk');

const API_ROUTES = [
  '/chunk/:posX/:posY',
  '/info',
];

const MAP_SETTINGS = {
  size: 96,
  borderSize: 1,
  scale: 200,
  seed: 2000,
  octaves: 5,
  persistance: 0.5,
  lacunarity: 2.5,
  filters: [
    { type: 'convolve', param: 'BLUR_GAUSSIAN5' },
    // { type: 'step', param: [0, 0.25, 0.5, 0.75, 1] },
  ],
};

/**
 * Returns a API Route object from a APIGatewayEvent object.
 * 
 * @param {APIGatewayEvent} event 
 * @returns APIRoute
 */
const getRoute = (event) => {
  const route = {
    method: event.httpMethod.toUpperCase(),
    params: event.pathParameters,
    query: event.queryStringParameters,
    headers: event.headers,
    routePath: event.path,
    auth: { },
  };

  if (event.requestContext && event.requestContext.domainPrefix) {
    route.prefix = event.requestContext.domainPrefix;
    route.routePath = route.routePath.replace('/' + route.prefix, '');
  }

  let i = 0;
  do {
    const pattern = new UrlPattern(API_ROUTES[i]);
    const match = pattern.match(route.routePath);
    if (match !== null) {
      route.params = match;
      route.resource = API_ROUTES[i].toUpperCase();
    }
    i += 1;
  } while (i < API_ROUTES.length && !route.resource)

  if (!route.resource) {
    route.resource = 'Unknown resource';
  }

  if (route.headers && route.headers.Authorization) {
    route.auth.token = route.headers.Authorization.split(' ').pop();
  }

  if (route.headers && route.headers.Authorization) {
    route.auth.token = route.headers.Authorization.split(' ').pop();
  }

  try {
    route.bodyJson = JSON.parse(event.body);
  } catch (e) {
    route.body = event.body;
  }

  return route;
};

/**
 * The main API Gateway worker.
 * 
 * @param {APIGatewayEvent} event 
 * @returns APIGatewayResult
 */
exports.handler = async (event) => {

  // Return a specifica status code and object
  const done = (statusCode, response) => {
    return {
      statusCode,
      headers: {
        'Access-Control-Allow-Headers': 'Content-Type,X-Amz-Date,Authorization,X-Api-Key',
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Methods': '*',
      },
      body: JSON.stringify(response),
      isBase64Encoded: false,
    };
  };

  const route = getRoute(event);
  switch (route.resource) {
    case '/CHUNK/:POSX/:POSY':
      if (route.method === 'GET') {
        return done(200, chunk.generate(
          MAP_SETTINGS,
          parseInt(route.params.posX),
          parseInt(route.params.posY),
        ));
      } else {
        return done(405, `Method Not Allowed: ${route.method}`);
      }
      break;

    case '/INFO':
      if (route.method === 'GET') {
        return done(200, MAP_SETTINGS);
      } else {
        return done(405, `Method Not Allowed: ${route.method}`);
      }
      break;

    default:
      return done(400, `Bad Request: ${route.routePath} - ${route.resource}`);
  }
};
