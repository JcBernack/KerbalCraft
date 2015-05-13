var lzf = require("lzf");
var multer = require("multer");
var model = require("./model.js");

function queryInt(request, name, standard, min, max) {
    if (!request.query.hasOwnProperty(name)) return standard;
    var value = parseInt(request.query[name]);
    if (isNaN(value)) value = standard;
    if (min && value < min) value = min;
    if (max && value > max) value = max;
    return value;
}

function handleError(error, response) {
    console.log("Error:", error);
    response.status(error.name === "ValidationError" ? 400 : 500).end();
}

// define REST routes
module.exports = function (router) {
    // GET craft list, without thumbnail and part list
    router.get("/craft", function (request, response) {
        var skip = queryInt(request, "skip", 0, 0);
        var limit = queryInt(request, "limit", 20, 1, 50);
        model.Craft.find(null, { date: true, author: true, info: true }, { sort: { date: -1 }, skip: skip, limit: limit }, function (error, crafts) {
            if (error) return handleError(error, response);
            response.send(crafts);
        });
    });
    
    // GET craft data
    router.get("/craft/:id/data", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, craft: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.set("Content-Type", "text/plain");
            response.send(lzf.compress(new Buffer(craft.craft)));
        });
    });
    
    // GET craft thumbnail
    router.get("/craft/:id/thumbnail", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, thumbnail: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.set("Content-Type", "image/png");
            response.send(craft.thumbnail);
        });
    });
    
    // DELETE craft
    router.delete("/craft/:id", function (request, response) {
        //TODO: find out how to remove without selecting the removed entry
        model.Craft.findByIdAndRemove(request.params.id, { select: { _id: true } }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.status(204).end();
        });
    });
    
    // POST craft, including thumbnail and part list
    router.post("/craft", [multer({
            dest: "./uploads/",
            putSingleFilesInArray: true, // see https://www.npmjs.com/package/multer#putsinglefilesinarray
            inMemory: true,
            limits: {
                files: 2,
                fileSize: 2097152 // 2 MB
            },
            onFilesLimit: function() {
                console.log("Too many files in the request. Ignoring any further files.");
            },
            onFileUploadComplete: function (file) {
                console.log("Received a file with " + file.size + " bytes.");
                if (file.truncated) {
                    console.log(".. but the file was too large.");
                }
            }
        }),
        function (request, response) {
            if (!request.files.hasOwnProperty("thumbnail") || !request.files.hasOwnProperty("craftData")) {
                return response.status(400).end();
            }
            var thumbnail = request.files.thumbnail[0];
            var craftData = request.files.craftData[0];
            if (thumbnail.truncated || craftData.truncated) {
                // 413 Request Entity Too Large
                console.log("Request aborted.");
                return response.status(413).end();
            }
            // create craft document
            var craft = new model.Craft({
                author: request.body.author,
                craft: lzf.decompress(craftData.buffer).toString("utf8"),
                thumbnail: thumbnail.buffer
            });
            // invoke parser to add craft information
            craft.parseCraft();
            // save it to the database
            craft.save(function (error) {
                if (error) return handleError(error, response);
                // return the new item back to client with updated fields like _id and date but without the binary data fields
                var ret = craft.toObject();
                delete ret.craft;
                delete ret.thumbnail;
                response.send(ret);
            });
        }
    ]);
    
    return router;
};
