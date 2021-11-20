const SimplexNoise = require('fast-simplex-noise');
const RandomGen = require('random-seed');
const Filters = require('./filters.js');
const util = require('./util');

module.exports = {
  generateNoiseMap(chunkLocation, mapSettings, regionFunction) {
    let noiseMap = [];

    let offset = { x: chunkLocation.x, y: chunkLocation.y };
    if (!offset || typeof offset.x === 'undefined' || typeof offset.y === 'undefined') {
			offset = { x: 0, y: 0 };
		}
		offset.x *= mapSettings.size;
		offset.y *= mapSettings.size;

    let borderSize = mapSettings.borderSize;
    if (!borderSize || borderSize <= 0) {
			borderSize = 0;
		}

    let scale = mapSettings.scale;
    if (!scale || scale <= 0) {
			scale = 0.0001;
		}

    let seed = mapSettings.seed;
    if (!seed || seed <= 0) {
			seed = 1;
		}

    let octaves = mapSettings.octaves;
    if (!octaves || octaves <= 0) {
			octaves = 1;
		}

    let persistance = mapSettings.persistance;
    if (!persistance) {
			persistance = 0.5;
    } else if (persistance < 0) {
			persistance = 0;
		} else if (persistance > 1) {
      persistance = 1;
    }

    let lacunarity = mapSettings.lacunarity;
    if (!lacunarity) {
      lacunarity = 2;
    } else if (lacunarity < 1) {
			lacunarity = 1;
		}

    let filters = mapSettings.filters || [];

		const rand = new RandomGen(seed);
    const makeNoise = SimplexNoise.makeNoise2D(rand.random);

		let amplitude = 1;
		let frequency = 1;
		let maxPossibleHeight = 0;
		const extraInfo = 10;

    const octaveOffsets = [];
		for (let i = 0; i < octaves; i++) {
			const offsetX = rand.intBetween(-100000, 100000) + offset.x;
			const offsetY = rand.intBetween(-100000, 100000) + offset.y;
			octaveOffsets [i] = { x: offsetX, y: offsetY };

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		function getRegionFromPos(x, y, mode) {
			let chunkPosX = chunkLocation.x;
			let chunkPosY = chunkLocation.y;
			if (y < extraInfo) {
				chunkPosY -= 1;
			}
			if (x <= extraInfo) {
				chunkPosX -= 1;
			}
			if (y > mapSettings.size + extraInfo) {
				chunkPosY += 1;
			}
			if (x > mapSettings.size + extraInfo) {
				chunkPosX += 1;
			}
			return regionFunction({ x: chunkPosX, y: chunkPosY }, mapSettings.regionSize);
		}

		const dataSize = mapSettings.size + (extraInfo * 2) + (borderSize * 2);
		const resultSize = mapSettings.size + (borderSize * 2);

		const cRegion = getRegionFromPos(Math.floor(dataSize / 2), Math.floor(dataSize / 2));
		const lRegion = getRegionFromPos(0, Math.floor(dataSize / 2));
		const rRegion = getRegionFromPos(dataSize - 1, Math.floor(dataSize / 2));
		const tRegion = getRegionFromPos(Math.floor(dataSize / 2), 0);
		const bRegion = getRegionFromPos(Math.floor(dataSize / 2), dataSize - 1);

		const c = (makeNoise(cRegion.r, cRegion.q) + 1) / 2;
		const l = (makeNoise(lRegion.r, lRegion.q) + 1) / 2;
		const r = (makeNoise(rRegion.r, rRegion.q) + 1) / 2;
		const t = (makeNoise(tRegion.r, tRegion.q) + 1) / 2;
		const b = (makeNoise(bRegion.r, bRegion.q) + 1) / 2;

		// Calcualte a larger area of noise so we can apply filters later
    for (let y = 0; y < dataSize; y++) {
      noiseMap.push([]);
			for (let x = 0; x < dataSize; x++) {
				amplitude = 1;
				frequency = 1;
				let noiseHeight = 0;

        for (let i = 0; i < octaves; i++) {
					const sampleX = (octaveOffsets[i].x + x - extraInfo - borderSize) / scale * frequency;
					const sampleY = (octaveOffsets[i].y + y - extraInfo - borderSize) / scale * frequency;

					let sampleValue = (makeNoise(sampleX, sampleY) + 1) / 2;
					if (i === 0 && mapSettings.regionSize > 0) {
						const myRegion = getRegionFromPos(x, y, 0);
						sampleValue = (makeNoise(myRegion.r, myRegion.q) + 1) / 2;

					}
					if (i < octaves / 1.5) {
						noiseHeight += sampleValue * amplitude;
					} else {
						noiseHeight -= sampleValue * amplitude;
					}

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				let normalizedHeight = util.clamp((noiseHeight + 0.0000001) / maxPossibleHeight, 0, 1);
				noiseMap[y].push(normalizedHeight);
			}
		}

		filters.forEach((f) => {
			if (Filters[f.type]) {
				noiseMap = Filters[f.type](f.param, noiseMap);
			}
		});

		// Return just the area for the heightmap
		const heightMap = [];
		for (let y = extraInfo; y < mapSettings.size + (borderSize * 2) + extraInfo; y++) {
      heightMap.push([]);
			for (let x = extraInfo; x < mapSettings.size + (borderSize * 2) + extraInfo; x++) {
				heightMap[y - extraInfo].push(noiseMap[y][x])
			}
		}

    return heightMap;
  },
};


// -3, 4
// -4, 3