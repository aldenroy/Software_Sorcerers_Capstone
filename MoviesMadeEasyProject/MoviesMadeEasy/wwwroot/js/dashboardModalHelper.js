document.addEventListener('click', e => {
    const trigger = e.target.closest('.movie-card img.img-fluid, .movie-card .movie-title');
    if (!trigger) return;
    trigger.closest('.movie-card')?.querySelector('.btn-primary')?.click();
});
