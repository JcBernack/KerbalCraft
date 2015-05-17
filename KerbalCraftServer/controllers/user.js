var User = require("../models/user");

function handleError(err, res) {
    console.log("Error:", err);
    res.status(err.name === "ValidationError" ? 400 : 500).end();
}

module.exports.postUser = function (req, res) {
    var user = new User({
        username: req.body.username,
        password: req.body.password
    });
    user.save(function (err) {
        if (err) return handleError(err, res);
        res.status(204).end();
    });
};

module.exports.getUser = function(req, res) {
    User.find(null, function(err, users) {
        if (err) return handleError(err, res);
        res.send(users);
    });
};
