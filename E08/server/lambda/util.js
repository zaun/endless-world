const axialToCube = (hex) => {
	const x = hex.q;
	const z = hex.r;
	const y = -x - z;
	return { x, y, z };
};

const cubeToAxial = (cube) => {
	var q = cube.x;
	var r = cube.z;
	return { q, r };
};

const cubeRound = (cube) => {
	var rx = Math.round(cube.x);
	var ry = Math.round(cube.y);
	var rz = Math.round(cube.z);

	var x_diff = Math.abs(rx - cube.x);
	var y_diff = Math.abs(ry - cube.y);
	var z_diff = Math.abs(rz - cube.z);

	if (x_diff > y_diff && x_diff > z_diff) {
		rx = -ry-rz;
	} else if (y_diff > z_diff) {
		ry = -rx-rz;
	} else {
		rz = -rx-ry;
	}

	return { x: rx, y: ry, z: rz };
};

const axialRound = (hex) => {
	return cubeToAxial(cubeRound(axialToCube(hex)));
};

const pointToAxialA = (location, size) => {
	var q = (0.5773502691896257 * location.x  -  0.3333333333 * location.y) / size;
	var r = (0.6666666667 * location.y) / size;
	return { q, r };
};

const pointToAxialB = (location, size) => {
	var q = (0.6666666667 * location.x) / size;
	var r = (-0.3333333333 * location.x + 0.5773502691896257 * location.y) / size;
	return { q, r };
};

module.exports = {
  HEX_POINT(location, size) {
		var hex = pointToAxialA(location, size);
		return axialRound(hex);
	},

	HEX_FLAT(location, size) {
		var hex = pointToAxialB(location, size);
		return axialRound(hex);
	},

	clamp(val, min, max) {
		if (val < min) {
			return min;
		} else if (val > max) {
			return max;
		}
		return val;
	},
};
