/**
 * @jest-environment jsdom
 */

const loadScript = () => require('../MoviesMadeEasy/wwwroot/js/usageSummaryCharts.js')
let ChartMock

beforeAll(() => {
  ChartMock = jest.fn()
  global.Chart = ChartMock
  loadScript()
})

beforeEach(() => {
  ChartMock.mockClear()
  document.body.innerHTML = `
    <canvas id="monthlyChart"></canvas>
    <canvas id="lifetimeChart"></canvas>
  `
})

describe('usageCharts.js', () => {
  test('does nothing if usageSummaryData is undefined', () => {
    delete window.usageSummaryData
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(0)
  })

  test('does nothing if usageSummaryData is null', () => {
    window.usageSummaryData = null
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(0)
  })

  test('does nothing if usageSummaryData is empty array', () => {
    window.usageSummaryData = []
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(0)
  })

  test('creates two pie charts with correct labels and data for multiple entries', () => {
    window.usageSummaryData = [
      { ServiceName: 'S1', MonthlyClicks: 1, LifetimeClicks: 5 },
      { ServiceName: 'S2', MonthlyClicks: 2, LifetimeClicks: 10 },
      { ServiceName: 'S3', MonthlyClicks: 0, LifetimeClicks: 0 }
    ]
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(2)
    const [monthlyEl, monthlyCfg] = ChartMock.mock.calls[0]
    expect(monthlyEl).toBe(document.getElementById('monthlyChart'))
    expect(monthlyCfg.type).toBe('pie')
    expect(monthlyCfg.data.labels).toEqual(['S1','S2','S3'])
    expect(monthlyCfg.data.datasets[0].data).toEqual([1,2,0])
    expect(monthlyCfg.data.datasets[0].label).toBe('Last 30 Days')
    expect(monthlyCfg.options.plugins.title.text).toBe('Last 30 Days Usage')
    const [lifetimeEl, lifetimeCfg] = ChartMock.mock.calls[1]
    expect(lifetimeEl).toBe(document.getElementById('lifetimeChart'))
    expect(lifetimeCfg.data.labels).toEqual(['S1','S2','S3'])
    expect(lifetimeCfg.data.datasets[0].data).toEqual([5,10,0])
    expect(lifetimeCfg.data.datasets[0].label).toBe('Lifetime')
    expect(lifetimeCfg.options.plugins.title.text).toBe('Lifetime Usage')
  })

  test('handles missing ServiceName or click counts gracefully', () => {
    window.usageSummaryData = [
      { MonthlyClicks: 3, LifetimeClicks: 7 },
      { ServiceName: 'OnlyName' }
    ]
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(2)
    expect(ChartMock.mock.calls[0][1].data.labels).toEqual([undefined, 'OnlyName'])
    expect(ChartMock.mock.calls[0][1].data.datasets[0].data).toEqual([3, undefined])
    expect(ChartMock.mock.calls[1][1].data.datasets[0].data).toEqual([7, undefined])
  })

  test('still calls Chart if one canvas element is missing', () => {
    window.usageSummaryData = [
      { ServiceName: 'A', MonthlyClicks: 1, LifetimeClicks: 2 }
    ]
    document.body.innerHTML = `<canvas id="monthlyChart"></canvas>`
    document.dispatchEvent(new Event('DOMContentLoaded'))
    expect(ChartMock).toHaveBeenCalledTimes(2)
    expect(ChartMock.mock.calls[0][0]).toBe(document.getElementById('monthlyChart'))
    expect(ChartMock.mock.calls[1][0]).toBeNull()
  })
})
