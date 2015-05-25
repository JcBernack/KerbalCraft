var argv = require("minimist")(process.argv.slice(2));
var mongoose = require("mongoose");
var Craft = require("./models/craft.js");

if (argv.h || argv.help) {
    console.log("Usage:");
    console.log("-d # or --database # Specify the database name to use, default is \"kerbalcraft\"");
    process.exit(0);
}

// connect to database
mongoose.connect("localhost", argv.d || argv.database || "kerbalcraft");

mongoose.connection.on("error", function (err) {
    console.log("MongoDB error: " + err);
});

mongoose.connection.once("open", function () {
    console.log("Connected to MongoDB");
});

function handleError(err) {
    console.log("Error", err);
}

// iterate over all craft in the database
Craft.find(null, { craft: true }, function (err, crafts) {
    if (err) {
        console.log("Error retrieving data from database.");
        return;
    }
    for (var i = 0; i < crafts.length; i++) {
        //TODO: check if that ugly version of \r also works under unix
        process.stdout.write("Processing craft " + (i+1) + "/" + crafts.length + ": " + crafts[i]._id + "\033[0G");
        try {
            // invoke parser to update craft information
            crafts[i].parseCraft();
            // save to database
            crafts[i].save(function(err, craft) {
                if (err) {
                    console.log("Error saving craft: " + craft._id + ":");
                    console.log(err);
                }
            });
        } catch (ex) {
            console.log("Error processing craft " + (i+1) + "/" + crafts.length + ": " + crafts[i]._id);
            console.log(ex);
        }
    }
    console.log("\ndone");
});
