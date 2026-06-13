var isSubmitted = false;

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick;
    }
}

function BackButtonClick() {
    if (document.getElementById("BackButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlBackButtonClick;
    }
}

function ScoreThisRoundButtonClick() {
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlScoreThisRoundButtonClick;
    }
}

function ViewOnlyButtonClick() {
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlViewOnlyButtonClick;
    }
}