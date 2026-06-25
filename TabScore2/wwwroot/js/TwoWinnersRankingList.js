// TabScore2, a wireless bridge scoring program.Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var isSubmitted = false;

var pollRanking = new XMLHttpRequest();
pollRanking.onload = pollRankingListener;
setTimeout(function () {
    pollRanking.open('get', urlPollRanking, true);
    pollRanking.send();
}, 10000);

function pollRankingListener() {
    rankingList = JSON.parse(this.responseText);
    var new_tbodyNS = document.createElement("tbody");
    var new_tbodyEW = document.createElement("tbody");
    var iRowEW = 0;
    var iRowNS = 0;
    for (var i = 0; i < rankingList.length; i++) {
        var row = null;
        if (rankingList[i].orientation == "E") {
            row = new_tbodyEW.insertRow(iRowEW);
            if (rankingList[i].contestantNumber == contestantNumberEast) row.className = "table-warning";
            iRowEW++;
        }
        else {
            row = new_tbodyNS.insertRow(iRowNS);
            if (rankingList[i].contestantNumber == contestantNumberNorth) row.className = "table-success";
            iRowNS++;
        }
        var cellRank = row.insertCell(0);
        var cellPairNumber = row.insertCell(1);
        var cellScore = row.insertCell(2);
        cellRank.innerHTML = rankingList[i].rank;
        cellPairNumber.innerHTML = rankingList[i].contestantNumber;
        cellScore.innerHTML = rankingList[i].score + "%";
    }
    var old_tbodyNS = document.getElementById("tableBodyNS");
    old_tbodyNS.parentNode.replaceChild(new_tbodyNS, old_tbodyNS);
    new_tbodyNS.id = "tableBodyNS";
    var old_tbodyEW = document.getElementById("tableBodyEW");
    old_tbodyEW.parentNode.replaceChild(new_tbodyEW, old_tbodyEW);
    new_tbodyEW.id = "tableBodyEW";
    setTimeout(function () {
        pollRanking.open('get', urlPollRanking, true);
        pollRanking.send();
    }, 10000);
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        if (finalRankingList) {
            location.href = urlEndScreen;
        }
        else {
            location.href = urlShowMove;
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