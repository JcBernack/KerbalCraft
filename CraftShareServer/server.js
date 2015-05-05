var express = require("express");
var bodyParser = require("body-parser");
var mongoose = require("mongoose");
var compress = require("compression");

//mongoose.set("debug", true);
mongoose.connect("localhost", "craftshare");

var db = mongoose.connection;

db.on("error", function (error) {
    console.log("MongoDB error: " + error);
});

db.once("open", function () {
    console.log("Connected to MongoDB");
});

var ObjectId = mongoose.Schema.Types.ObjectId;

var CraftSchema = mongoose.Schema({
    date: { type: Date, default: Date.now },
    name: { type: String, required: true },
    facility: { type: String, required: true },
    author: { type: String, required: true },
    craft: { type: String, required: true }
});

var CraftModel = mongoose.model("Craft", CraftSchema);

// create express http server
var app = express();
//app.use(compress());
app.use(bodyParser.json());

// add middleware to log all api requests to the console
app.use(function(request, response, next) {
    // log http method and url
    console.log(request.method, request.originalUrl);
    //console.log(request.headers);
    // log request body
    //console.dir(request.body);
    next();
});

process.on("uncaughtException", function (error) {
    console.log("Caught exception: " + error);
});

function handleError(error, response) {
    console.log("Error:", error);
    response.status(error.name === "ValidationError" ? 400 : 500).end();
}

// define REST routes
var router = express.Router();

function queryInt(request, name, standard, min, max) {
    if (!request.query.hasOwnProperty(name)) return standard;
    var value = parseInt(request.query[name]);
    if (isNaN(value)) value = standard;
    if (value < min) value = min;
    if (value > max) value = max;
    return value;
}

// GET craft list, without thumbnail and part list
router.get("/craft", function (request, response) {
    var skip = queryInt(request, "skip", 0, 0, Number.MAX_VALUE);
    var limit = queryInt(request, "limit", 20, 1, 50);
    CraftModel.find(null, { craft: false, __v: false }, { sort: { date: -1 }, skip: skip, limit: limit }, function (error, crafts) {
        if (error) return handleError(error, response);
        if (!crafts || crafts.length < 1) return response.status(404).end();
        response.send(crafts);
    });
});

// GET craft data
router.get("/craft/:id", function (request, response) {
    CraftModel.findById(request.params.id, { _id: false, craft: true }, function (error, craft) {
        if (error) return handleError(error, response);
        if (!craft) return response.status(404).end();
        response.send(craft);
    });
});

// DELETE craft
router.delete("/craft/:id", function (request, response) {
    CraftModel.findByIdAndRemove(request.params.id, { select: { craft: false, __v: false } }, function (error, craft) {
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
    CraftModel.create(request.body, function(error, craft) {
        if (error) return handleError(error, response);
        // return it back to client on success with updated fields like _id and date
        response.send(craft);
    });
});

// enable the rest router
app.use("/api", router);

// hide errors from the client
app.use(function(error, request, response, next) {
    console.error(error);
    response.status(error.status).end();
});

// start the server
var port = process.env.PORT || 8000;
var server = app.listen(port, function() {
    console.log("listening on port " + port);
});
