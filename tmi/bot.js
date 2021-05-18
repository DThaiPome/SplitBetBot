const tmi = require('tmi.js');
const axios = require('axios');
const https = require('https');
const {inspect} = require('util');
https.globalAgent.options.rejectUnauthorized = false;
const client = new tmi.Client({
    options: { debug: true },
    identity: {
        username: 'dpentsworth',
        password: 'oauth:7x023m9ksm0e1vyr03ha5zoxhnswqh' // TODO: Supply this some other way to keep it out of the repo
    },
    channels: [ 'DThaiPome' ]
});
const port = 44326;
const urlStart = `https://localhost:${port}/split/`;

init();

var pings = 0;

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

    client.on('ping', () => {
        client.pong(0);
    });
    
    client.say('DThaiPome', "Hello!");
    client.ping();

    await get("GetRewards").then((res) => console.log(JSON.stringify(res))).catch((err) => {});
    await post("SetBettingOpen", 
        {open: true}).then((res) => console.log(JSON.stringify(res))).catch((err) => {});

    rewardLoop('DThaiPome');
}

async function rewardLoop(channel) {
    while(true) {
        await checkForRewards(channel);
        await sleep(100);
    }
}

async function checkForRewards(channel) {
    let rewards;
    await get("GetRewards")
    .then((res) => {
        rewards = res;
    }).catch((err) => {});

    if (rewards && rewards.length > 0) {
        rewards.forEach((val) => {
            client.say(channel, `@${val.user} has won ${val.points} points!`);
        });
    }
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
    await post("AddBet", {
        user: username,
        bet: bet,
        points: points
    }).then((res) => {
        client.say(channel, res);
    }).catch((err) => {
        client.say(channel, "Could not place bet. Try again later.");
    });
}

async function get(action, args) {
    let url = generateURL(action, args);
    let response;
    await axios.get(url, {
        httpsAgent: https.Agent({rejectUnauthorized: false})
    }).then((res) => {
        console.log(`${url} - Success!`);
        response = res;
    }).catch((err) => {
        console.log(`${url} - Error: ${inspect(err)}`);
    });
    if (response) {
        return response.data;
    } else {
        throw err;
    }
}

async function post(action, args, body) {
    let url = generateURL(action, args);
    let response;
    if (!body) {
        body = {};
    }
    await axios.post(url, {
        httpsAgent: https.Agent({rejectUnauthorized: false}),
        body: body
    }).then((res) => {
        console.log(`${url} - Success!`);
        response = res;
    }).catch((err) => {
        console.log(`${url} - Error: ${inspect(err)}`);
    });
    if (response) {
        return response.data;
    } else {
        throw err;
    }
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