const noise = require('./noise');

module.exports = {
  generate(mapSettings, posX, posY) {
    const location = {
      x: posX,
      y: posY,
    };
    const mapData = {
      size: mapSettings.size,
      borderSize: mapSettings.borderSize,
      location,
      heightMap: noise.generateNoiseMap(location, mapSettings),
    };

    return mapData;
  },
};