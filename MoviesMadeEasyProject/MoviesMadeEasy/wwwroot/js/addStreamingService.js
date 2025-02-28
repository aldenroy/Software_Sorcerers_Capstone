document.addEventListener("DOMContentLoaded", function () {
    let selectedServices = new Set();

    function toggleSelection(card) {
        let serviceId = card.getAttribute('data-id');

        if (selectedServices.has(serviceId)) {
            selectedServices.delete(serviceId);
            card.classList.remove('selected');
        } else {
            selectedServices.add(serviceId);
            card.classList.add('selected');
        }

        // Update hidden input field before form submission
        document.getElementById('selectedServices').value = Array.from(selectedServices).join(',');
    }

    // Attach event listeners to all subscription cards
    document.querySelectorAll('.subscription-container .card').forEach(card => {
        card.addEventListener("click", function () {
            toggleSelection(this);
        });
    });
});
