function manageStreamingService() {
    let selectedServices = new Set();
    const preselectedServices = new Set();

    const preSelectedInput = document.getElementById('preSelectedServices');
    if (preSelectedInput && preSelectedInput.value.trim() !== "") {
        preSelectedInput.value.split(',').forEach(id => {
            const trimmedId = id.trim();
            preselectedServices.add(trimmedId);
            selectedServices.add(trimmedId);
        });
    }

    const originalSelection = Array.from(selectedServices).join(',');
    const form = document.getElementById('subscriptionForm');
    if (form) {
        form.setAttribute('data-original-selection', originalSelection);
    }

    function updateCardAppearance(card) {
        let serviceId = card.getAttribute('data-id');
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
      let serviceId = card.getAttribute('data-id');
  
      if (selectedServices.has(serviceId)) {
        selectedServices.delete(serviceId);
        card.classList.remove('selected');
      } else {
        selectedServices.add(serviceId);
        card.classList.add('selected');
      }
      updateCardAppearance(card);
  
      document.getElementById('selectedServices').value = Array.from(selectedServices).join(',');
    }
  
    document.querySelectorAll('.subscription-container .card').forEach(card => {
      let serviceId = card.getAttribute('data-id');
      if (selectedServices.has(serviceId)) {
        card.classList.add('selected');
      }
      card.addEventListener("click", function () {
        toggleSelection(this);
      });
      updateCardAppearance(card);
    });

    document.getElementById('selectedServices').value = Array.from(selectedServices).join(',');
  }
  
document.addEventListener("DOMContentLoaded", function () {
    manageStreamingService();

    const form = document.getElementById('subscriptionForm');
    form.addEventListener('submit', function (event) {
        const currentSelection = document.getElementById('selectedServices').value;
        const originalSelection = form.getAttribute('data-original-selection') || "";

        if (currentSelection === originalSelection) {
            alert("No changes were made");
        }
    });
});
  
  module.exports = { manageStreamingService };
  