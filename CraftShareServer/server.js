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

// define rest routes
var baseUrl = "/api";

process.on("uncaughtException", function (error) {
    console.log("Caught exception: " + error);
});

function handleError(error, response) {
    console.log("Error:", error);
    response.status(error.name === "ValidationError" ? 400 : 500).end();
}

// GET craft list, without thumbnail and part list
app.get(baseUrl + "/crafts", function (request, response) {
    //TODO: paging
    CraftModel.find(null, { craft: false, __v: false }).sort({ date: "-1" }).limit(20).exec(function (error, crafts) {
        if (error) return handleError(error, response);
        response.send(crafts);
    });
});

// GET craft data
app.get(baseUrl + "/craft/:id", function (request, response) {
    CraftModel.findById(request.params.id, { _id: false, craft: true }, function (error, craft) {
        if (error) return handleError(error, response);
        if (!craft) return response.status(404).end();
        response.send(craft);
    });
});

// DELETE craft
app.delete(baseUrl + "/craft/:id", function (request, response) {
    CraftModel.findByIdAndRemove(request.params.id, { select: { craft: false, __v: false } }, function (error, craft) {
        if (error) return handleError(error, response);
        if (!craft) return response.status(404).end();
        response.status(204).end();
    });
});

// POST craft including part list and encoded thumbnail
app.post(baseUrl + "/craft", function (request, response) {
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

// start the server
var port = process.env.PORT || 8000;
var server = app.listen(port, function() {
    console.log("listening on port " + port);
});
