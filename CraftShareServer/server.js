var https = require("https");
var fs = require("fs");
var express = require("express");
var bodyParser = require("body-parser");
var mongoose = require("mongoose");
var compress = require("compression");
var api = require("./api");

// log uncaught exceptions, but stop the application from crashing
process.on("uncaughtException", function (error) {
    console.log("Caught exception: " + error);
});

// connect to database
//mongoose.set("debug", true);
mongoose.connect("localhost", "craftshare");

mongoose.connection.on("error", function (error) {
    console.log("MongoDB error: " + error);
});

mongoose.connection.once("open", function () {
    console.log("Connected to MongoDB");
});

// create express server
var app = express();
//app.use(compress());  // causes error with RestSharp under Mono :-(
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

// add the rest router
app.use("/api", api(express.Router()));

// add middleware hide errors from the client
app.use(function(error, request, response, next) {
    console.error(error);
    response.status(error.status).end();
});

// load ssl certificate
var credentials = {
    key: fs.readFileSync("file.pem"),
    cert: fs.readFileSync("file.crt")
};
console.log("Certificate loaded");

// supply passphrase if given
if (process.argv.length > 2) {
    console.log("Using the given passphrase");
    credentials.passphrase = process.argv[2];
}

// start the server
var port = process.env.PORT || 8000;
var server = https.createServer(credentials, app);
server.listen(port, function() {
    console.log("listening on port " + port);
});
