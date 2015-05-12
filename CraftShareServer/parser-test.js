var fs = require("fs");
var configNode = require("./config-node");

var file = fs.readFileSync("test.craft", "utf8");
var parsed = configNode(file);

debugger;