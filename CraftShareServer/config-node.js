
// Returns a new ConfingNode object.
function ConfigNode(name) {
    return {
        name: name,
        values: {},
        nodes: []
    };
}

// Searches for the occurence of the given string and moves everything in front or behind it on separate lines.
// Returns the change in index.
function MoveToSeparateLine(lines, index, str) {
    var shift = 0;
    var num = lines[index].indexOf(str);
    // line does not contain given string or does only contain given string
    if (num === -1 || lines[index].length === str.length) return shift;
    // there is something in the line before the string
    if (num > 0) {
        // move everything in front of the string to a new line above
        lines.splice(index, 1, lines[index].substr(0, num), lines[index].substr(num));
        // we are now one line below
        index++;
        shift++;
        // and the string starts at the beginning of that line
        num = 0;
    }
    // there is something in the line after the string
    if (num + str.length < lines[index].length) {
        // move everything after the string to a new line below
        lines.splice(index, 1, lines[index].substr(0, num + str.length), lines[index].substr(num + str.length));
        shift += 2;
    }
    return shift;
}

// Prepares lines to be parsed. Splits key-value pairs, removes trailing whitespace and empty lines.
function Tokenize(lines) {
    var num;
    // iterate over the lines backwards
    var i = lines.length;
    while (--i >= 0) {
        // remove comments
        num = lines[i].indexOf("//");
        if (num === 0) {
            // remove the whole line
            lines.splice(i, 1);
            continue;
        } else if (num !== -1) {
            // remove everything after "//"
            lines[i] = lines[i].substr(0, num);
        }
        // trim whitespace
        lines[i] = lines[i].trim();
        // remove the line if it is empty now
        if (lines[i].length === 0) {
            lines.splice(i, 1);
            continue;
        }
        // make sure braces are on otherwise empty lines
        var shift = MoveToSeparateLine(lines, i, "}");
        i += shift;
        // if shifting downwards do so immediatly because there was stuff inserted we have not processed yet
        if (shift > 1) continue;
        i += MoveToSeparateLine(lines, i, "{");
    }
    // build resulting tokens
    var token = new Array(lines.length);
    // iterate over the lines forwards
    while (++i < lines.length) {
        // try to split at "="
        num = lines[i].indexOf("=");
        if (num === -1) {
            // no split
            token[i] = [lines[i]];
        } else {
            // split at num while trimming both parts
            token[i] = [lines[i].substr(0, num).trim(), lines[i].substr(num + 1).trim()];
        }
    }
    return token;
}

function Parse(tokens, index, configNode) {
    // iterate through lines
    while (index < tokens.length) {
        // parse key-value pair
        if (tokens[index].length === 2) {
            configNode.values[tokens[index][0]] = tokens[index][1];
            index++;
            continue;
        }
        // recursively read sub unnamed nodes
        if (tokens[index][0] === "{") {
            var unnamedNode = ConfigNode("");
            configNode.nodes.push(unnamedNode);
            index = Parse(tokens, index + 1, unnamedNode);
            continue;
        }
        // break condition for recursion
        if (tokens[index][0] === "}") {
            return index + 1;
        }
        // recursively read sub named nodes
        if (index + 1 < tokens.length && tokens[index + 1][0] === "{") {
            var namedNode = ConfigNode(tokens[index][0]);
            configNode.nodes.push(namedNode);
            index = Parse(tokens, index + 2, namedNode);
            continue;
        }
        // step to next line
        index++;
    }
    return index;
}

module.exports = function (input) {
    // split at linebreaks
    var lines = input.split(/\r?\n/);
    console.log("loaded " + lines.length + " lines");
    // preformat lines
    var tokens = Tokenize(lines);
    console.log("with " + tokens.length + " tokens");
    // parse ConfigNodes recursively
    var configNode = ConfigNode("");
    Parse(tokens, 0, configNode);
    return configNode;
};
