let originalSelection = "";

function manageStreamingService() {
    const selectedServices = new Set();
    const preselectedServices = new Set();
    const preSelectedInput = document.getElementById('preSelectedServices');
    const selectedServicesInput = document.getElementById('selectedServices');

    if (preSelectedInput && preSelectedInput.value.trim() !== "") {
        preSelectedInput.value.split(',').forEach(id => {
            const trimmedId = id.trim();
            preselectedServices.add(trimmedId);
            selectedServices.add(trimmedId);
        });
        originalSelection = preSelectedInput.value.trim();
    } else {
        originalSelection = "";
    }

    function updateSelectedServicesInput() {
        if (selectedServicesInput) {
            selectedServicesInput.value = Array.from(selectedServices).join(',');
        }
    }

    function updateCardAppearance(card) {
        const serviceId = card.getAttribute('data-id');
        if (preselectedServices.has(serviceId)) {
            if (!selectedServices.has(serviceId)) {
                card.classList.add('marked-for-deletion');
            } else {
                card.classList.remove('marked-for-deletion');
            }
        } else {
            if (selectedServices.has(serviceId)) {
                card.classList.add('marked-for-addition');
            } else {
                card.classList.remove('marked-for-addition');
            }
        }
    }

    function toggleSelection(card) {
        const serviceId = card.getAttribute('data-id');
        if (selectedServices.has(serviceId)) {
            selectedServices.delete(serviceId);
            card.classList.remove('selected');
        } else {
            selectedServices.add(serviceId);
            card.classList.add('selected');
        }
        updateCardAppearance(card);
        updateSelectedServicesInput();
    }

    document.querySelectorAll('.subscription-container .card').forEach(card => {
        const serviceId = card.getAttribute('data-id');
        if (selectedServices.has(serviceId)) {
            card.classList.add('selected');
        }
        card.addEventListener("click", function () {
            toggleSelection(this);
        });
        updateCardAppearance(card);
    });

    updateSelectedServicesInput();
}

document.addEventListener("DOMContentLoaded", function () {
    manageStreamingService();
    const form = document.getElementById('subscriptionForm');
    form.addEventListener('submit', function (event) {
        const currentSelection = document.getElementById('selectedServices').value;
        if (currentSelection === originalSelection) {
            alert("No changes were made");
            event.preventDefault();
        }
    });
});

module.exports = { manageStreamingService };
