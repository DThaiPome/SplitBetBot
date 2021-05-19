const tmi = require('tmi.js');

// Every message: { channel, message } 
var messages = [];
var timeLastSent = 0;
const msPerMessage = 1750;

module.exports.say = function (channel, message) {
    messages.push({ 
        channel: channel, 
        message: message});
}

module.exports.flushChat = function (client) {
    const now = Date.now();
    if (now - timeLastSent > msPerMessage && messages.length > 0) {
        timeLastSent = now;
        message = messages.shift();
        client.say(message.channel, message.message);
    }
}