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
        model.Craft.find(null, { craft: false, __v: false }, { sort: { date: -1 }, skip: skip, limit: limit }, function (error, crafts) {
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
    router.post("/craft", function (request, response) {
        // prevent manipulating the _id or date field
        delete request.body._id;
        delete request.body.date;
        // create craft document and save it to the database
        model.Craft.create(request.body, function (error, craft) {
            if (error) return handleError(error, response);
            // return it back to client on success with updated fields like _id and date
            response.send(craft);
        });
    });

    return router;
};

