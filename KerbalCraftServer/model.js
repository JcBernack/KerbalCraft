var mongoose = require("mongoose");
var parser = require("./config-node-parser");

var CraftSchema = mongoose.Schema({
    date: { type: Date, default: Date.now },
    author: { type: String, required: true },
    craft: { type: String, required: true },
    thumbnail: { type: Buffer, required: true },
    info: { type: mongoose.Schema.Types.Mixed }
});

CraftSchema.methods.parseCraft = function () {
    var config = parser.load(this.craft);
    var v = config.values;
    // parse and format the ship size, also round to two decimal places
    var sizes = [];
    v.size.split(",").forEach(function (str) {
        sizes.push(parseFloat(str).toFixed(2));
    });
    // update the information object
    this.info = {
        ship: v.ship,
        type: v.type,
        description: v.description,
        version: v.version,
        partCount: config.countByName("PART"),
        size: sizes.join(" x ")
    };
    // allow chaining
    return this;
};

var Craft = mongoose.model("Craft", CraftSchema);

module.exports = {
    Craft: Craft
};