// Prepares lines to be parsed. Removes comments, trailing whitespace and empty lines.
function TrimCommentsAndWhitespace(lines) {
    var num;
    // iterate over the lines backwards because lines may be deleted
    var i = lines.length;
    while (--i >= 0) {
        // get line from the array
        var line = lines[i];
        // remove everything after "//"
        num = line.indexOf("//");
        if (num !== -1) line = line.substr(0, num);
        // trim whitespace and put it back in the array
        lines[i] = line.trim();
        // remove the line if it is empty now
        if (line.length === 0) lines.splice(i, 1);
    }
}

// Returns a new ConfingNode object.
function ConfigNode(name) {

    this.name = name;
    this.values = {};
    this.nodes = [];

    this._parse = function(lines, index) {
        // iterate through lines
        while (index < lines.length) {
            var line = lines[index];
            // parse key-value pair
            var num = line.indexOf("=");
            if (num !== -1) {
                this.addValue(line.substr(0, num).trim(), line.substr(num + 1).trim());
                index++;
                continue;
            }
            // break condition for recursion
            if (line === "}") {
                return index + 1;
            }
            // recursively read unnamed sub nodes
            if (line === "{") {
                var unnamedNode = new ConfigNode("");
                this.addNode(unnamedNode);
                index = unnamedNode._parse(lines, index + 1);
                continue;
            }
            // recursively read named sub nodes
            if (index + 1 < lines.length && lines[index + 1] === "{") {
                var namedNode = new ConfigNode(line);
                this.addNode(namedNode);
                index = namedNode._parse(lines, index + 2);
                continue;
            }
            // step to next line
            index++;
        }
        return index;
    };

    this.addValue = function(name, value) {
        this.values[name] = value;
    };
    
    this.addNode = function(configNode) {
        this.nodes.push(configNode);
    }
    
    this.countByName = function (name) {
        var n = 0;
        for (var i = 0; i < this.nodes.length; i++) {
            if (this.nodes[i].name === name) n++;
        }
        return n;
    };
    
    // Parses the given input string to ConfigNode object structure
    this.load = function (input) {
        // split at linebreaks
        var lines = input.split(/\r?\n/);
        // preprocess lines
        TrimCommentsAndWhitespace(lines);
        console.log("Parsing " + lines.length + " lines...");
        // parse ConfigNodes
        this._parse(lines, 0);
        // allow chaining
        return this;
    };
}

module.exports = ConfigNode;
