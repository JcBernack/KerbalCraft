var express = require("express");
var bodyParser = require("body-parser");
var mongoose = require("mongoose");

mongoose.set("debug", true);
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
    name: { type: String, required: true },
    author: { type: String, required: true },
    date: { type: Date, default: Date.now },
    craft: { type: String, required: true },
    thumbnail: { type: String }
});

var CraftModel = mongoose.model("Craft", CraftSchema);

// create express http server
var app = express();
app.use(bodyParser.json());

// add middleware to log all api requests to the console
app.use(function(request, response, next) {
    // log http method and url
    console.log(request.method, request.originalUrl);
    // log request body
    console.dir(request.body);
    next();
});

// define rest routes
var baseUrl = "/api";

function handleError(error, response) {
    console.log("Error:", error);
    response.status(error.name === "ValidationError" ? 400 : 500).end();
}

// GET craft list, without thumbnail and part list
app.get(baseUrl + "/crafts", function (request, response) {
    //TODO: paging
    // query crafts from the database
    CraftModel.find(null, { craft: false, thumbnail: false }).sort({ date: "-1" }).limit(5).exec(function (error, objects) {
        if (error) return handleError(error, response);
        // return crafts to the client
        response.send(objects);
    });
});

// GET craft data, including thumbnaul and part list
app.get(baseUrl + "/craft/:id", function (request, response) {
    // query crafts from the database
    CraftModel.findById(request.params.id, function (error, objects) {
        if (error) return handleError(error, response);
        if (!objects) return response.status(404).end();
        // return craft to the client
        response.send(objects);
    });
});

// POST craft including part list and encoded thumbnail
app.post(baseUrl + "/craft", function (request, response) {
    // prevent manipulating the _id or date field
    delete request.body._id;
    delete request.body.date;
    // create craft document
    var object = new CraftModel(request.body);
    // save it to the database
    object.save(function (error) {
        if (error) return handleError(error, response);
        // return it back to client on success with updated fields like _id and date
        response.send(object);
    });
});

//TODO: remove __v from response { __v: 0 }

// start the server
var port = process.env.PORT || 8000;
var server = app.listen(port, function() {
    console.log("listening on port " + port);
});
