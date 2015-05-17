var multer = require("multer");

module.exports.createInMemoryMulter = function(limitFiles, limitFileSize) {
    return multer({
        dest: "./uploads/",
        putSingleFilesInArray: true, // see https://www.npmjs.com/package/multer#putsinglefilesinarray
        inMemory: true,
        limits: {
            files: limitFiles,
            fileSize: limitFileSize
        },
        onFilesLimit: function() {
            console.log("Too many files in the request. Ignoring any further files.");
        },
        onFileUploadComplete: function(file) {
            console.log("Received a file with " + file.size + " bytes.");
            if (file.truncated) {
                console.log(".. but the file was too large.");
            }
        }
    });
};