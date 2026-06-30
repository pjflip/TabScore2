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
    var new_tbody = document.createElement("tbody");
    for (var i = 0; i < rankingList.length; i++) {
        var row = new_tbody.insertRow(i);
        if (rankingList[i].contestantNumber == contestantNumberNorth) row.className = "table-success";
        if (rankingList[i].contestantNumber == contestantNumberEast) row.className = "table-warning";
        var cellRank = row.insertCell(0);
        var cellPairNumber = row.insertCell(1);
        var cellScore = row.insertCell(2);
        cellRank.innerHTML = rankingList[i].rank;
        cellPairNumber.innerHTML = rankingList[i].contestantNumber;
        cellScore.innerHTML = rankingList[i].score;
    }
    var old_tbody = document.getElementById("tableBody");
    old_tbody.parentNode.replaceChild(new_tbody, old_tbody);
    new_tbody.id = "tableBody";
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