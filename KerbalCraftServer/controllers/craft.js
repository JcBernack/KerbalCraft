var lzf = require("lzf");
var Craft = require("../models/craft");

function queryInt(req, name, standard, min, max) {
    if (!req.query.hasOwnProperty(name)) return standard;
    var value = parseInt(req.query[name]);
    if (isNaN(value)) value = standard;
    if (min && value < min) value = min;
    if (max && value > max) value = max;
    return value;
}

function handleError(err, res) {
    console.log("Error:", err);
    res.status(err.name === "ValidationError" ? 400 : 500).end();
}

// POST craft, including thumbnail and part list
module.exports.postCraft = function (req, res) {
    if (!req.files.hasOwnProperty("thumbnail") || !req.files.hasOwnProperty("craftData")) {
        return res.status(400).end();
    }
    var thumbnail = req.files.thumbnail[0];
    var craftData = req.files.craftData[0];
    if (thumbnail.truncated || craftData.truncated) {
        // 413 Request Entity Too Large
        console.log("Request aborted.");
        return res.status(413).end();
    }
    // create craft document
    var craft = new Craft({
        author: req.body.author,
        craft: lzf.decompress(craftData.buffer).toString("utf8"),
        thumbnail: thumbnail.buffer
    });
    // invoke parser to add craft information
    craft.parseCraft();
    // save it to the database
    craft.save(function (err) {
        if (err) return handleError(err, res);
        // return the new item back to client with updated fields like _id and date but without the binary data fields
        var ret = craft.toObject();
        delete ret.craft;
        delete ret.thumbnail;
        res.send(ret);
    });
};

// GET craft list, without thumbnail and part list
module.exports.getCraft = function(req, res) {
    var skip = queryInt(req, "skip", 0, 0);
    var limit = queryInt(req, "limit", 20, 1, 50);
    Craft.find(null, { date: true, author: true, info: true },
        { sort: { date: -1 }, skip: skip, limit: limit },
        function(err, crafts) {
            if (err) return handleError(err, res);
            res.send(crafts);
        }
    );
};
    
// GET craft data
module.exports.getCraftData = function(req, res) {
    Craft.findById(req.params.id, { _id: false, craft: true },
        function(err, craft) {
            if (err) return handleError(err, res);
            if (!craft) return res.status(404).end();
            res.set("Content-Type", "text/plain");
            res.send(lzf.compress(new Buffer(craft.craft)));
        }
    );
};
    
// GET craft thumbnail
module.exports.getCraftThumbnail = function(req, res) {
    Craft.findById(req.params.id, { _id: false, thumbnail: true },
        function(err, craft) {
            if (err) return handleError(err, res);
            if (!craft) return res.status(404).end();
            res.set("Content-Type", "image/png");
            res.send(craft.thumbnail);
        }
    );
};
    
// DELETE craft
module.exports.deleteCraft = function(req, res) {
    //TODO: find out how to remove without selecting the removed entry
    Craft.findByIdAndRemove(req.params.id, { select: { _id: true } },
        function(err, craft) {
            if (err) return handleError(err, res);
            if (!craft) return res.status(404).end();
            res.status(204).end();
        }
    );
};
