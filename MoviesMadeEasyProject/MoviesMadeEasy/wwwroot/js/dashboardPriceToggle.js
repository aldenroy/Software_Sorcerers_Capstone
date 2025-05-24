(function () {
    var btn = document.getElementById('toggle-analysis-btn');
    var total = document.getElementById('subscription-total');
    var analysis = document.getElementById('analysis-section');
    if (!btn || !total || !analysis) return;

    btn.addEventListener('click', function () {
        var showing = btn.textContent.trim() === 'Streaming Service Analysis';
        document.querySelectorAll('.subscription-price').forEach(function (el) {
            el.classList.toggle('d-none', !showing);
        });
        total.classList.toggle('d-none', !showing);
        analysis.classList.toggle('d-none', !showing);
        btn.textContent = showing
            ? 'Hide Analysis'
            : 'Streaming Service Analysis';
    });
})();
