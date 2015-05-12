var fs = require("fs");
var ConfigNode = require("./config-node");
var ShipConstruct = require("./ship-construct");

var file = fs.readFileSync("test.craft", "utf8");
var config = new ConfigNode();
config.load(file);
var craft = new ShipConstruct(config);

debugger;