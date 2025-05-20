(function (window, document) {
    function createPieChart(canvasId, labels, data, datasetLabel, titleText) {
        new Chart(document.getElementById(canvasId), {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    label: datasetLabel
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: titleText,
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
    }

    document.addEventListener('DOMContentLoaded', function () {
        const data = window.usageSummaryData || [];
        if (!data.length) return;

        const labels = data.map(x => x.ServiceName);
        const monthlyCounts = data.map(x => x.MonthlyClicks);
        const lifetimeCounts = data.map(x => x.LifetimeClicks);

        createPieChart('monthlyChart', labels, monthlyCounts, 'Last 30 Days', 'Last 30 Days Usage');
        createPieChart('lifetimeChart', labels, lifetimeCounts, 'Lifetime', 'Lifetime Usage');
    });
})(window, document);
