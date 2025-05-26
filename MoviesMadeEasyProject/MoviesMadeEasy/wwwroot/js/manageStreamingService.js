; (function () {
    let originalSelection = "";
    let originalPrices = {};

    function manageStreamingService() {
        const preSelectedInput = document.getElementById('preSelectedServices');
        const selectedServicesInput = document.getElementById('selectedServices');
        const servicePricesInput = document.getElementById('servicePrices');
        const selectedServices = new Set();
        const preselectedServices = new Set();

        let initialPrices = {};
        if (servicePricesInput && servicePricesInput.value.trim()) {
            try {
                initialPrices = JSON.parse(servicePricesInput.value);
            } catch (e) {
                initialPrices = {};
            }
        }
        originalPrices = { ...initialPrices };

        if (preSelectedInput && preSelectedInput.value.trim()) {
            preSelectedInput.value
                .split(',')
                .map(s => s.trim())
                .forEach(id => {
                    selectedServices.add(id);
                    preselectedServices.add(id);
                });
            originalSelection = preSelectedInput.value.trim();
        }

        function updateServicePricesInput() {
            if (!servicePricesInput) return;
            const prices = {};
            selectedServices.forEach(id => {
                const card = document.querySelector(`.subscription-container .card[data-id="${id}"]`);
                const input = card.querySelector('.price-input');
                const v = input.value.trim();
                prices[id] = v === '' && initialPrices[id] != null
                    ? initialPrices[id]
                    : parseFloat(v) || 0;
            });
            servicePricesInput.value = JSON.stringify(prices);
        }

        function updateSelectedServicesInput() {
            selectedServicesInput.value = Array.from(selectedServices).join(',');
            updateServicePricesInput();
        }

        function updateCardAppearance(card) {
            const id = card.getAttribute('data-id');
            let txt = '';
            if (preselectedServices.has(id)) {
                if (!selectedServices.has(id)) {
                    card.classList.add('marked-for-deletion');
                    txt = 'Marked for deletion';
                } else {
                    card.classList.remove('marked-for-deletion');
                    txt = 'Preselected';
                }
            } else {
                if (selectedServices.has(id)) {
                    card.classList.add('marked-for-addition');
                    txt = 'Marked for addition';
                } else {
                    card.classList.remove('marked-for-addition');
                    txt = 'Not selected';
                }
            }
            card.setAttribute('aria-checked', String(selectedServices.has(id)));
            const base = card.querySelector('.card-text')?.innerText || '';
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
            const id = card.getAttribute('data-id');
            const serviceName = card.querySelector('.card-text').textContent.trim();
            card.setAttribute('role', 'checkbox');
            card.setAttribute('tabindex', '0');
            card.insertAdjacentHTML(
                'beforeend',
                `<input type="number" min="0" max="1000" step="0.01" class="price-input" placeholder="Price" aria-label="Price for service ${serviceName}">`
            );
            const pi = card.querySelector('.price-input');
            if (initialPrices[id] != null) pi.value = initialPrices[id];
            pi.addEventListener('input', updateServicePricesInput);
            pi.addEventListener('click', e => e.stopPropagation());
            if (selectedServices.has(id)) card.classList.add('selected');
            card.addEventListener('click', () => toggleSelection(card));
            card.addEventListener('keydown', e => {
                if ((e.key === 'Enter' || e.key === ' ') && e.target === card) {
                    e.preventDefault();
                    toggleSelection(card);
                }
            });
            card.addEventListener('focus', () => updateCardAppearance(card));
            updateCardAppearance(card);
        });

        updateSelectedServicesInput();
    }

    document.addEventListener('DOMContentLoaded', () => {
        manageStreamingService();
        const form = document.getElementById('subscriptionForm');
        if (form) {
            form.addEventListener('submit', e => {
                const currentSel = document.getElementById('selectedServices').value;
                const pricesField = document.getElementById('servicePrices');
                let pricesChanged = true;
                if (pricesField) {
                    const now = JSON.parse(pricesField.value || '{}');
                    const origKeys = Object.keys(originalPrices);
                    const curKeys = Object.keys(now);
                    if (
                        origKeys.length === curKeys.length &&
                        origKeys.every(k => now[k] === originalPrices[k])
                    ) {
                        pricesChanged = false;
                    }
                }
                if (currentSel === originalSelection && !pricesChanged) {
                    alert('No changes were made');
                    e.preventDefault();
                }
            });
        }
    });

    if (typeof module !== 'undefined' && module.exports) {
        module.exports = { manageStreamingService };
    }
})();
