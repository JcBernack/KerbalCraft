var argv = require("minimist")(process.argv.slice(2));
var mongoose = require("mongoose");
var model = require("./model.js");

if (argv.h || argv.help) {
    console.log("Usage:");
    console.log("-d # or --database # Specify the database name to use, default is \"craftshare\"");
    process.exit(0);
}

// connect to database
mongoose.connect("localhost", argv.d || argv.database || "craftshare");

mongoose.connection.on("error", function (error) {
    console.log("MongoDB error: " + error);
});

mongoose.connection.once("open", function () {
    console.log("Connected to MongoDB");
});

function handleError(error) {
    console.log("Error", error);
}

// iterate over all craft in the database
model.Craft.find(null, { craft: true }, function (error, crafts) {
    if (error) {
        console.log("Error retrieving data from database.");
        return;
    }
    for (var i = 0; i < crafts.length; i++) {
        //TODO: check if that ugly version \r also works under unix
        process.stdout.write("Processing craft " + (i+1) + "/" + crafts.length + ": " + crafts[i]._id + "\033[0G");
        try {
            // invoke parser to update craft information
            crafts[i].parseCraft();
            // save to database
            crafts[i].save(function(error, craft) {
                if (error) {
                    console.log("Error saving craft: " + craft._id + ":");
                    console.log(error);
                }
            });
        } catch (ex) {
            console.log("Error processing craft " + (i+1) + "/" + crafts.length + ": " + crafts[i]._id);
            console.log(ex);
        }
    }
    console.log("\ndone");
});
