(function (window, document) {
    const barGlow = {
        id: 'barGlow',
        afterDatasetsDraw(chart, args, opts) {
            const ctx = chart.ctx;
            const active = chart.getActiveElements();
            if (!active.length) return;
            ctx.save();
            ctx.shadowColor = opts.color || 'rgba(0,123,255,0.8)';
            ctx.shadowBlur = opts.blur || 20;
            active.forEach(item => {
                const bar = item.element;
                ctx.fillStyle = 'rgba(0,0,0,0)';
                ctx.fillRect(
                    bar.x - bar.width / 2 - (opts.extraPadding || 0),
                    bar.y - bar.height / 2 - (opts.extraPadding || 0),
                    bar.width + (opts.extraPadding || 0) * 2,
                    bar.height + (opts.extraPadding || 0) * 2
                );
            });
            ctx.restore();
        }
    };
    Chart.register(barGlow);

    const styles = getComputedStyle(document.documentElement);
    Chart.defaults.color = styles.getPropertyValue('--chart-label-color').trim();
    Chart.defaults.plugins.title.color = styles.getPropertyValue('--chart-title-color').trim();
    Chart.defaults.plugins.legend.labels.color = styles.getPropertyValue('--chart-label-color').trim();

    function getPalette(n) {
        const colors = [];
        for (let i = 0; i < n; i++) {
            const hue = Math.round(360 * i / n);
            colors.push(`hsl(${hue},65%,50%)`);
        }
        return colors;
    }

    function generateLegend(labels, colors, charts) {
        const container = document.getElementById('chart-legend');
        if (!container) return;
        container.innerHTML = '';
        labels.forEach((label, idx) => {
            const li = document.createElement('li');
            li.className = 'legend-item';
            li.innerHTML = `<span class="legend-box" style="background:${colors[idx]}"></span>${label}`;
            li.addEventListener('mouseenter', () => {
                charts.forEach(ch => {
                    if (!ch) return;
                    const i = ch.data.labels.indexOf(label);
                    if (i !== -1) {
                        ch.setActiveElements([{ datasetIndex: 0, index: i }]);
                        ch.update();
                    }
                });
            });
            li.addEventListener('mouseleave', () => {
                charts.forEach(ch => {
                    if (!ch) return;
                    ch.setActiveElements([]);
                    ch.update();
                });
            });
            container.appendChild(li);
        });
    }

    function createPieChart(ctx, labels, data, titleText, colors) {
        return new Chart(ctx, {
            type: 'pie',
            data: { labels, datasets: [{ data, backgroundColor: colors, hoverOffset: 20 }] },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: { padding: 20 },
                plugins: {
                    title: { display: true, text: titleText, font: { size: 24 }, padding: { top: 20, bottom: 20 } },
                    legend: { display: false }
                }
            }
        });
    }

    function createBarChart(ctx, labels, data, titleText, barColors) {
        return new Chart(ctx, {
            type: 'bar',
            data: { labels, datasets: [{ data, backgroundColor: barColors, borderRadius: 4 }] },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                hover: { mode: 'nearest', intersect: true },
                plugins: {
                    barGlow: { color: 'rgba(0,123,255,0.8)', blur: 20, extraPadding: 4 },
                    title: { display: true, text: titleText, font: { size: 24 }, padding: { top: 20, bottom: 20 } },
                    legend: { display: false },
                    tooltip: { callbacks: { label: ctx => `$${ctx.parsed.x.toFixed(2)} per click` } }
                },
                scales: {
                    x: {
                        title: { display: true, text: 'Cost per Click ($)', font: { size: 20 } },
                        ticks: { font: { size: 16 } }
                    },
                    y: {
                        ticks: { display: false },
                        grid: { display: false }     
                    }
                }
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        const data = window.usageSummaryData;
        if (!Array.isArray(data) || !data.length) return;

        const labels = data.map(x => x.ServiceName);
        const palette = getPalette(labels.length);
        const monthly = data.map(x => x.MonthlyClicks);
        const lifetime = data.map(x => x.LifetimeClicks);
        const cpcData = data
            .filter(d => d.CostPerClick > 0)
            .map(d => ({ name: d.ServiceName, cost: +d.CostPerClick.toFixed(2) }))
            .sort((a, b) => a.cost - b.cost);

        const charts = [];

        const elM = document.getElementById('monthlyChart');
        if (elM && elM.getContext) {
            charts.push(createPieChart(elM.getContext('2d'), labels, monthly, 'Last 30 Days Usage', palette));
        }

        const elL = document.getElementById('lifetimeChart');
        if (elL && elL.getContext) {
            charts.push(createPieChart(elL.getContext('2d'), labels, lifetime, 'Lifetime Usage', palette));
        }

        if (cpcData.length) {
            const elC = document.getElementById('costPerClickChart');
            if (elC && elC.getContext) {
                const names = cpcData.map(d => d.name);
                const costs = cpcData.map(d => d.cost);
                const barCols = names.map(n => palette[labels.indexOf(n)]);
                charts.push(createBarChart(elC.getContext('2d'), names, costs, 'Cost per Click', barCols));
            }
        }

        generateLegend(labels, palette, charts);
    });
})(window, document);
