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


			if ((i + 1) % 3) {
				maxPossibleHeight += amplitude;
			} else {
				maxPossibleHeight -= amplitude;
			}
			amplitude *= persistance;
		}

		function getRegionFromPos(x, y) {
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

		// Get the region for each of the 8 serounding regions
		const cRegion = getRegionFromPos(Math.floor(dataSize / 2), Math.floor(dataSize / 2));
		const lRegion = getRegionFromPos(0, Math.floor(dataSize / 2));
		const rRegion = getRegionFromPos(dataSize - 1, Math.floor(dataSize / 2));
		const tRegion = getRegionFromPos(Math.floor(dataSize / 2), 0);
		const bRegion = getRegionFromPos(Math.floor(dataSize / 2), dataSize - 1);
		const tlRegion = getRegionFromPos(0, 0);
		const trRegion = getRegionFromPos(dataSize - 1, 0);
		const blRegion = getRegionFromPos(0, dataSize - 1);
		const brRegion = getRegionFromPos(dataSize - 1, dataSize - 1);

		// Get the color of the serounding regions
		const c = (makeNoise(cRegion.r, cRegion.q) + 1) / 2;
		const l = (makeNoise(lRegion.r, lRegion.q) + 1) / 2;
		const r = (makeNoise(rRegion.r, rRegion.q) + 1) / 2;
		const t = (makeNoise(tRegion.r, tRegion.q) + 1) / 2;
		const b = (makeNoise(bRegion.r, bRegion.q) + 1) / 2;
		const tl = (makeNoise(tlRegion.r, tlRegion.q) + 1) / 2;
		const tr = (makeNoise(trRegion.r, trRegion.q) + 1) / 2;
		const bl = (makeNoise(blRegion.r, blRegion.q) + 1) / 2;
		const br = (makeNoise(brRegion.r, brRegion.q) + 1) / 2;

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
					const locX = x - extraInfo - borderSize;
					const locY = y - extraInfo - borderSize;
					const percentX = locX / (mapSettings.size - 1);
					const percentY = locY / (mapSettings.size - 1);

					// Get the current sample's noise value
					let sampleValue = (makeNoise(sampleX, sampleY) + 1) / 2;

					// If this is octive 0, then handle the region handling code
					if (i === 0 && mapSettings.regionSize > 0) {
						const myRegion = getRegionFromPos(x, y);
						const my = (makeNoise(myRegion.r, myRegion.q) + 1) / 2;
						sampleValue = my;

						// Top left corner max value
						let maxTL = c;
						if (t >= tl && t >= l && t >= c) {
							maxTL = t;
						} else if (tl >= t && tl >= l && tl >= c) {
							maxTL = tl;
						} else if (l >= tl && l >= t && l >= c) {
							maxTL = l;
						}

						// Top right corner max value
						let maxTR = c;
						if (t >= tr && t >= r && t >= c) {
							maxTR = t;
						} else if (tr >= t && tr >= r && tr >= c) {
							maxTR = tr;
						} else if (r >= tr && r >= t && r >= c) {
							maxTR = r;
						}

						// Bottom left corner max value
						let maxBL = c;
						if (b >= bl && b >= l && b >= c) {
							maxBL = b;
						} else if (bl >= b && bl >= l && bl >= c) {
							maxBL = bl;
						} else if (l >= bl && l >= b && l >= c) {
							maxBL = l;
						}

						// Bottom right corner max value
						let maxBR = c;
						if (b >= br && b >= r && b >= c) {
							maxBR = b;
						} else if (br >= b && br >= r && br >= c) {
							maxBR = br;
						} else if (r >= br && r >= b && r >= c) {
							maxBR = r;
						}

						// Get the sample value for each location in the region
						const L = (1 - percentY) * maxTL + percentY * maxBL;
						const R = (1 - percentY) * maxTR + percentY * maxBR;
						sampleValue = util.clamp((1 - percentX) * L + percentX * R, 0, 1);
					}

					// Depending on the octive add or subtract it from
					// the current noise height
					if ((i + 1) % 3) {
						noiseHeight += sampleValue * amplitude;
					} else {
						noiseHeight -= sampleValue * amplitude;
					}

					// Clamp things so we are out of bounds
					noiseHeight = util.clamp(noiseHeight, 0, maxPossibleHeight);

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				const normalizedHeight = util.clamp((noiseHeight + 0.0000001) / maxPossibleHeight, 0, 1);
				noiseMap[y].push(normalizedHeight);
			}
		}

		// Apply filters
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
