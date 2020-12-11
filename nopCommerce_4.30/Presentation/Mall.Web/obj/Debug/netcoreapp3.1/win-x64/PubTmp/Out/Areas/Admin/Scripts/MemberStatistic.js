var myChart;
require.config({ paths: { echarts: '/Scripts' } });
require(['echarts', 'echarts/chart/bar', 'echarts/chart/line', 'echarts/chart/map'], function (echarts) {
    myChart = echarts.init(document.getElementById('main'));
    LoadChart();
});
var option = {
    tooltip: {
        trigger: 'axis',
        formatter: function (params, ticket, callback) {
            var t1 = params[0][0].replace('月', '月' + params[0][1] + '号') + '  :  <b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>  人';
            var t2 = params[1][0].replace('月', '月' + params[1][1] + '号') + '  :  <b style="color:yellow;font-size:14px;">' + params[1][2] + '</b>  人';
            var html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
            return html.join('');
        }
    },
    legend: {},
    toolbox: {
        show: true,
        feature: {
            magicType: { show: true, type: ['line', 'bar'] },
            restore: { show: true },
            saveAsImage: { show: true }
        }
    },
    calculable: true,
    xAxis: [{ type: 'category' }],
    yAxis: [{ type: 'value', axisLabel: { formatter: '{value} 人' }, splitArea: { show: true } }],
    series: []
};
$(function () {
    $('#inputStartDate').daterangepicker({ format: 'YYYY-MM-DD',startDate:$("[name=begin]").val() }, function (begin, end) {
        $("[name=begin]").val(begin.format("YYYY-MM-DD"));
        $("[name=end]").val(end.format("YYYY-MM-DD"));
        LoadChart();
    });
    $("#SearchBtn").click(function () {
        $('#inputStartDate').val('');//清空时间
        $("[name=begin]").val('');
        $("[name=end]").val('');
        LoadChart();
    });
});

function LoadChart() {
    var params = {
        year: $("#year").val(),
        month: $("#month").val(),
        begin: $("[name=begin]").val(),
        end: $("[name=end]").val()
    };
    myChart.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    $.ajax({
        url: "./GetMemberChartByMonth",
        data: params,
        success: function (data) {
            myChart.hideLoading();
            if (data.success) {
                option.xAxis[0].data = data.chart.XAxisData;
                option.series = [];
                $("tr.info").hide();
                if (data.chart.SeriesData.length > 1) {
                    option.series.push(createLine(data.chart.SeriesData[0]));
                    option.series.push(createLine(data.chart.SeriesData[1]));
                    option.legend.data = [option.series[0].name, option.series[1].name];
                    $("#lblCurMonth").text("{0}:{1}".format(
                        option.series[0].name,
                        option.series[0].data.sum(function (p) { return p; })));

                    $("#lblPrevMonth").text("{0}:{1}".format(
                        option.series[1].name,
                        option.series[1].data.sum(function (p) { return p; })));
                    $("tr.lbl1").show();
                } else {
                    option.series.push(createLine(data.chart.SeriesData[0]));
                    option.legend.data = [option.series[0].name];
                    $("#lblDate").text("{0}:{1}".format(
                        option.series[0].name,
                        option.series[0].data.sum(function (p) { return p; })));
                    $("tr.lbl2").show();
                }
                myChart.clear();
                myChart.setOption(option);
            }
        }, error: function () {
            myChart.hideLoading();
        }
    });
}

function ExportExeclByMonth() {
    var year = $("#year").val();
    var month = $("#month").val();
    var href = "/Admin/Statistics/ExportMemberByMonth?year=" + year + "&month=" + month;
    $("#aMonthExport").attr("href", href);
}

function createLine(SeriesData) {
    return {
        name: SeriesData.Name,
        data: SeriesData.Data,
        type: 'line',
        smooth: true,
        symbol: 'emptyCircle',
        markPoint: { data: [{ type: 'max', name: '最多人数' }, { type: 'min', name: '最少人数' }] },
        markLine: { data: [{ type: 'average', name: '平均值' }] }
    };
}