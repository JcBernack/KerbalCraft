var fs = require("fs");
var multer = require("multer");
var autoreap = require("./multer-autoreap");
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
            if (!crafts || crafts.length < 1) return response.status(404).end();
            response.send(crafts);
        });
    });
    
    // GET craft data
    router.get("/craft/:id", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, craft: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.send(craft);
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

    // POST craft including part list and encoded thumbnail
    router.post("/craft", [multer({
            dest: "./uploads/",
            putSingleFilesInArray: true, // see https://www.npmjs.com/package/multer#putsinglefilesinarray
            //inMemory: true, //TODO: try this out
            limits: {
                files: 1,
                fileSize: 2097152 // 2 MB
            },
            onFilesLimit: function() {
                console.log("Too many files in the request. Ignoring any further files.");
            },
            onFileUploadComplete: function (file, request, response) {
                console.log("Received a file with " + file.size + " bytes.");
                if (file.truncated) {
                    console.log(".. but the file was too large.");
                }
            }
        }),
        // automatically delete all temporary files after the request was handled
        autoreap,
        function (request, response) {
            if (!request.files.hasOwnProperty("thumbnail")) {
                return response.status(400).end();
            }
            var thumbnail = request.files.thumbnail[0];
            if (thumbnail.truncated) {
                // 413 Request Entity Too Large
                console.log("Request aborted.");
                return response.status(413).end();
            }
            // prevent manipulating the _id or date field
            delete request.body._id;
            delete request.body.date;
            // load thumbnail file and add it the database
            // create craft document and save it to the database
            var input = request.body;
            fs.readFile(thumbnail.path, function (error, data) {
                if (error) return handleError(error, response);
                input.thumbnail = data;
                model.Craft.create(input, function (error, craft) {
                    if (error) return handleError(error, response);
                    // return it back to client on success with updated fields like _id and date but without the binary data fields
                    var ret = craft.toObject();
                    delete ret.craft;
                    delete ret.thumbnail;
                    response.send(ret);
                });
            });
        }
    ]);

    // GET craft thumbnail
    router.get("/craft/thumbnail/:id", function (request, response) {
        model.Craft.findById(request.params.id, { _id: false, thumbnail: true }, function (error, craft) {
            if (error) return handleError(error, response);
            if (!craft) return response.status(404).end();
            response.set("Content-Type", "image/png");
            response.send(craft.thumbnail);
        });
    });

    return router;
};
