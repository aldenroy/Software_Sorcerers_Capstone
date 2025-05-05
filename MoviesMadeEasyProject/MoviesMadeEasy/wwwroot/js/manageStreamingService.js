let originalSelection = "";

function manageStreamingService() {
    const selectedServices = new Set();
    const preselectedServices = new Set();
    const preSelectedInput = document.getElementById('preSelectedServices');
    const selectedServicesInput = document.getElementById('selectedServices');
    const servicePricesInput = document.getElementById('servicePrices');

    if (preSelectedInput && preSelectedInput.value.trim() !== "") {
        preSelectedInput.value.split(',').forEach(id => {
            const t = id.trim();
            preselectedServices.add(t);
            selectedServices.add(t);
        });
        originalSelection = preSelectedInput.value.trim();
    }

    function updateSelectedServicesInput() {
        selectedServicesInput.value = Array.from(selectedServices).join(',');
        updateServicePricesInput();
    }

    function updateServicePricesInput() {
        if (!servicePricesInput) return;
        const prices = {};
        selectedServices.forEach(id => {
            const card = document.querySelector(`.subscription-container .card[data-id="${id}"]`);
            if (!card) return;
            const v = card.querySelector('.price-input').value;
            prices[id] = parseFloat(v) || 0;
        });
        servicePricesInput.value = JSON.stringify(prices);
    }

    function updateCardAppearance(card) {
        const id = card.getAttribute('data-id');
        let txt = "";

        if (preselectedServices.has(id)) {
            if (!selectedServices.has(id)) {
                card.classList.add('marked-for-deletion');
                txt = "Marked for deletion";
            } else {
                card.classList.remove('marked-for-deletion');
                txt = "Preselected";
            }
        } else {
            if (selectedServices.has(id)) {
                card.classList.add('marked-for-addition');
                txt = "Marked for addition";
            } else {
                card.classList.remove('marked-for-addition');
                txt = "Not selected";
            }
        }

        const base = card.querySelector('.card-text')?.innerText ?? "";
        card.setAttribute('aria-label', `${base}, ${txt}`);
    }

    function toggleSelection(card) {
        const id = card.getAttribute('data-id');
        if (selectedServices.has(id)) {
            selectedServices.delete(id);
            card.classList.remove('selected');
        } else {
            selectedServices.add(id);
            card.classList.add('selected');
        }
        updateCardAppearance(card);
        updateSelectedServicesInput();
    }

    document.querySelectorAll('.subscription-container .card').forEach(card => {
        card.insertAdjacentHTML('beforeend', `
            <input type="number" min="0" step="0.01" class="price-input" placeholder="Price">
        `);

        const pi = card.querySelector('.price-input');
        pi.addEventListener('input', () => {
            updateServicePricesInput();
        });

        ['click', 'keydown', 'keypress'].forEach(evt =>
            pi.addEventListener(evt, e => e.stopPropagation())
        );

        card.setAttribute('tabindex', '0');
        if (selectedServices.has(card.getAttribute('data-id'))) {
            card.classList.add('selected');
        }

        card.addEventListener('click', () => toggleSelection(card));
        card.addEventListener('keydown', e => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                toggleSelection(card);
            }
        });
        card.addEventListener('focus', () => updateCardAppearance(card));
        updateCardAppearance(card);
    });

    updateSelectedServicesInput();
}

document.addEventListener("DOMContentLoaded", () => {
    manageStreamingService();
    const form = document.getElementById('subscriptionForm');
    form?.addEventListener('submit', e => {
        if (document.getElementById('selectedServices').value === originalSelection) {
            alert("No changes were made");
            e.preventDefault();
        }
    });
});

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { manageStreamingService };
}
