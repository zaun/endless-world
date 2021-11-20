
function newArray(size) {
	var result = new Array(size);
	for (var i = 0; i < size; i++) {
		result[i] = new Array(size);
	}
	return result;
}

const kernels = {
  IDENTITY: [
    [0, 0, 0],
    [0, 1, 0],
    [0, 0, 0],
  ],
  
  EDGE: [
    [-1, -1, -1],
    [-1,  8, -1],
    [-1, -1, -1],
  ],
  
  SHARPEN: [
    [ 0, -1, 0],
    [-1, 5, -1],
    [ 0, -1, 0],
  ],
  
  BLUR_BOX: [
    [0.1111111111, 0.1111111111, 0.1111111111],
    [0.1111111111, 0.1111111111, 0.1111111111],
    [0.1111111111, 0.1111111111, 0.1111111111],
  ],
  
  BLUR_GAUSSIAN3: [
    [0.0625, 0.1250, 0.0625],
    [0.1250, 0.2500, 0.1250],
    [0.0625, 0.1250, 0.0625],
  ],
  
  BLUR_GAUSSIAN5: [
    [0.00390625, 0.01562500, 0.02343750, 0.01562500, 0.00390625],
    [0.01562500, 0.06250000, 0.09375000, 0.06250000, 0.01562500],
    [0.02343750, 0.09375000, 0.14062500, 0.09375000, 0.02343750],
    [0.01562500, 0.06250000, 0.09375000, 0.06250000, 0.01562500],
    [0.00390625, 0.01562500, 0.02343750, 0.01562500, 0.00390625],
  ],
};

module.exports = {
  convolve(kernel, array) {
    let filter = kernels.IDENTITY;
    if (kernels[kernel]) {
      filter = kernels[kernel];
    }

    var result = newArray(array.length);
    for (var i = 0; i < array.length; i++) {
      var arrayRow = array[i];
      for (var j = 0; j <= arrayRow.length; j++) {
  
        var sum = 0;
        for (var w = 0; w < filter.length; w++) {
            if(array.length - i < filter.length) {
              break;
            }
  
            var filterRow = filter[w];
            for (var z = 0; z < filter.length; z++) {
              if(arrayRow.length - j < filterRow.length) {
                break;
              }
              sum += array[w + i][z + j] * filter[w][z];
            }
        }
  
        if(i < array.length - filter.length + 1 && j < array.length - filter.length + 1) {
          result[i][j] = Math.abs(sum);
        }
      }   
    }
    return result;
  },

  step(steps, array) {
    var result = newArray(array.length);
    for (var y = 0; y < array.length; y++) {
      var arrayRow = array[y];
      for (var x = 0; x <= arrayRow.length; x++) {
        for (var i = 1; i < steps.length; i++) {
          if (array[y][x] <= steps[i] && array[y][x] > steps[i - 1]) {
            result[y][x] = (1 / (steps.length - 2)) * (i - 1);
          }
        }
      }
    }
    return result;
  },
};
