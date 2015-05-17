var User = require("../models/user");

function handleError(err, res) {
    console.log("Error:", err);
    res.status(err.name === "ValidationError" ? 400 : 500).end();
}

module.exports.postUser = function (req, res) {
    User.findOne({ username: req.body.username }, { username: true, password: true }, function (err, user) {
        if (err) return handleError(err);
        if (user) {
            user.verifyPassword(req.body.password, function (err, matched) {
                if (err) return handleError(err);
                // wrong password, i.e. the user is already existing
                // but with a different password than the one given
                if (!matched) return res.status(400).end();
                // user credentials ok
                res.status(204).end();
            });
        } else {
            // user not existing, create it
            user = new User({
                username: req.body.username,
                password: req.body.password
            });
            user.save(function (err) {
                if (err) return handleError(err, res);
                res.status(204).end();
            });
        }
    });
};

module.exports.getUser = function(req, res) {
    User.find(null, function(err, users) {
        if (err) return handleError(err, res);
        res.send(users);
    });
};
