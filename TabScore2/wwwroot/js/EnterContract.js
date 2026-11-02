// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var isSubmitted = false;

function onFullPageLoad() {
    if (level == -1) {
        setSkip();
    }
    else if (level == 0) {
        setPass();
    }
    else if (level > 0) {
        document.getElementById('n' + level.toString()).className = "btn btn-lg btn-warning m-1 px-0";
        document.getElementById('s' + suit).className = "btn btn-lg btn-warning m-1 p-0";
        document.getElementById('d' + NSEW).className = "btn btn-lg btn-warning m-1 px-2";
        if (double == 'x') {
            document.getElementById('X').className = "btn btn-lg btn-warning m-1 px-2";
        }
        else if (double == 'xx') {
            document.getElementById('XX').className = "btn btn-lg btn-warning m-1 px-2";
        }
        if (suit != "" && NSEW != "") document.getElementById("OKButton").disabled = false;
    }
}

function clearNumbers() {
    document.getElementById('n1').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n2').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n3').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n4').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n5').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n6').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById('n7').className = "btn btn-lg btn-primary m-1 px-0";
    document.getElementById("OKButton").disabled = true;
}

function clearSuits() {
    document.getElementById('sNT').className = "btn btn-lg btn-suit m-1 p-0";
    document.getElementById('sS').className = "btn btn-lg btn-suit m-1 p-0";
    document.getElementById('sH').className = "btn btn-lg btn-suit m-1 p-0";
    document.getElementById('sD').className = "btn btn-lg btn-suit m-1 p-0";
    document.getElementById('sC').className = "btn btn-lg btn-suit m-1 p-0";
    document.getElementById("OKButton").disabled = true;
}

function clearNSEWs() {
    document.getElementById('dN').className = "btn btn-lg btn-secondary m-1 px-2";
    document.getElementById('dS').className = "btn btn-lg btn-secondary m-1 px-2";
    document.getElementById('dE').className = "btn btn-lg btn-secondary m-1 px-2";
    document.getElementById('dW').className = "btn btn-lg btn-secondary m-1 px-2";
    document.getElementById("OKButton").disabled = true;
}

function setLevel(n) {
    level = n;
    document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
    document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
    clearNumbers();
    document.getElementById('n' + level.toString()).className = "btn btn-lg btn-warning m-1 px-0";
    if (suit != "" && NSEW != "") document.getElementById("OKButton").disabled = false;
}

function setSkip() {
    level = -1;
    suit = "";
    double = "";
    NSEW = "";
    document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
    clearNumbers();
    clearSuits();
    clearNSEWs();
    document.getElementById('X').className = "btn btn-lg btn-double m-1 px-2";
    document.getElementById('XX').className = "btn btn-lg btn-redouble m-1 px-2";
    document.getElementById('skip').className = "btn btn-lg btn-warning m-1 px-2";
    document.getElementById("OKButton").disabled = false;
}

function setPass() {
    level = 0;
    suit = "";
    double = "";
    NSEW = "";
    document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
    clearNumbers();
    clearSuits();
    clearNSEWs();
    document.getElementById('X').className = "btn btn-lg btn-double m-1 px-2";
    document.getElementById('XX').className = "btn btn-lg btn-redouble m-1 px-2";
    document.getElementById('pass').className = "btn btn-lg btn-warning m-1 px-2";
    document.getElementById("OKButton").disabled = false;
}

function setSuit(s) {
    suit = s;
    clearSuits();
    document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
    document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
    document.getElementById('s' + suit).className = "btn btn-lg btn-warning m-1 p-0";
    if (level > 0 && NSEW != "") document.getElementById("OKButton").disabled = false;
}

function setX() {
    if (double != 'x') {
        double = "x";
        document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
        document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
        document.getElementById('X').className = "btn btn-lg btn-warning m-1 px-2";
        document.getElementById('XX').className = "btn btn-lg btn-redouble m-1 px-2";
    }
    else {
        double = "";
        document.getElementById('X').className = "btn btn-lg btn-double m-1 px-2";
    }
}

function setXX() {
    if (double != 'xx') {
        double = "xx";
        document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
        document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
        document.getElementById('XX').className = "btn btn-lg btn-warning m-1 px-2";
        document.getElementById('X').className = "btn btn-lg btn-double m-1 px-2";
    }
    else {
        double = "";
        document.getElementById('XX').className = "btn btn-lg btn-redouble m-1 px-2";
    }
}

function setNSEW(d) {
    NSEW = d;
    clearNSEWs();
    document.getElementById('pass').className = "btn btn-lg btn-pass m-1 px-2";
    document.getElementById('skip').className = "btn btn-lg btn-outline-danger m-1 px-2";
    document.getElementById('d' + NSEW).className = "btn btn-lg btn-warning m-1 px-2";
    if (level > 0 && suit != "") document.getElementById("OKButton").disabled = false;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (level == 0) {
        if (!isSubmitted) {
            isSubmitted = true;
            location.href = urlOKButtonPass;
        }
    }
    else if (level == -1) {
        if (!isSubmitted) {
            isSubmitted = true;
            location.href = urlOKButtonSkip;
        }
    }
    else {
        if (!isSubmitted) {
            isSubmitted = true;
            location.href = urlOKButtonContract + '&contractLevel=' + level.toString() + '&contractSuit=' + suit + '&contractX=' + double + '&declarerNSEW=' + NSEW;
        }
    }
}

function BackButtonClick() {
    if (document.getElementById("BackButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlShowBoards;
    }
}