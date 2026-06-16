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
