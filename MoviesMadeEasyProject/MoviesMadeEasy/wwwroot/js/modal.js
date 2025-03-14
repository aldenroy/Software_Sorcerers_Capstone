document.addEventListener("DOMContentLoaded", () => {
    //listens for the dynamically added elements
    document.body.addEventListener("click", (event) => {
        if (event.target.matches(".btn-primary")) {
            openModal(event.target);
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("results").addEventListener("click", function (event) {
        if (event.target.classList.contains("btn-primary")) {
            let movieCard = event.target.closest(".movie-card");

            let title = movieCard.querySelector("h5").textContent;
            let posterUrl = movieCard.querySelector("img").src;
            let genres = movieCard.querySelector(".movie-genres").textContent.replace("Genres: ", "");
            let rating = movieCard.querySelector(".movie-rating").textContent.replace("Rating: ", "");
            
            let overview = movieCard.getAttribute("data-overview") || "No overview available.";
            let streamingServices = movieCard.getAttribute("data-streaming") || "Not available on streaming platforms.";

            //populate the modal with movie details
            document.getElementById("modalTitle").textContent = title;
            document.getElementById("modalPoster").src = posterUrl;
            document.getElementById("modalGenres").textContent = `Genres: ${genres}`;
            document.getElementById("modalRating").textContent = `Rating: ${rating}`;
            document.getElementById("modalOverview").textContent = `Overview: ${overview}`;
            document.getElementById("modalStreaming").textContent = `Streaming Services: ${streamingServices}`;

            // Show the modal
            var movieModal = new bootstrap.Modal(document.getElementById("movieModal"));
            movieModal.show();
        }
    });
});
