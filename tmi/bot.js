const tmi = require('tmi.js');
const axios = require('axios');
const https = require('https');
const {inspect} = require('util');
const {say, flushChat} = require('./utils/chat_rate_limiting.js');
require('dotenv').config();
https.globalAgent.options.rejectUnauthorized = false;
const client = new tmi.Client({
    options: { debug: true },
    identity: {
        username: 'dpentsworth',
        password: process.env.OAUTH
    },
    channels: [ 'DThaiPome' ]
});
const urlStart = process.env.API_URL;

let bonusUsers = [];
let bettingStatus;

init();

async function init () {
    
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
        regularMessage('DThaiPome');
    });

    client.on('disconnected', (reason) => {
        reconnect();
    });
    
    client.on('connected', () => {
        regularMessage('DThaiPome');
    });
    
    await client.connect();
    rewardLoop('DThaiPome');
}

async function reconnect() {
    let delay = 2;
    const maxDelay = 60;
    let state;
    while (state !== "OPEN") {
        await sleep(delay * 1000);
        console.log(`Waited ${delay} seconds, attempting to connect`);
        await client.connect().catch((err) => {
            delay *= 2;
            delay = delay > maxDelay ? maxDelay : delay;
        });
        state = await client.readyState();
    }
}

async function rewardLoop(channel) {
    while(true) {
        await checkForRewards(channel);
        await checkForBettingStatus(channel);
        flushChat(client);
        await sleep(100);
    }
}

