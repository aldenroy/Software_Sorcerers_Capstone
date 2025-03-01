function addStreamingService() {
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
  
      document.getElementById('selectedServices').value = Array.from(selectedServices).join(',');
    }
  
    document.querySelectorAll('.subscription-container .card').forEach(card => {
      card.addEventListener("click", function () {
        toggleSelection(this);
      });
    });
  }
  
document.addEventListener("DOMContentLoaded", function () {
    addStreamingService();

    const form = document.getElementById('subscriptionForm');
    form.addEventListener('submit', function (event) {
        const selectedValue = document.getElementById('selectedServices').value;
        if (!selectedValue.trim()) {
            event.preventDefault();
            alert("Please select at least one subscription service to add.");
        }
    });
});
  
  module.exports = { addStreamingService };
  