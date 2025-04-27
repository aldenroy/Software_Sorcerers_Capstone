document.addEventListener('click', e => {
    const trigger = e.target.closest('.movie-card img.img-fluid, .movie-card .movie-title');
    if (!trigger) return;
    trigger.closest('.movie-card')?.querySelector('.btn-primary')?.click();
});

document.addEventListener('click', async e => {
    const removeBtn = e.target.closest('.remove-rvt');
    if (!removeBtn) return;

    const card = removeBtn.closest('.movie-card');
    const titleId = removeBtn.dataset.titleId;

    try {
        const res = await fetch(`/User/RemoveRecentlyViewed/${titleId}`, { method: 'DELETE' });
        if (res.ok) card.remove();
    } catch (err) {
        console.error('Failed to remove recently-viewed title:', err);
    }
});