async function checkForBettingStatus(channel) {
    let status = await getBettingStatus();
    if (bettingStatus === undefined) {
        bettingStatus = status;
    } else {
        if (bettingStatus !== status) {
            bettingStatus = status;
            say(channel, `Betting has ${bettingStatus ? "opened" : "closed"}!`);
        }
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
    let winningUsers = [];
    rewards.forEach((val) => {
        winningUsers.push(`@${val.user}`);
        addUserPoints(val.user, val.points);
    });
    let winnersStr = winningUsers.join(", ");
    if (count == 0) {
        say(channel, "There were no winners this split", 5000);
    } else {
        say(channel, `${count} player${count == 1 ? "" : "s"} ha${count == 1 ? "s" : "ve"} won the pot! ${winnersStr}`, 5000);
        getAndAnnounceStreaks(channel);
    }
}

async function getAndAnnounceStreaks(channel) {
    await getStreaks()
    .then((streaks) => {
        if (streaks) {
            announceStreaks(streaks, channel);
        }
    })
}

async function getStreaks() {let streaks;
    await get("UserStreaks")
    .then((res) => {
        if (res.error === 0) {
            streaks = res.streaks;
        }
    }).catch((error) => {});

    if (streaks) {
        return streaks;
    }
}

async function announceStreaks(streaks, channel) {
    streaks.forEach((streak) => {
        let user = streak.user;
        let val = streak.streak;
        if (val > 1) {
            say(channel, `@${user} is on a ${val}-win streak, and has received an extra ${20 * (val - 1)} points!`, 5000);
        }
    })
}

function regularMessage(channel) {
    let msg = "Enjoying the stream? Support my channel by following! It is free, and you can always unfollow later. Use '!d help' to see available commands.";
    say(channel, msg);
}

// Assume at least 1 argument
async function handleCommand(username, args, channel) {
    switch(args[0]) {
        case "bet":
            args.shift();
            handleBetCommand(username, args, channel);
            break;
        case "help":
            handleHelpCommand(username, channel);
            break;
        case "bonus":
            handleBonusCommand(username, channel);
            break;
        case "pot":
            handlePotCommand(username, channel);
            break;
        case "streak":
            handleStreakCommand(username, channel);
            break;
        case "status":
            handleStatusCommand(username, channel);
            break;
    }
}

async function handleCloseCommand(username) {
    client.disconnect();
}

async function handleStatusCommand(username, channel) {
    let open = await getBettingStatus();

    if (open === true) {
        say(channel, `@${username} betting is currently open!`);
    } else if (open === false) {
        say(channel, `@${username} betting is currently closed!`);
    }
}

async function getBettingStatus() {
    let open;
    await get("GetBettingOpen")
    .then((res) => {
        open = res.open;
    }).catch((err) => {});
    return open;
}

async function handleStreakCommand(username, channel) {
    await getStreaks()
    .then((streaks) => {
        if (streaks) {
            let streak = getStreakOf(streaks, username);
            if (streak === 0) {
                say(channel, `@${username} you do not have a streak`);
            } else {
                say(channel, `@${username} you are on a ${streak}-win streak!`);
            }
        }
    })
}

function getStreakOf(streaks, username) {
    let streak;
    streaks.forEach((val) => {
        if (val.user === username) {
            streak = val.streak;
        }
    });
    return streak ? streak : 0;
}

async function handlePotCommand(username, channel) {
    let pool;
    await get("PointPool")
    .then((res) => {
        pool = res;
    }).catch((err) => {});

    if (pool) {
        say(channel, `@${username} there are ${pool.points} points in the pot!`)
    }
}

async function handleBonusCommand(username, channel) {
    let hasUser = false;
    bonusUsers.forEach((val) => {
        if (username == val) {
            hasUser = true;
        }
    });
    if (hasUser) {
        say(channel, `@${username} you have already claimed your bonus!`);
    } else {
        let points = 200;
        addUserPoints(username, points);
        bonusUsers.push(username);
        say(channel, `@${username} here is your bonus of ${points} points!`);
    }
}

async function handleHelpCommand(username, channel) {
    // let helpLines = [
    //     "!d help",
    //     "!d bet [seconds OR MM:SS] [point amount]",
    //     "!d bonus (once only)",
    //     "!d pot",
    //     "!d streak"
    // ];
    // let helpStr = `@${username} here are some commands you can use: ${helpLines.join(", ")}`;
    let url = "https://www.notion.so/DThaiPome-Split-Betting-Guide-d283dbe9df534ef287ea296651309c0d";
    let helpStr = `@${username} go to this page for a guide split betting and what commands you can use: ${url}`;
    say(channel, helpStr);
}

async function handleBetCommand(username, args, channel) {
    let betArg = parseTime(args[0]);
    let pointArg = parseInt(args[1]);
    if (args.length == 2 &&
        betArg &&
        pointArg) {
        let timeTokens = args[0].split(":");
        let hasMinutes = timeTokens.length === 2;    
        addBet(username, betArg, pointArg, hasMinutes, channel);
    } else {
        console.log(JSON.stringify(args));
        say(channel, `@${username} Usage: !d bet [seconds OR MM:SS] [point amount]`);
    }
}

function parseTime(time) {
    if (!time) {
        return;
    }

    let tokens = time.split(':');
    console.log(tokens[1] + " " + parseInt(tokens[1]));
    if (tokens.length == 2 && (parseInt(tokens[0]) != undefined) && (parseInt(tokens[1]) != undefined)) {
        return (parseInt(tokens[0]) * 60) + parseInt(tokens[1]);
    } else if (tokens.length == 1 && parseInt(time)) {
        return parseInt(time);
    } else {
        return;
    }
}

// string, number, number, points, channel
async function addBet(username, bet, points, inMinutes, channel) {
    let currentPoints = await getUserPoints(username);
    console.log(currentPoints);
    if (currentPoints == -1 || currentPoints < points) {
        say(channel, `@${username} you do not have enough points to bet that much!`);
        return;
    }
    await post("AddBet", {
        user: username,
        bet: bet,
        points: points
    }).then(async (res) => {
        if (res.error === 0) {
            await addUserPoints(username, -points);
            say(channel, `@${username} has bet ${points} points on ${inMinutes ? toMmSs(bet) : bet}${inMinutes ? "" : " seconds"}!`);
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
                    msg = `\"${bet}\" is not a valid bet. Must be a time (either in seconds or in the form [minutes]:[seconds])`;
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

// number -> string
function toMmSs(time) {
    return `${Math.floor(time / 60)}:${Math.floor(time % 60).toLocaleString('en-US', {minimumIntegerDigits: 2, useGrouping: false})}`;
}

async function get(action, args) {
    let url = generateURL(action, args);
    let response;
    await axios.get(url, {
        httpsAgent: https.Agent({rejectUnauthorized: false})
    }).then((res) => {
        response = res;
    }).catch((err) => {
        //console.log(`${url} - Error: ${inspect(err)}`);
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
        //console.log(`${url} - Error: ${inspect(err)}`);
    });
    if (response) {
        return response.data;
    } else {
        throw err;
    }
}

async function addUserPoints(username, points) {
    const url = `https://api.streamelements.com/kappa/v2/points/${process.env.CHANNEL_ID}/${username}/${points}`;
    const options = {
        method: 'PUT',
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${process.env.JWT}`
        }
    };

    await axios(url, options)
    .then((res) => {
        console.log(`Added ${points} points to ${username}'s account`);
    }).catch((err) => {
        console.error(`Point transfer failed at ${url}: ${JSON.stringify(err)}`);
    });
}

async function getUserPoints(username) {
    const url = `https://api.streamelements.com/kappa/v2/points/${process.env.CHANNEL_ID}/${username}`
    const options = {
        method: 'GET',
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${process.env.JWT}`
        }
    };
    let response;
    await axios(url, options)
    .then((res) => {
        response = res;
    }).catch((err) => {
        console.error(`Cannot read points: ${url}: ${inspect(err)}`);
    });
    return response ? response.data.points : -1;
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