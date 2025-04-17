; (function () {
    const CAPTURE_URL = '/Home/CaptureMovie';

    document.addEventListener('click', async e => {
        if (!e.target.matches('.btn-view-details')) return;

        const btn = e.target;
        const card = btn.closest('.movie-card');
        if (!card) return;

        const payload = {
            ExternalId: card.dataset.externalId,
            TitleName: card.querySelector('h5').childNodes[0].textContent.trim(),
            Year: parseInt(card.querySelector('.movie-year').textContent.replace(/[()]/g, ''), 10),
            Type: card.dataset.type,
            PosterUrl: card.dataset.posterUrl || null,
            Genres: card.dataset.genres,        
            Rating: card.querySelector('.movie-rating').textContent.replace('Rating: ', '') || null,
            Overview: card.dataset.overview,
            StreamingServices: card.dataset.streaming     
        };

        try {
            btn.disabled = true;
            await fetch(CAPTURE_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            btn.textContent = 'Saved!';
        } catch {
            btn.textContent = 'Error';
        } finally {
            setTimeout(() => {
                btn.disabled = false;
                btn.textContent = 'View Details';
            }, 1500);
        }
    });
})();
