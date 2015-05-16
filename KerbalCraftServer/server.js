var argv = require("minimist")(process.argv.slice(2));
var util = require("util");
var https = require("https");
var fs = require("fs");
var express = require("express");
var bodyParser = require("body-parser");
var helmet = require("helmet");
//var compress = require("compression");
var mongoose = require("mongoose");
var craftController = require("./controllers/craft");
var uploadController = require("./controllers/upload");

if (argv.h || argv.help) {
    console.log("Usage:");
    console.log("-p # or --port # Specify the port to listen on.");
    console.log("-d # or --database # Specify the database name to use, default is \"kerbalcraft\"");
    console.log("-s # or --passphrase # Specify the passphrase for the certificate.");
    console.log("-h # or --help # Displays this message.");
    process.exit(0);
}

if (!argv.p && !argv.port) {
    console.log("Must specify a port with -p # or --port #");
    process.exit(0);
}

// connect to database
//mongoose.set("debug", true);
mongoose.connect("localhost", argv.d || argv.database || "kerbalcraft");

mongoose.connection.on("error", function (err) {
    console.log("MongoDB error: " + err);
});

mongoose.connection.once("open", function () {
    console.log("Connected to MongoDB");
});

// create express server
var app = express();
app.use(helmet());
//app.use(compress());  // causes error under Unity's version of Mono: "EntryPointNotFoundException : CreateZStream"
app.use(bodyParser.json());

// add middleware to log all api requests to the console
app.use(function(req, res, next) {
    // log http method and url
    console.log(req.method, req.originalUrl);
    // log request body
    //console.dir(req.body);
    // monkey patch the response.end function to log all responses
    var originalEnd = res.end;
    res.end = function () {
        var value = originalEnd.apply(this, arguments);
        console.log("Status", res.statusCode);
        return value;
    }
    next();
});

// add the rest router
var router = express.Router();
router.route("/craft")
    .post(uploadController.createInMemoryMulter(2, 2097152), craftController.postCraft)
    .get(craftController.getCraft);
router.route("/craft/:id/thumbnail")
    .get(craftController.getCraftThumbnail);
router.route("/craft/:id/data")
    .get(craftController.getCraftData);
router.route("/craft/:id")
    .delete(craftController.deleteCraft);
app.use("/api", router);

//TODO: have a look at proper express error handling
// add middleware hide errors from the client
//app.use(function(err, req, res, next) {
//    console.error(err);
//    res.status(err.status).end();
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
process.on("uncaughtException", function (err) {
    console.log("Caught exception: " + err);
});
