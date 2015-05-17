var mongoose = require("mongoose");
var bcrypt = require("bcrypt-nodejs");

var UserSchema = new mongoose.Schema({
    username: { type: String, required: true, unique: true },
    password: { type: String, required: true, select: false }
});

UserSchema.pre("save", function(next) {
    // break out if the password hasn't changed
    if (!this.isModified("password")) return next();
    // password changed so we need to hash it
    var user = this;
    bcrypt.genSalt(5, function(err, salt) {
        if (err) return next(err);

        bcrypt.hash(user.password, salt, null, function(err, hash) {
            if (err) return next(err);
            user.password = hash;
            next();
        });
    });
});

UserSchema.methods.verifyPassword = function (password, cb) {
    bcrypt.compare(password, this.password, function (err, isMatch) {
        if (err) return cb(err);
        cb(null, isMatch);
    });
};

UserSchema.statics.validate = function (username, password) {
    // validate username
    if (username.length < 4) {
        return { valid: false,  message: "Username must be at least 4 characters long" };
    }
    if (username !== username.trim()) {
        return { valid: false, message: "Username must not start or end with whitespace" };
    }
    // validate password
    if (password.length < 6) {
        return { valid: false, message: "Password must be at least 6 characters long" };
    }
    return { valid: true, message: "" };
};

module.exports = mongoose.model("User", UserSchema);

module.exports.on("index", function (err) {
    if (err) console.error(err);
})