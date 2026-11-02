// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var isSubmitted = false;
var direction = "N";
var suit = "S"
var cardCount = 0;
var cardCountN = 0;
var cardCountS = 0;
var cardCountE = 0;
var cardCountW = 0;
const NSABC = [];
const NHABC = [];
const NDABC = [];
const NCABC = [];
const SSABC = [];
const SHABC = [];
const SDABC = [];
const SCABC = [];
const ESABC = [];
const EHABC = [];
const EDABC = [];
const ECABC = [];
const WSABC = [];
const WHABC = [];
const WDABC = [];
const WCABC = [];
const cardDBString = ['A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2'];
const suitString = ['S', 'H', 'D', 'C'];

function displayString(cABC) {
    return cardString[parseInt(cABC, 36) - 10];  // Convert A to 0, B to 1, etc to index array
}

function dBString(cABC) {
    return cardDBString[parseInt(cABC, 36) - 10];
}

function redrawS() {
    if (direction == "N") {
        for (var index = 0; index < NSABC.length; index++) {
            document.getElementById('S' + index.toString()).innerHTML = displayString(NSABC[index]);
            document.getElementById('S' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = NSABC.length; index < 13; index++) {
            document.getElementById('S' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "S") {
        for (var index = 0; index < SSABC.length; index++) {
            document.getElementById('S' + index.toString()).innerHTML = displayString(SSABC[index]);
            document.getElementById('S' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = SSABC.length; index < 13; index++) {
            document.getElementById('S' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "E") {
        for (var index = 0; index < ESABC.length; index++) {
            document.getElementById('S' + index.toString()).innerHTML = displayString(ESABC[index]);
            document.getElementById('S' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = ESABC.length; index < 13; index++) {
            document.getElementById('S' + index.toString()).className = "btn d-none";
        }
    }
    else {  // direction = W
        for (var index = 0; index < WSABC.length; index++) {
            document.getElementById('S' + index.toString()).innerHTML = displayString(WSABC[index]);
            document.getElementById('S' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = WSABC.length; index < 13; index++) {
            document.getElementById('S' + index.toString()).className = "btn d-none";
        }
    }
}

function redrawH() {
    if (direction == "N") {
        for (var index = 0; index < NHABC.length; index++) {
            document.getElementById('H' + index.toString()).innerHTML = displayString(NHABC[index]);
            document.getElementById('H' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = NHABC.length; index < 13; index++) {
            document.getElementById('H' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "S") {
        for (var index = 0; index < SHABC.length; index++) {
            document.getElementById('H' + index.toString()).innerHTML = displayString(SHABC[index]);
            document.getElementById('H' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = SHABC.length; index < 13; index++) {
            document.getElementById('H' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "E") {
        for (var index = 0; index < EHABC.length; index++) {
            document.getElementById('H' + index.toString()).innerHTML = displayString(EHABC[index]);
            document.getElementById('H' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = EHABC.length; index < 13; index++) {
            document.getElementById('H' + index.toString()).className = "btn d-none";
        }
    }
    else {  // direction = W
        for (var index = 0; index < WHABC.length; index++) {
            document.getElementById('H' + index.toString()).innerHTML = displayString(WHABC[index]);
            document.getElementById('H' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = WHABC.length; index < 13; index++) {
            document.getElementById('H' + index.toString()).className = "btn d-none";
        }
    }
}

function redrawD() {
    if (direction == "N") {
        for (var index = 0; index < NDABC.length; index++) {
            document.getElementById('D' + index.toString()).innerHTML = displayString(NDABC[index]);
            document.getElementById('D' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = NDABC.length; index < 13; index++) {
            document.getElementById('D' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "S") {
        for (var index = 0; index < SDABC.length; index++) {
            document.getElementById('D' + index.toString()).innerHTML = displayString(SDABC[index]);
            document.getElementById('D' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = SDABC.length; index < 13; index++) {
            document.getElementById('D' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "E") {
        for (var index = 0; index < EDABC.length; index++) {
            document.getElementById('D' + index.toString()).innerHTML = displayString(EDABC[index]);
            document.getElementById('D' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = EDABC.length; index < 13; index++) {
            document.getElementById('D' + index.toString()).className = "btn d-none";
        }
    }
    else {  // direction = W
        for (var index = 0; index < WDABC.length; index++) {
            document.getElementById('D' + index.toString()).innerHTML = displayString(WDABC[index]);
            document.getElementById('D' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = WDABC.length; index < 13; index++) {
            document.getElementById('D' + index.toString()).className = "btn d-none";
        }
    }
}

function redrawC() {
    if (direction == "N") {
        for (var index = 0; index < NCABC.length; index++) {
            document.getElementById('C' + index.toString()).innerHTML = displayString(NCABC[index]);
            document.getElementById('C' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = NCABC.length; index < 13; index++) {
            document.getElementById('C' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "S") {
        for (var index = 0; index < SCABC.length; index++) {
            document.getElementById('C' + index.toString()).innerHTML = displayString(SCABC[index]);
            document.getElementById('C' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = SCABC.length; index < 13; index++) {
            document.getElementById('C' + index.toString()).className = "btn d-none";
        }
    }
    else if (direction == "E") {
        for (var index = 0; index < ECABC.length; index++) {
            document.getElementById('C' + index.toString()).innerHTML = displayString(ECABC[index]);
            document.getElementById('C' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = ECABC.length; index < 13; index++) {
            document.getElementById('C' + index.toString()).className = "btn d-none";
        }
    }
    else {  // direction = W
        for (var index = 0; index < WCABC.length; index++) {
            document.getElementById('C' + index.toString()).innerHTML = displayString(WCABC[index]);
            document.getElementById('C' + index.toString()).className = "btn btn-sm btn-primary px-custom";
        }
        for (var index = WCABC.length; index < 13; index++) {
            document.getElementById('C' + index.toString()).className = "btn d-none";
        }
    }
}

function setDirection(newDirection) {
    document.getElementById('d' + direction).className = "btn btn-secondary m-1 px-2";
    direction = newDirection;
    document.getElementById('d' + direction).className = "btn btn-warning m-1 px-2";
    redrawS();
    redrawH();
    redrawD();
    redrawC();
    if (direction == "N") {
        document.getElementById("count").innerHTML = cardCountN.toString();
    }
    else if (direction == "S") {
        document.getElementById("count").innerHTML = cardCountS.toString();
    }
    else if (direction == "E") {
        document.getElementById("count").innerHTML = cardCountE.toString();
    }
    else {
        document.getElementById("count").innerHTML = cardCountW.toString();
    }
    setSuit('S');
    showRemainder();
}

function setSuit(newSuit) {
    document.getElementById('suit' + suit).className = "btn btn-suit m-1 p-0";
    suit = newSuit;
    document.getElementById('suit' + suit).className = "btn btn-warning m-1 p-0";
    for (var index = 0; index < 13; index++) {
        document.getElementById('c' + String.fromCharCode(65 + index)).disabled = false;
    }
    if (suit == "S") {
        for (var index = 0; index < NSABC.length; index++) {
            document.getElementById('c' + NSABC[index]).disabled = true;
        }
        for (var index = 0; index < SSABC.length; index++) {
            document.getElementById('c' + SSABC[index]).disabled = true;
        }
        for (var index = 0; index < ESABC.length; index++) {
            document.getElementById('c' + ESABC[index]).disabled = true;
        }
        for (var index = 0; index < WSABC.length; index++) {
            document.getElementById('c' + WSABC[index]).disabled = true;
        }
    }
    else if (suit == "H") {
        for (var index = 0; index < NHABC.length; index++) {
            document.getElementById('c' + NHABC[index]).disabled = true;
        }
        for (var index = 0; index < SHABC.length; index++) {
            document.getElementById('c' + SHABC[index]).disabled = true;
        }
        for (var index = 0; index < EHABC.length; index++) {
            document.getElementById('c' + EHABC[index]).disabled = true;
        }
        for (var index = 0; index < WHABC.length; index++) {
            document.getElementById('c' + WHABC[index]).disabled = true;
        }
    }
    else if (suit == "D") {
        for (var index = 0; index < NDABC.length; index++) {
            document.getElementById('c' + NDABC[index]).disabled = true;
        }
        for (var index = 0; index < SDABC.length; index++) {
            document.getElementById('c' + SDABC[index]).disabled = true;
        }
        for (var index = 0; index < EDABC.length; index++) {
            document.getElementById('c' + EDABC[index]).disabled = true;
        }
        for (var index = 0; index < WDABC.length; index++) {
            document.getElementById('c' + WDABC[index]).disabled = true;
        }
    }
    else {
        for (var index = 0; index < NCABC.length; index++) {
            document.getElementById('c' + NCABC[index]).disabled = true;
        }
        for (var index = 0; index < SCABC.length; index++) {
            document.getElementById('c' + SCABC[index]).disabled = true;
        }
        for (var index = 0; index < ECABC.length; index++) {
            document.getElementById('c' + ECABC[index]).disabled = true;
        }
        for (var index = 0; index < WCABC.length; index++) {
            document.getElementById('c' + WCABC[index]).disabled = true;
        }
    }
}

function addCard(cardABC) {   // cardABC values of A=Ace, B=King, etc to allow easy sorting
    if (document.getElementById('c' + cardABC).disabled == true) return;
    if (direction == "N") {
        if (cardCountN == 13) return;
        document.getElementById('c' + cardABC).disabled = true;
        if (suit == "S") {
            NSABC.push(cardABC);
            NSABC.sort();
            redrawS();
        }
        else if (suit == "H") {
            NHABC.push(cardABC);
            NHABC.sort();
            redrawH();
        }
        else if (suit == "D") {
            NDABC.push(cardABC);
            NDABC.sort();
            redrawD();
        }
        else {
            NCABC.push(cardABC);
            NCABC.sort();
            redrawC();
        }
        cardCountN++;
        document.getElementById("count").innerHTML = cardCountN.toString();
    }
    else if (direction == "S") {
        if (cardCountS == 13) return;
        document.getElementById('c' + cardABC).disabled = true;
        if (suit == "S") {
            SSABC.push(cardABC);
            SSABC.sort();
            redrawS();
        }
        else if (suit == "H") {
            SHABC.push(cardABC);
            SHABC.sort();
            redrawH();
        }
        else if (suit == "D") {
            SDABC.push(cardABC);
            SDABC.sort();
            redrawD();
        }
        else {
            SCABC.push(cardABC);
            SCABC.sort();
            redrawC();
        }
        cardCountS++;
        document.getElementById("count").innerHTML = cardCountS.toString();
    }
    else if (direction == "E") {
        if (cardCountE == 13) return;
        document.getElementById('c' + cardABC).disabled = true;
        if (suit == "S") {
            ESABC.push(cardABC);
            ESABC.sort();
            redrawS();
        }
        else if (suit == "H") {
            EHABC.push(cardABC);
            EHABC.sort();
            redrawH();
        }
        else if (suit == "D") {
            EDABC.push(cardABC);
            EDABC.sort();
            redrawD();
        }
        else {
            ECABC.push(cardABC);
            ECABC.sort();
            redrawC();
        }
        cardCountE++;
        document.getElementById("count").innerHTML = cardCountE.toString();
    }
    else {   // direction = W
        if (cardCountW == 13) return;
        document.getElementById('c' + cardABC).disabled = true;
        if (suit == "S") {
            WSABC.push(cardABC);
            WSABC.sort();
            redrawS();
        }
        else if (suit == "H") {
            WHABC.push(cardABC);
            WHABC.sort();
            redrawH();
        }
        else if (suit == "D") {
            WDABC.push(cardABC);
            WDABC.sort();
            redrawD();
        }
        else {
            WCABC.push(cardABC);
            WCABC.sort();
            redrawC();
        }
        cardCountW++;
        document.getElementById("count").innerHTML = cardCountW.toString();
    }
    cardCount++;
    if (cardCount == 52) {
        document.getElementById("OKButton").disabled = false;
    }
    else {
        document.getElementById("OKButton").disabled = true;
    }
    document.getElementById("skipButton").className = "btn d-none" 
    showRemainder();
}

function removeCard(removeSuit, position) {
    var cABC = "";
    if (direction == "N") {
        if (removeSuit == "S") {
            cABC = NSABC[position];
            NSABC.splice(position, 1);
            redrawS();
        }
        else if (removeSuit == "H") {
            cABC = NHABC[position];
            NHABC.splice(position, 1);
            redrawH();
        }
        else if (removeSuit == "D") {
            cABC = NDABC[position];
            NDABC.splice(position, 1);
            redrawD();
        }
        else {
            cABC = NCABC[position];
            NCABC.splice(position, 1);
            redrawC();
        }
        cardCountN--;
        document.getElementById("count").innerHTML = cardCountN.toString();
    }
    else if (direction == "S") {
        if (removeSuit == "S") {
            cABC = SSABC[position];
            SSABC.splice(position, 1);
            redrawS();
        }
        else if (removeSuit == "H") {
            cABC = SHABC[position];
            SHABC.splice(position, 1);
            redrawH();
        }
        else if (removeSuit == "D") {
            cABC = SDABC[position];
            SDABC.splice(position, 1);
            redrawD();
        }
        else {
            cABC = SCABC[position];
            SCABC.splice(position, 1);
            redrawC();
        }
        cardCountS--;
        document.getElementById("count").innerHTML = cardCountS.toString();
    }
    else if (direction == "E") {
        if (removeSuit == "S") {
            cABC = ESABC[position];
            ESABC.splice(position, 1);
            redrawS();
        }
        else if (removeSuit == "H") {
            cABC = EHABC[position];
            EHABC.splice(position, 1);
            redrawH();
        }
        else if (removeSuit == "D") {
            cABC = EDABC[position];
            EDABC.splice(position, 1);
            redrawD();
        }
        else {
            cABC = ECABC[position];
            ECABC.splice(position, 1);
            redrawC();
        }
        cardCountE--;
        document.getElementById("count").innerHTML = cardCountE.toString();
    }
    else {   // direction = W
        if (removeSuit == "S") {
            cABC = WSABC[position];
            WSABC.splice(position, 1);
            redrawS();
        }
        else if (removeSuit == "H") {
            cABC = WHABC[position];
            WHABC.splice(position, 1);
            redrawH();
        }
        else if (removeSuit == "D") {
            cABC = WDABC[position];
            WDABC.splice(position, 1);
            redrawD();
        }
        else {
            cABC = WCABC[position];
            WCABC.splice(position, 1);
            redrawC();
        }
        cardCountW--;
        document.getElementById("count").innerHTML = cardCountW.toString();
    }
    if (removeSuit == suit) {
        document.getElementById('c' + cABC).disabled = false;
    }
    cardCount--;
    document.getElementById("OKButton").disabled = true;
    if (cardCount == 0) {
        document.getElementById("skipButton").className = "btn btn-outline-danger float-end" 
    }
    showRemainder();
}

function showRemainder() {
    if (cardCount == 39 && ((direction == 'N' && cardCountN == 0) || (direction == 'S' && cardCountS == 0) || (direction == 'E' && cardCountE == 0) || (direction == 'W' && cardCountW == 0))) {
        document.getElementById("remainderButton").className = "btn btn-dark float-start"
    }
    else {
        document.getElementById("remainderButton").className = "btn d-none"
    }
}

function remainder() {
    const allSABC = NSABC.concat(SSABC, ESABC, WSABC);
    const allHABC = NHABC.concat(SHABC, EHABC, WHABC);
    const allDABC = NDABC.concat(SDABC, EDABC, WDABC);
    const allCABC = NCABC.concat(SCABC, ECABC, WCABC);
    for (var index = 0; index < 13; index++) {
        var cardABC = String.fromCharCode(65 + index);
        document.getElementById('c' + cardABC).disabled = true;
        if (direction == "N") {
            if (!allSABC.includes(cardABC)) {
                NSABC.push(cardABC);
            }
            if (!allHABC.includes(cardABC)) {
                NHABC.push(cardABC);
            }
            if (!allDABC.includes(cardABC)) {
                NDABC.push(cardABC);
            }
            if (!allCABC.includes(cardABC)) {
                NCABC.push(cardABC);
            }
        }
        else if (direction == "S") {
            if (!allSABC.includes(cardABC)) {
                SSABC.push(cardABC);
            }
            if (!allHABC.includes(cardABC)) {
                SHABC.push(cardABC);
            }
            if (!allDABC.includes(cardABC)) {
                SDABC.push(cardABC);
            }
            if (!allCABC.includes(cardABC)) {
                SCABC.push(cardABC);
            }
        }
        else if (direction == "E") {
            if (!allSABC.includes(cardABC)) {
                ESABC.push(cardABC);
            }
            if (!allHABC.includes(cardABC)) {
                EHABC.push(cardABC);
            }
            if (!allDABC.includes(cardABC)) {
                EDABC.push(cardABC);
            }
            if (!allCABC.includes(cardABC)) {
                ECABC.push(cardABC);
            }
        }
        else {
            if (!allSABC.includes(cardABC)) {
                WSABC.push(cardABC);
            }
            if (!allHABC.includes(cardABC)) {
                WHABC.push(cardABC);
            }
            if (!allDABC.includes(cardABC)) {
                WDABC.push(cardABC);
            }
            if (!allCABC.includes(cardABC)) {
                WCABC.push(cardABC);
            }
        }
    }
    cardCountN = cardCountS = cardCountE = cardCountW = 13;
    document.getElementById("count").innerHTML = '13';
    cardCount = 52;
    redrawS();
    redrawH();
    redrawD();
    redrawC();
    showRemainder();
    document.getElementById("OKButton").disabled = false;
}

function makeReturnString() {
    var returnString = '&NS=';
    for (var index = 0; index < NSABC.length; index++) {
        returnString += dBString(NSABC[index]);
    }
    returnString += '&NH=';
    for (var index = 0; index < NHABC.length; index++) {
        returnString += dBString(NHABC[index]);
    }
    returnString += '&ND=';
    for (var index = 0; index < NDABC.length; index++) {
        returnString += dBString(NDABC[index]);
    }
    returnString += '&NC=';
    for (var index = 0; index < NCABC.length; index++) {
        returnString += dBString(NCABC[index]);
    }
    returnString += '&SS=';
    for (var index = 0; index < SSABC.length; index++) {
        returnString += dBString(SSABC[index]);
    }
    returnString += '&SH=';
    for (var index = 0; index < SHABC.length; index++) {
        returnString += dBString(SHABC[index]);
    }
    returnString += '&SD=';
    for (var index = 0; index < SDABC.length; index++) {
        returnString += dBString(SDABC[index]);
    }
    returnString += '&SC=';
    for (var index = 0; index < SCABC.length; index++) {
        returnString += dBString(SCABC[index]);
    }
    returnString += '&ES=';
    for (var index = 0; index < ESABC.length; index++) {
        returnString += dBString(ESABC[index]);
    }
    returnString += '&EH=';
    for (var index = 0; index < EHABC.length; index++) {
        returnString += dBString(EHABC[index]);
    }
    returnString += '&ED=';
    for (var index = 0; index < EDABC.length; index++) {
        returnString += dBString(EDABC[index]);
    }
    returnString += '&EC=';
    for (var index = 0; index < ECABC.length; index++) {
        returnString += dBString(ECABC[index]);
    }
    returnString += '&WS=';
    for (var index = 0; index < WSABC.length; index++) {
        returnString += dBString(WSABC[index]);
    }
    returnString += '&WH=';
    for (var index = 0; index < WHABC.length; index++) {
        returnString += dBString(WHABC[index]);
    }
    returnString += '&WD=';
    for (var index = 0; index < WDABC.length; index++) {
        returnString += dBString(WDABC[index]);
    }
    returnString += '&WC=';
    for (var index = 0; index < WCABC.length; index++) {
        returnString += dBString(WCABC[index]);
    }
    return returnString;
}

function skip() {
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlSkipButtonClick;
    }
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + makeReturnString();
    }
}

function BackButtonClick() {
    if (document.getElementById("BackButton").disabled) return;
    var backModal = new bootstrap.Modal(document.getElementById("backModal"), { backdrop: 'static' })
    backModal.show()
}

function modalBack() {
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlBackButtonClick;
    }
}