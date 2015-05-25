var mongoose = require("mongoose");
var parser = require("../config-node-parser");

var CraftSchema = mongoose.Schema({
    date: { type: Date, default: Date.now },
    author: { type: mongoose.Schema.Types.ObjectId, ref: "User", required: true },
    craft: { type: String, required: true },
    thumbnail: { type: Buffer, required: true },
    impressions: { type: Number, default: 0 },
    downloads: { type: Number, default: 0 },
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
        "Name": v.ship,
        "Type": v.type,
        "Part Count": config.countByName("PART"),
        "Size": sizes.join(" x "),
        "KSP Version": v.version,
        "Description": v.description.replace(/\u00A8/g, "\n")
    };
    // allow chaining
    return this;
};

module.exports = mongoose.model("Craft", CraftSchema);
