// Returns a new ConfingNode object.
function ConfigNode(name) {

    this.name = name;
    this.values = {};
    this.nodes = [];

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
}

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

function Parse(lines, index, configNode) {
    // iterate through lines
    while (index < lines.length) {
        var line = lines[index];
        // parse key-value pair
        var num = line.indexOf("=");
        if (num !== -1) {
            configNode.addValue(line.substr(0, num).trim(), line.substr(num + 1).trim());
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
            configNode.addNode(unnamedNode);
            index = Parse(lines, index + 1, unnamedNode);
            continue;
        }
        // recursively read named sub nodes
        if (index + 1 < lines.length && lines[index + 1] === "{") {
            var namedNode = new ConfigNode(line);
            configNode.addNode(namedNode);
            index = Parse(lines, index + 2, namedNode);
            continue;
        }
        // step to next line
        index++;
    }
    return index;
};

module.exports = {
    ConfigNode: ConfigNode,
    // Parses the given input string to ConfigNode object structure
    load: function(input) {
        // split at linebreaks
        var lines = input.split(/\r?\n/);
        // preprocess lines
        TrimCommentsAndWhitespace(lines);
        //console.log("Parsing " + lines.length + " lines...");
        // parse ConfigNodes
        var configNode = new ConfigNode("");
        Parse(lines, 0, configNode);
        // allow chaining
        return configNode;
    }
};