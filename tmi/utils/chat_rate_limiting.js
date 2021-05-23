const tmi = require('tmi.js');

// Every message: { channel, message, delay } 
var messages = [];
var timeLastSent = 0;
const msPerMessage = 1750;

module.exports.say = function (channel, message, delay) {
    messages.push({ 
        channel: channel, 
        message: message,
        delay: delay});
}

module.exports.flushChat = function (client) {
    const now = Date.now();
    if (now - timeLastSent > msPerMessage && messages.length > 0) {
        timeLastSent = now;
        message = messages.shift();
        sayWithDelay(client, message.channel, message.message, message.delay);
    }
}

async function sayWithDelay(client, channel, message, delay) {
    if (delay) {
        await sleep(delay);
    }
    client.say(channel, message);
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}