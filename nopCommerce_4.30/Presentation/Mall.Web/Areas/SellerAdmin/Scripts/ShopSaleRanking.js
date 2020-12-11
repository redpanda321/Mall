var myChart;
require.config({ paths: { echarts: '/Scripts' } });
require(['echarts', 'echarts/chart/bar', 'echarts/chart/line', 'echarts/chart/map'], function (echarts) {
    myChart = echarts.init(document.getElementById('main'));
    load();
});
var option = {
    tooltip: {
        trigger: 'axis',
        formatter: function (params, ticket, callback) {
            var type = $("button.active").val();
            var html = '';
            var t1 = '<span style="text-align:left;">' + params[0][1] + '销售额：<b style="color:yellow;font-size:14px;">' + params[0][2] + '元</b></span>';
            html = ['<div style="text-align:left;">', t1, '</div>'];
            return html.join('');
        }
    },
    legend: {
        data: ['店铺交易额走势图']
    },
    toolbox: {
        show: true,
        feature: {
            magicType: { show: true, type: ['line', 'bar'] },
            restore: { show: true },
            saveAsImage: { show: true }
        }
    },
    calculable: true,
    xAxis: [
        {
            type: 'category',
            data: []
        }
    ],
    yAxis: [
        {
            type: 'value',
            splitArea: { show: true }
        }
    ],
    series: [
        {
            name: '123',
            type: 'line',
            data: [],
            smooth: true,
            symbol: 'emptyCircle',
            markPoint: {
                data: [
                    { type: 'max', name: '最高' },
                    { type: 'min', name: '最低' }
                ]
            },
            markLine: {
                data: [
                    { type: 'average', name: '平均值' }
                ]
            }
        }
    ]
};

$(function () {
    $("#SearchBtn").click(function () { load(); });
    $("#dimensionType li").click(function () {
        $("#dimensionType li").each(function () {
            $(this).removeClass('active');
        });
        $(this).addClass('active');
        load();
    });
});

function load() {
    myChart.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    var dimension = $("#dimensionType .active").attr("data");
    var url = "/SellerAdmin/Billing/GetSevenDaysTradeChart";
    if (dimension == 1)
        url = "/SellerAdmin/Billing/GetSevenDaysTradeChart";
    else if (dimension == 2)
        url = "/SellerAdmin/Billing/GetThirdtyDaysTradeChart";
    else if (dimension == 3)
        url = "/SellerAdmin/Billing/GetTradeChartMonthChart";

    $.ajax({
        type: 'post',
        url: url,
        success: function (data) {
            myChart.hideLoading();
            if (data.success) {
                data.chart.SeriesData[0].Name = '店铺交易额走势图';
                option.series[0].data = [];
                option.xAxis[0].data = data.chart.XAxisData;
                option.series[0].data = data.chart.SeriesData[0].Data;
                option.series[0].name = data.chart.SeriesData[0].Name;
                myChart.clear();
                myChart.setOption(option);
            }
        }, error: function () {
            myChart.hideLoading();
        }
    });
}

