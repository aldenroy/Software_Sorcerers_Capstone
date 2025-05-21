/**
 * @jest-environment jsdom
 */

const loadScript = () => require('../MoviesMadeEasy/wwwroot/js/usageSummaryCharts.js')
let ChartMock

beforeAll(() => {
  ChartMock = jest.fn()
  ChartMock.register = jest.fn()
  ChartMock.defaults = {
    plugins: {
      title: {},
      legend: { labels: {} }
    }
  }
  global.Chart = ChartMock
  loadScript()
})

beforeEach(() => {
  ChartMock.mockClear()
  document.body.innerHTML = `
    <canvas id="monthlyChart"></canvas>
    <canvas id="lifetimeChart"></canvas>
    <ul id="chart-legend"></ul>
  `
})

describe('usageSummaryCharts.js', () => {
  test('does nothing if usageSummaryData is undefined', () => {
    delete window.usageSummaryData
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).not.toHaveBeenCalled()
  })

  test('does nothing if usageSummaryData is null or empty', () => {
    window.usageSummaryData = null
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).not.toHaveBeenCalled()

    ChartMock.mockClear()
    window.usageSummaryData = []
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).not.toHaveBeenCalled()
  })

  test('creates two pie charts with correct labels and data', () => {
    window.usageSummaryData = [
      { ServiceName: 'S1', MonthlyClicks: 1, LifetimeClicks: 5 },
      { ServiceName: 'S2', MonthlyClicks: 2, LifetimeClicks: 10 },
      { ServiceName: 'S3', MonthlyClicks: 0, LifetimeClicks: 0 }
    ]
    document.dispatchEvent(new Event('DOMContentLoaded'))

    expect(ChartMock).toHaveBeenCalledTimes(2)

    const [ctxM, cfgM] = ChartMock.mock.calls[0]
    expect(ctxM).toBe(document.getElementById('monthlyChart').getContext('2d'))
    expect(cfgM.type).toBe('pie')
    expect(cfgM.data.labels).toEqual(['S1','S2','S3'])
    expect(cfgM.data.datasets[0].data).toEqual([1,2,0])
    expect(cfgM.data.datasets[0].hoverOffset).toBe(20)
    expect(cfgM.options.plugins.title.text).toBe('Last 30 Days Usage')

    const [ctxL, cfgL] = ChartMock.mock.calls[1]
    expect(ctxL).toBe(document.getElementById('lifetimeChart').getContext('2d'))
    expect(cfgL.type).toBe('pie')
    expect(cfgL.data.labels).toEqual(['S1','S2','S3'])
    expect(cfgL.data.datasets[0].data).toEqual([5,10,0])
    expect(cfgL.data.datasets[0].hoverOffset).toBe(20)
    expect(cfgL.options.plugins.title.text).toBe('Lifetime Usage')
  })

  test('handles missing ServiceName or click counts gracefully', () => {
    window.usageSummaryData = [
      { MonthlyClicks: 3, LifetimeClicks: 7 },
      { ServiceName: 'OnlyName' }
    ]
    document.dispatchEvent(new Event('DOMContentLoaded'))

    expect(ChartMock).toHaveBeenCalledTimes(2)

    const [, cfgM2] = ChartMock.mock.calls[0]
    expect(cfgM2.data.labels).toEqual([undefined, 'OnlyName'])
    expect(cfgM2.data.datasets[0].data).toEqual([3, undefined])

    const [, cfgL2] = ChartMock.mock.calls[1]
    expect(cfgL2.data.datasets[0].data).toEqual([7, undefined])
  })

  test('skips missing canvases without crashing', () => {
    window.usageSummaryData = [
      { ServiceName: 'A', MonthlyClicks: 1, LifetimeClicks: 2 }
    ]
    document.body.innerHTML = `
      <canvas id="monthlyChart"></canvas>
      <ul id="chart-legend"></ul>
    `
    expect(() => document.dispatchEvent(new Event('DOMContentLoaded'))).not.toThrow()
    expect(ChartMock).toHaveBeenCalledTimes(1)
    const [ctxOnly] = ChartMock.mock.calls[0]
    expect(ctxOnly).toBe(document.getElementById('monthlyChart').getContext('2d'))
  })

  test('service colors match across pie and bar charts', () => {
    window.usageSummaryData = [
      { ServiceName: 'A', MonthlyClicks: 1, LifetimeClicks: 2, CostPerClick: 3 },
      { ServiceName: 'B', MonthlyClicks: 2, LifetimeClicks: 4, CostPerClick: 1 },
      { ServiceName: 'C', MonthlyClicks: 3, LifetimeClicks: 6, CostPerClick: 2 }
    ]
    document.body.innerHTML += `<canvas id="costPerClickChart"></canvas>`
    document.dispatchEvent(new Event('DOMContentLoaded'))

    expect(ChartMock).toHaveBeenCalledTimes(3)

    const palette = ChartMock.mock.calls[0][1].data.datasets[0].backgroundColor
    const palette2 = ChartMock.mock.calls[1][1].data.datasets[0].backgroundColor
    expect(palette2).toEqual(palette)

    const barColors = ChartMock.mock.calls[2][1].data.datasets[0].backgroundColor
    const expectedOrder = ['B','C','A']
    const expectedBarColors = expectedOrder.map(name => {
      const idx = ['A','B','C'].indexOf(name)
      return palette[idx]
    })
    expect(barColors).toEqual(expectedBarColors)
  })
})
