const tmi = require('tmi.js');
const axios = require('axios');
const https = require('https');
const {inspect} = require('util');
const auth = require('./private/auth.js');
const {say, flushChat} = require('./utils/chat_rate_limiting.js');
https.globalAgent.options.rejectUnauthorized = false;
const client = new tmi.Client({
    options: { debug: true },
    identity: {
        username: 'dpentsworth',
        password: auth.oauth // TODO: Supply this some other way to keep it out of the repo
    },
    channels: [ 'DThaiPome' ]
});
const urlStart = auth.api;

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
        client.ping();
    });
    
    say('DThaiPome', "Hello!");
    rewardLoop('DThaiPome');
}

async function rewardLoop(channel) {
    while(true) {
        await checkForRewards(channel);
        flushChat(client);
        await sleep(100);
    }
}

async function checkForRewards(channel) {
    let rewards;
    await get("GetRewards")
    .then((res) => {
        if (res.error === 0) {
            rewards = res.rewards;
        }
    }).catch((err) => {});

    if (rewards) {
        giveRewards(channel, rewards);
    }
}

async function giveRewards(channel, rewards) {
    let count = rewards.length;
    if (count == 0) {
        say(channel, "There were no winners this split");
    } else {
        say(channel, `${count} player${count == 1 ? "" : "s"} ha${count == 1 ? "s" : "ve"} won the pot!`);
    }
    rewards.forEach((val) => {
        //say(channel, `@${val.user} has won ${val.points} points!`);
        addUserPoints(val.user, val.points);
    });
}

// Assume at least 1 argument
async function handleCommand(username, args, channel) {
    switch(args[0]) {
        case "bet":
            if (args.length == 3 &&
                parseInt(args[2])) {
                addBet(username, args[1], parseInt(args[2]), channel);
            } else {
                say(channel, `@${username} Usage: !d bet [behind/tied/ahead/gold] [point amount]`);
            }
            break;
    }
}

// string, number, channel
async function addBet(username, bet, points, channel) {
    await post("AddBet", {
        user: username,
        bet: bet,
        points: points
    }).then((res) => {
        if (res.error === 0) {
            //say(channel, `@${username} has bet ${points} points on ${bet}!`);
            addUserPoints(username, -points);
        } else {
            let msg;
            switch(res.error) {
                case 1:
                    msg = "You cannot place bets right now!";
                    break;
                case 2:
                    msg = "You have already placed your bets!";
                    break;
                case 3:
                    min = res.points;
                    msg = `You must bet at least ${min} points!`;
                    break;
                case 4:
                    msg = `\"${bet}\" is not a valid bet. Choose one of \"behind\", \"tied\", \"ahead\", or \"gold\"`;
                    break;
                default:
                    msg = "Could not place bet. Try again later."
                    break;
            }
            msg = `@${username} ${msg}`;
            say(channel, msg);
        }
    }).catch((err) => {
        say(channel, `@${username} Could not place bet. Try again later.`);
    });
}

async function get(action, args) {
    let url = generateURL(action, args);
    let response;
    await axios.get(url, {
        httpsAgent: https.Agent({rejectUnauthorized: false})
    }).then((res) => {
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

async function addUserPoints(username, points) {
    const url = `https://api.streamelements.com/kappa/v2/points/${auth.channel_id}/${username}/${points}`
    const options = {
        method: 'PUT',
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${auth.streamelements_jwt}`
        }
    };

    await axios(url, options)
    .then((res) => {
        console.log(`Added ${points} points to ${username}'s account`);
    }).catch((err) => {
        console.error(`Point transfer failed at ${url}: ${JSON.stringify(err)}`);
    });
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