(function (window, document) {
    document.addEventListener('DOMContentLoaded', function () {
        const data = window.usageSummaryData || [];
        if (!data.length) return;

        const labels = data.map(x => x.ServiceName);
        const monthlyCounts = data.map(x => x.MonthlyClicks);
        const lifetimeCounts = data.map(x => x.LifetimeClicks);

        new Chart(document.getElementById('monthlyChart'), {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: monthlyCounts,
                    label: 'Last 30 Days'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Last 30 Days Usage',
                        font: { size: 24 },
                        padding: { top: 20, bottom: 20 }
                    },
                    legend: {
                        labels: {
                            font: { size: 18 },
                            padding: 20
                        }
                    }
                }
            }
        });

        new Chart(document.getElementById('lifetimeChart'), {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: lifetimeCounts,
                    label: 'Lifetime'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Lifetime Usage',
                        font: { size: 24 },
                        padding: { top: 20, bottom: 20 }
                    },
                    legend: {
                        labels: {
                            font: { size: 18 },
                            padding: 20
                        }
                    }
                }
            }
        });
    });
})(window, document);
