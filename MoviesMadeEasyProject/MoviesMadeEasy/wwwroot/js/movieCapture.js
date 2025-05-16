;(function(){
  const CAPTURE_URL = '/Home/CaptureMovie';
  const modalEl     = document.getElementById('movieModal');
  let lastBtn, lastText;

  document.body.addEventListener('click', e => {
    const btn = e.target.closest('button.btn-primary');
    if (!btn || !btn.textContent.includes('View Details')) return;
    lastBtn  = btn;
    lastText = btn.textContent;
    btn.disabled = true;
    modalEl.addEventListener('shown.bs.modal', onShown, { once: true });
  });

  function onShown() {
    const posterEl  = document.getElementById('modalPoster');
    const initial   = posterEl.getAttribute('src');
    if (initial) {
      capture();
    } else {
      const obs = new MutationObserver((_, o) => {
        if (posterEl.getAttribute('src')) {
          o.disconnect();
          capture();
        }
      });
      obs.observe(posterEl, { attributes: true, attributeFilter: ['src'] });
    }
  }

    async function capture() {
        const btn = lastBtn;
        const txt = document.getElementById('modalTitle').textContent.trim();
        const m = txt.match(/(.+?)\s*\((\d{4})\)/) || [];
        const payload = {
            TitleName: m[1] || txt,
            Year: m[2] ? parseInt(m[2], 10) : null,
            PosterUrl: document.getElementById('modalPoster').getAttribute('src') || '',
            Genres: document.getElementById('modalGenres').textContent.replace('Genres: ', '') || '',
            Rating: document.getElementById('modalRating').textContent.replace('Rating: ', '') || '',
            Overview: document.getElementById('modalOverview').textContent.replace('Overview: ', '') || '',
            StreamingServices: Array.from(
                document.querySelectorAll('#modalStreaming .streaming-icon')
            ).map(img => img.alt).join(', ')
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
                btn.textContent = lastText;
            }, 1500);
        }
    }
})();
