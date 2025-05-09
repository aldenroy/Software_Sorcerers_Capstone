; (function () {
    const CAPTURE_URL = '/Home/CaptureMovie';
    let stash = null;

    document.body.addEventListener('click', e => {
        if (!e.target.classList.contains('btn-primary')) return;
        const btn = e.target;
        const card = btn.closest('.movie-card');
        if (!card) return;
        stash = { btn, originalText: btn.textContent };
        btn.disabled = true;
        const titleEl = document.getElementById('modalTitle');
        const observer = new MutationObserver(() => {
            const txt = titleEl.textContent.trim();
            if (txt && txt !== 'Loading...' && stash) {
                observer.disconnect();
                doCapture(txt);
            }
        });
        observer.observe(titleEl, { childList: true, characterData: true, subtree: true });
    });

    function doCapture(titleText) {
        const { btn, originalText } = stash;
        const m = titleText.match(/(.+?)\s*\((\d{4})\)/) || [];
        const realTitle = m[1] || titleText;
        const realYear = m[2] ? parseInt(m[2], 10) : null;
        const posterEl = document.getElementById('modalPoster');
        const realPosterUrl = posterEl?.src || '';
        const genres = document.getElementById('modalGenres')?.textContent.replace('Genres: ', '') || '';
        const rating = document.getElementById('modalRating')?.textContent.replace('Rating: ', '') || '';
        const overview = document.getElementById('modalOverview')?.textContent.replace('Overview: ', '') || '';
        const services = Array.from(document.querySelectorAll('#modalStreaming .streaming-icon'))
            .map(i => i.alt).join(', ');
        fetch(CAPTURE_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                TitleName: realTitle,
                Year: realYear,
                PosterUrl: realPosterUrl,
                Genres: genres,
                Rating: rating,
                Overview: overview,
                StreamingServices: services
            })
        })
            .then(() => btn.textContent = 'Saved!')
            .catch(() => btn.textContent = 'Error')
            .finally(() => {
                setTimeout(() => {
                    btn.disabled = false;
                    btn.textContent = originalText;
                    stash = null;
                }, 1500);
            });
    }
})();
