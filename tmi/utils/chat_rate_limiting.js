const tmi = require('tmi.js');

// Every message: { channel, message, delay } 
var messages = [];
var whispers = [];
var timeLastSent = 0;
var timeLastWhispered = 0;
const msPerMessage = 1750;
const msPerWhisper = 3500;

module.exports.say = function (channel, message, delay) {
    messages.push({ 
        channel: channel, 
        message: message,
        delay: delay});
}

module.exports.whisper = function (username, message, delay) {
    whispers.push({
        username: username,
        message: message,
        delay: delay
    });
}

module.exports.flushChat = async function (client) {
    let state = await client.readyState();
    if (state !== "OPEN") {
        return;
    }
    const now = Date.now();
    if (now - timeLastSent > msPerMessage && messages.length > 0) {
        timeLastSent = now;
        message = messages.shift();
        sayWithDelay(client, message.channel, message.message, message.delay);
    }
}

module.exports.flushWhispers = async function (client) {
    let state = await client.readyState();
    if (state !== "OPEN") {
        return;
    }
    const now = Date.now();
    if (now - timeLastWhispered > msPerWhisper && whispers.length > 0) {
        timeLastWhispered = now;
        whisper = whispers.shift();
        whisperWithDelay(client, whisper.username, whisper.message, whisper.delay);
    }
}

async function sayWithDelay(client, channel, message, delay) {
    if (delay) {
        await sleep(delay);
    }
    client.say(channel, message);
}

async function whisperWithDelay(client, username, message, delay) {
    if (delay) {
        await sleep(delay);
    }
    client.whisper(username, message);
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}