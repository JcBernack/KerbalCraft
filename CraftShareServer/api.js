var fs = require("fs");
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
        model.Craft.find(null, { craft: false, thumbnail: false, __v: false }, { sort: { date: -1 }, skip: skip, limit: limit }, function (error, crafts) {
            if (error) return handleError(error, response);
            response.send(crafts);
        });
    });
    
    // GET craft data
    router.get("/craft/data/:id", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, craft: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.set("Content-Type", "text/plain");
            response.send(craft.craft);
        });
    });
    
    // GET craft thumbnail
    router.get("/craft/thumbnail/:id", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, thumbnail: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.set("Content-Type", "image/png");
            response.send(craft.thumbnail);
        });
    });
    
    // DELETE craft
    router.delete("/craft/:id", function (request, response) {
        model.Craft.findByIdAndRemove(request.params.id, { select: { craft: false, thumbnail: false, __v: false } }, function (error, craft) {
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
            // create craft document and save it to the database
            model.Craft.create({
                name: request.body.name,
                facility: request.body.facility,
                author: request.body.author,
                craft: craftData.buffer,
                thumbnail: thumbnail.buffer
            }, function (error, craft) {
                if (error) return handleError(error, response);
                // return it back to client on success with updated fields like _id and date but without the binary data fields
                var ret = craft.toObject();
                delete ret.craft;
                delete ret.thumbnail;
                response.send(ret);
            });
        }
    ]);
    
    return router;
};
