// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var suit = "";
var card = "";
var isSubmitted = false;

function onFullPageLoad() {
    if (leadCard == "SKIP") {
        suit = "SKIP";
        if (document.getElementById('skip')) {
            document.getElementById('skip').className = "btn btn-warning btn-lg m-1 px-2";
        }
        document.getElementById("OKButton").disabled = false;
    }
    else if (leadCard != "") {
        suit = leadCard.substr(0, 1);
        card = leadCard.substr(1, 1);
        document.getElementById('s' + suit).className = "btn btn-warning btn-lg m-1 p-0";
        document.getElementById('c' + card).className = "btn btn-warning btn-lg m-1 px-0";
        document.getElementById("OKButton").disabled = false;
    }
}

function setCard(c) {
    card = c;
    if (suit == "SKIP") suit = "";
    document.getElementById('cA').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cK').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cQ').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cJ').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cT').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c9').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c8').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c7').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c6').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c5').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c4').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c3').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c2').className = "btn btn-primary btn-lg m-1 px-0";
    if (document.getElementById('skip')) {
        document.getElementById('skip').className = "btn btn-outline-danger btn-lg m-1 px-2";
    }
    document.getElementById('c' + card).className = "btn btn-warning btn-lg m-1 px-0";
    if (suit == "") {
        document.getElementById("OKButton").disabled = true;
    }
    else {
        document.getElementById("OKButton").disabled = false;
    }
}

function setSuit(s) {
    suit = s;
    document.getElementById('sS').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sH').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sD').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sC').className = "btn btn-suit btn-lg m-1 p-0";
    if (document.getElementById('skip')) {
        document.getElementById('skip').className = "btn btn-outline-danger btn-lg m-1 px-2";
    }
    document.getElementById('s' + suit).className = "btn btn-warning btn-lg m-1 p-0";
    if (card == "") {
        document.getElementById("OKButton").disabled = true;
    }
    else {
        document.getElementById("OKButton").disabled = false;
    }
}

function setSkip() {
    suit = "SKIP";
    card = "";
    document.getElementById('cA').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cK').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cQ').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cJ').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('cT').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c9').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c8').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c7').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c6').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c5').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c4').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c3').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('c2').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('sS').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sH').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sD').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('sC').className = "btn btn-suit btn-lg m-1 p-0";
    document.getElementById('skip').className = "btn btn-warning btn-lg m-1 px-2";
    document.getElementById("OKButton").disabled = false;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + '&card=' + suit + card;
    }
}

function BackButtonClick() {
    if (document.getElementById("BackButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlEnterContract;
    }
}