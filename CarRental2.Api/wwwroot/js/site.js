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





document.addEventListener("DOMContentLoaded", () => {
    const loginCard = document.querySelector(".login-card");
    if (!loginCard) return;

    // Sélection UNIQUEMENT des champs utiles
    const inputs = loginCard.querySelectorAll(
        'input[type="email"], input[type="password"]'
    );

    // Effet hover carte
    loginCard.addEventListener("mouseenter", () => {
        loginCard.style.transform = "scale(1.02)";
    });

    loginCard.addEventListener("mouseleave", () => {
        loginCard.style.transform = "scale(1)";
    });

    // Animation focus input
    inputs.forEach(input => {
        input.addEventListener("focus", () => {
            input.style.backgroundColor = "#f8fafc";
        });

        input.addEventListener("blur", () => {
            input.style.backgroundColor = "#ffffff";
        });
    });

    // Validation correcte
    const form = loginCard.querySelector("form");
    form.addEventListener("submit", (e) => {
        const empty = [...inputs].some(i => i.value.trim() === "");
        if (empty) {
            e.preventDefault();
            alert("Please fill in all fields.");
        }
    });
});
