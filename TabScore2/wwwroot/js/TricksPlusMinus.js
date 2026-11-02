// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var isSubmitted = false;

function onFullPageLoad() {
    if (tricksTaken != -1) {
        var diffTricks = tricksTaken - contractLevel - 6;
        if (diffTricks < 0) {
            document.getElementById('minus' + (-diffTricks).toString()).className = "btn btn-warning btn-lg m-1 px-0";
        }
        else if (diffTricks == 0) {
            document.getElementById('equals').className = "btn btn-warning btn-lg m-1 px-0";
        }
        else {
            document.getElementById('plus' + diffTricks.toString()).className = "btn btn-warning btn-lg m-1 px-0";
        }
        document.getElementById("OKButton").disabled = false;
    }
}

function resetButtons() {
    for (var i = 1; i < contractLevel + 7; i++) {
        document.getElementById('minus' + i.toString()).className = "btn btn-minus btn-lg m-1 px-0";
    }
    document.getElementById('equals').className = "btn btn-primary btn-lg m-1 px-0";
    for (var i = 1; i < 8 - contractLevel; i++) {
        document.getElementById('plus' + i.toString()).className = "btn btn-plus btn-lg m-1 px-0";
    }
}

function setMinusTricks(n) {
    tricksTaken = contractLevel + 6 - n;
    resetButtons();
    document.getElementById('minus' + n.toString()).className = "btn btn-warning btn-lg m-1 px-0";
    document.getElementById("OKButton").disabled = false;
}

function setPlusTricks(n) {
    tricksTaken = contractLevel + 6 + n;
    resetButtons();
    document.getElementById('plus' + n.toString()).className = "btn btn-warning btn-lg m-1 px-0";
    document.getElementById("OKButton").disabled = false;
}

function setEqualsTricks() {
    tricksTaken = contractLevel + 6;
    resetButtons();
    document.getElementById('equals').className = "btn btn-warning btn-lg m-1 px-0";
    document.getElementById("OKButton").disabled = false;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + '&tricksTaken=' + tricksTaken;
    }
}

function BackButtonClick() {
    if (document.getElementById("BackButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlBackButtonClick;
    }
}