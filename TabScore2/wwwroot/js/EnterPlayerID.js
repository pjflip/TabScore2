// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var playerIdString = "";
var isSubmitted = false;

function addNumber(e) {
    if (playerIdString == "0") playerIdString = "";
    playerIdString = playerIdString + e;
    document.getElementById('playerNumberBox').value = playerIdString;
    document.getElementById("OKButton").disabled = false;
}

function unknown() {
    playerIdString = "0";
    document.getElementById('playerNumberBox').value = stringUnknown;
    document.getElementById("OKButton").disabled = false;
}

function clearplayerNumber() {
    playerIdString = ""
    document.getElementById('playerNumberBox').value = "";
    document.getElementById("OKButton").disabled = true;
}

function clearLastEntry() {
    if (playerIdString.length > 0) {
        playerIdString = playerIdString.slice(0, -1);
        if (playerIdString == "") document.getElementById("OKButton").disabled = true;
    }
    document.getElementById('playerNumberBox').value = playerIdString;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + '&playerId=' + playerIdString;
    }
}