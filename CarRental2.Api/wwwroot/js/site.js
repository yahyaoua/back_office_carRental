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






document.addEventListener("DOMContentLoaded", function () {

    const card = document.querySelector(".home-card");
    const button = document.querySelector(".btn-primary-custom");

    /* Déclenche l’animation */
    if (card) {
        setTimeout(() => {
            card.style.opacity = "1";
        }, 100);
    }

    /* Effet interactif bouton */
    if (button) {
        button.addEventListener("mouseenter", () => {
            button.style.letterSpacing = "1px";
        });

        button.addEventListener("mouseleave", () => {
            button.style.letterSpacing = "0";
        });
    }

});

