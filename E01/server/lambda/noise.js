const SimplexNoise = require('fast-simplex-noise');
const RandomGen = require('random-seed');

module.exports = {
  generateNoiseMap(chunkLocation, mapSettings) {
    const noiseMap = [];

    let offset = { x: chunkLocation.x, y: chunkLocation.y };
    if (!offset || typeof offset.x === 'undefined' || typeof offset.y === 'undefined') {
			offset = { x: 0, y: 0 };
		}
		offset.x *= mapSettings.size;
		offset.y *= mapSettings.size;

    let scale = mapSettings.scale;
    if (!scale || scale <= 0) {
			scale = 0.0001;
		}

    let seed = mapSettings.seed;
    if (!seed || seed <= 0) {
			seed = 1;
		}

    const rand = new RandomGen(seed);
    const makeNoise = SimplexNoise.makeNoise2D(rand.random);

    for (let y = 0; y < mapSettings.size; y++) {
      noiseMap.push([]);
			for (let x = 0; x < mapSettings.size; x++) {
				const sampleX = (offset.x + x) / scale;
				const sampleY = (offset.y + y) / scale;

        // sampleValue is normalized to be between 0 and 1
				const sampleValue = (makeNoise(sampleX, sampleY) + 1) / 2;

        noiseMap[y].push(sampleValue);
			}
		}

    return noiseMap;
  },
};