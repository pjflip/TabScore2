// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var isSubmitted = false;

function onFullPageLoad() {
    if (tricksTaken != '-1') {
        document.getElementById('n' + tricksTaken).className = "btn btn-warning btn-lg m-1 px-0";
        document.getElementById("OKButton").disabled = false;
    }
}

function settricksTaken(n) {
    tricksTaken = n;
    document.getElementById('n0').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n1').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n2').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n3').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n4').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n5').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n6').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n7').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n8').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n9').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n10').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n11').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n12').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n13').className = "btn btn-primary btn-lg m-1 px-0";
    document.getElementById('n' + tricksTaken).className = "btn btn-warning btn-lg m-1 px-0";
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