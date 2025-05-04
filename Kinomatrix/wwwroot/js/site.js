function handleKeyPress(event) {
    if (event.key === "Enter") {
        searchMovie();
    }
}

function debugmsg() {
    console.log("test");
}
function goBack() {
    window.location.href = "/";
}

function showToast(message) {
    // Get the toast element
    const toastElement = document.getElementById('customToast');

    // Set the text inside the toast body
    const toastBody = toastElement.querySelector('.toast-body');
    toastBody.textContent = message;

    // Initialize the toast with Bootstrap
    const toast = new bootstrap.Toast(toastElement);

    // Show the toast
    toast.show();

    // Hide the toast after 3 seconds
    setTimeout(() => {
        toast.hide();
    }, 2000);
}

function generateStars(rating) {
    const stars = Math.round(parseFloat(rating));
    return '★'.repeat(stars) + '☆'.repeat(10 - stars);
}

