const noise = require('./noise');
const util = require('./util');

module.exports = {
  generate(mapSettings, posX, posY) {
    const location = {
      x: posX,
      y: posY,
    };

    const defaultFunc = () => ({ r: 0, q: 0 });
    const regionFunction = mapSettings.regionSize ? util[mapSettings.regionType] || defaultFunc : defaultFunc;

    const mapData = {
      size: mapSettings.size,
      borderSize: mapSettings.borderSize,
      location,
      region: regionFunction(location, mapSettings.regionSize),
      heightMap: noise.generateNoiseMap(location, mapSettings, regionFunction),
    };

    return mapData;
  },
};