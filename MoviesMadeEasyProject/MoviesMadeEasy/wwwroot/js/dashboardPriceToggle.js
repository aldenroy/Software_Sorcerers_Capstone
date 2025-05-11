(function () {
    var btn = document.getElementById('toggle-prices-btn');
    var total = document.getElementById('subscription-total');
    if (!btn || !total) return;
    btn.addEventListener('click', function () {
        var showing = btn.textContent.trim() === 'Show Prices';
        document.querySelectorAll('.subscription-price').forEach(function (div) {
            div.classList.toggle('d-none', !showing);
        });
        total.classList.toggle('d-none', !showing);
        btn.textContent = showing ? 'Hide Prices' : 'Show Prices';
    });
})();
