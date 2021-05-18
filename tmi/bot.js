const tmi = require('tmi.js');
const fetch = require("node-fetch");
const client = new tmi.Client({
    options: { debug: true },
    identity: {
        username: 'dpentsworth',
        password: 'oauth:7x023m9ksm0e1vyr03ha5zoxhnswqh'
    },
    channels: [ 'DThaiPome' ]
});
const port = 5001;
const urlStart = `https://localhost:${port}/split/`;

init();

async function init () {
    await client.connect();
    
    client.on('message', async (channel, tags, message, self) => {
        // Ignore echoed messages.
        if(self) return;
    
        let tokens = message.split(' ');
        if (tokens[0] && tokens[0] == "!d") {
            tokens.shift();
            handleCommand(tags.username, tokens, channel);
        }
    });
    
    client.on('pong', async (latency) => {
        console.log("Pong!");
        await sleep(5000);
        client.ping();
    });
		
    client.say('DThaiPome', "Hello!");
    client.ping();
}

// Assume at least 1 argument
async function handleCommand(username, args, channel) {
    switch(args[0]) {
        case "bet":
            if (args.length == 3 &&
                validateBet(args[1]) &&
                parseInt(args[2])) {
                addBet(username, args[1], parseInt(args[2]), channel);
            } else {
                client.say(channel, "Invalid bet input! Usage: !d bet [behind/tied/ahead/gold] [point amount]");
            }
            break;
    }
}

function validateBet(bet) {
    return bet == "behind" || bet == "tied" || bet == "ahead" || bet == "gold";
}

// string, number, channel
async function addBet(username, bet, points, channel) {
    const url = generateURL("AddBet", {
        user: username,
        bet: bet,
        points: points
    });
    let response = await fetch(url, {
        method: 'POST',
        mode: 'same-origin',
        credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json'
        },
        body: {}
    }).catch((err) => {
        console.log(JSON.stringify(err));
    });
    console.log(JSON.stringify(response));
}

// string, json object
function generateURL(action, args) {
    let argStrs = [];
    for(var key in args) {
        argStrs.push(`${key}=${args[key]}`);
    }
    return `${urlStart}${action}?${argStrs.join("&")}`;
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}