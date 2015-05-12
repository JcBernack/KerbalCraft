//require("./config-parser");

function ShipConstruct(configNode) {
    var v = configNode.values;
    this.name = v.ship;
    this.type = v.type;
    this.description = v.description;
    this.version = v.version;
    var sizes = [];
    configNode.values.size.split(",").forEach(function (str) {
        sizes.push(parseFloat(str).toFixed(2));
    });
    this.size = sizes.join(" x ");
    this.partCount = configNode.countByName("PART");
}

module.exports = ShipConstruct;