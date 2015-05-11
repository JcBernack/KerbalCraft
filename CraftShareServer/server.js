var util = require("util");
var https = require("https");
var fs = require("fs");
var express = require("express");
var bodyParser = require("body-parser");
var helmet = require("helmet");
//var compress = require("compression");
var mongoose = require("mongoose");
var argv = require("minimist")(process.argv.slice(2));
var api = require("./api");

function printUsage() {
    console.log("Usage:");
    console.log("-p # or --port # Specify the port to listen on.");
    console.log("-d # or --database # Specify the database name to use, default is \"craftshare\"");
    console.log("-s # or --passphrase # Specify the passphrase for the certificate.");
    process.exit(0);
}

if (argv.h || argv.help) {
    printUsage();
}

if (!argv.p && !argv.port) {
    console.log("Must specify a port with -p # or --port #");
    process.exit(0);
}

// connect to database
//mongoose.set("debug", true);
mongoose.connect("localhost", argv.d || argv.database || "craftshare");

mongoose.connection.on("error", function (error) {
    console.log("MongoDB error: " + error);
});

mongoose.connection.once("open", function () {
    console.log("Connected to MongoDB");
});

// create express server
var app = express();
app.use(helmet());
//app.use(compress());  // causes error under Unity's version of Mono: "EntryPointNotFoundException : CreateZStream"

// add middleware to log the last request in raw format
//TODO: find out how to access the raw headers
//app.use(function (request, response, next) {
//    var rawBody = "";
//    request.on("data", function (chunk) {
//        rawBody += chunk;
//    });
//    request.on("end", function () {
//        fs.writeFile("request.txt", util.inspect(request.headers) + "\r\n" + rawBody);
//    });
//    next();
//});

app.use(bodyParser.json());

// add middleware to log all api requests to the console
app.use(function(request, response, next) {
    // log http method and url
    console.log(request.method, request.originalUrl);
    //console.log(request.headers);
    // log request body
    //console.dir(request.body);
    // monkey patch the response.end function to log all responses
    var originalEnd = response.end;
    response.end = function () {
        var value = originalEnd.apply(this, arguments);
        console.log("Status", response.statusCode);
        return value;
    }
    next();
});

// add the rest router
app.use("/api", api(express.Router()));

//TODO: have a look at proper express error handling
// add middleware hide errors from the client
//app.use(function(error, request, response, next) {
//    console.error(error);
//    response.status(error.status).end();
//});

// load ssl certificate
var credentials = {
    key: fs.readFileSync("file.pem"),
    cert: fs.readFileSync("file.crt")
};
console.log("Certificate loaded");

// supply passphrase if given
if (argv.s || argv.passphrase) {
    console.log("Using the given passphrase");
    credentials.passphrase = argv.s || argv.passphrase;
}

// start the server
var port = argv.p || argv.port;
var server = https.createServer(credentials, app);
server.listen(port, function() {
    console.log("listening on port " + port);
});

// log uncaught exceptions after successful initialization, but stop the application from crashing while the server is up
process.on("uncaughtException", function (error) {
    console.log("Caught exception: " + error);
});
