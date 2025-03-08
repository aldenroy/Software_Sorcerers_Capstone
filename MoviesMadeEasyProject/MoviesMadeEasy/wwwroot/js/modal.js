document.addEventListener("DOMContentLoaded", () => {
    //listens for the dynamically added elements
    document.body.addEventListener("click", (event) => {
        if (event.target.matches(".btn-primary")) {
            openModal(event.target);
        }
    });
});

function openModal(button) {
    let movieCard = button.closest(".movie-card");
    let title = movieCard.querySelector("h5").innerText;
    let posterUrl = movieCard.querySelector("img").src;
    let genres = movieCard.querySelector(".movie-genres").innerText;
    let rating = movieCard.querySelector(".movie-rating").innerText;

    //placeholder content for population
    document.getElementById("modalTitle").innerText = title;
    document.getElementById("modalPoster").src = posterUrl;
    document.getElementById("modalGenres").innerText = genres;
    document.getElementById("modalRating").innerText = rating;

    //start showing the modal
    let modal = new bootstrap.Modal(document.getElementById("movieModal"));
    modal.show();
}
