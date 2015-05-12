var fs = require("fs");
var parser = require("./config-node");

var file = fs.readFileSync("test.craft", "utf8");
var configNode = parser(file);

debugger;