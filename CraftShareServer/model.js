var mongoose = require("mongoose");

var CraftSchema = mongoose.Schema({
    date: { type: Date, default: Date.now },
    name: { type: String, required: true },
    facility: { type: String, required: true },
    author: { type: String, required: true },
    craft: { type: Buffer, required: true },
    thumbnail: { type: Buffer }
});

var Craft = mongoose.model("Craft", CraftSchema);

module.exports = {
    Craft: Craft
};