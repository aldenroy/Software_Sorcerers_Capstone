document.addEventListener('DOMContentLoaded', () => {
    const links = document.querySelectorAll('.subscription-link');

    function getRequestVerificationToken() {
        const meta = document.querySelector('meta[name="RequestVerificationToken"]');
        return meta ? meta.getAttribute('content') : '';
    }

    links.forEach(link => {
        link.addEventListener('click', () => {
            const item = link.closest('.subscription-item');
            if (!item) return;

            const serviceId = item.dataset.serviceId;
            if (!serviceId) return;

            fetch('/User/TrackSubscriptionClick', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getRequestVerificationToken()
                },
                body: JSON.stringify({ streamingServiceId: parseInt(serviceId, 10) })
            })
                .then(res => {
                    if (!res.ok) {
                        console.error('Failed to record click:', res.statusText);
                    }
                })
                .catch(err => console.error('Error in tracking subscription click:', err));
        });
    });
});
