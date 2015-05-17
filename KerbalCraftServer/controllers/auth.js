var passport = require("passport");
var BasicStrategy = require("passport-http").BasicStrategy;
var User = require("../models/user");

passport.use(new BasicStrategy(
    function (username, password, callback) {
        User.findOne({ username: username }, { username: true, password: true }, function (err, user) {
            if (err) return callback(err);
            // user not found
            if (!user) return callback(null, false);
            // user found, check password
            user.verifyPassword(password, function (err, matched) {
                if (err) return callback(err);
                // wrong password
                if (!matched) return callback(null, false);
                // user credentials ok
                return callback(null, user);
            });
        });
    }
));

module.exports.isAuthenticated = passport.authenticate("basic", { session : false });
