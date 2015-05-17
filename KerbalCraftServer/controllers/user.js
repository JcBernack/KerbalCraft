var User = require("../models/user");

function handleError(err, res) {
    console.log("Error:", err);
    res.status(err.name === "ValidationError" ? 400 : 500).end();
}

module.exports.postUser = function (req, res) {
    var validation = User.validate(req.body.username, req.body.password);
    if (!validation.valid) {
        return res.status(400).send({ message: validation.message });
    }
    // try to find a user with the given username
    User.findOne({ username: req.body.username }, { username: true, password: true }, function (err, user) {
        if (err) return handleError(err);
        if (user) {
            // user found, verify password
            user.verifyPassword(req.body.password, function (err, matched) {
                if (err) return handleError(err);
                // wrong password, i.e. the user is already existing
                // but with a different password than the one given
                if (!matched) return res.status(400).send({ message: "Login failed" });
                // user credentials ok
                if (!req.body.newPassword) {
                    return res.status(200).send({ message: "Success, login ok" });
                }
                // the user may want to change the password
                validation = User.validate(req.body.username, req.body.newPassword);
                if (!validation.valid) {
                    return res.status(400).send({ message: validation.message });
                }
                user.password = req.body.newPassword;
                user.save(function (err) {
                    if (err) return handleError(err, res);
                    return res.status(200).send({ message: "Success, password changed" });
                });
            });
        } else {
            // user not existing, create it
            user = new User({
                username: req.body.username,
                password: req.body.password
            });
            user.save(function (err) {
                if (err) return handleError(err, res);
                return res.status(200).send({ message: "Success, user created" });
            });
        }
    });
};

//module.exports.getUser = function(req, res) {
//    User.find(null, function(err, users) {
//        if (err) return handleError(err, res);
//        res.send(users);
//    });
//};
