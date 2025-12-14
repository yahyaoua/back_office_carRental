document.addEventListener("DOMContentLoaded", () => {

    const startDate = document.querySelector('input[name="RequestedStart"]');
    const endDate = document.querySelector('input[name="RequestedEnd"]');

    if (!startDate || !endDate) return;

    function validateDates() {
        if (startDate.value && endDate.value) {
            if (endDate.value < startDate.value) {
                endDate.setCustomValidity("Return date must be after pickup date");
                endDate.classList.add("is-invalid");
            } else {
                endDate.setCustomValidity("");
                endDate.classList.remove("is-invalid");
            }
        }
    }

    startDate.addEventListener("change", validateDates);
    endDate.addEventListener("change", validateDates);

});
