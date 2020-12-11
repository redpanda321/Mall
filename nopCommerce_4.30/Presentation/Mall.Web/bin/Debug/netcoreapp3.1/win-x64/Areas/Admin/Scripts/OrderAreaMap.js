var myChart;
require.config({ paths: { echarts: '/Scripts' } });
require(['echarts', 'echarts/chart/bar', 'echarts/chart/line', 'echarts/chart/map'], function (echarts) {
    myChart = echarts.init(document.getElementById('mainMap'));
    LoadChart();
});
var option = {
    tooltip: {
        trigger: 'item', formatter: function (params, ticket, callback) {
            var type = parseInt($("button.active").val()) - 1;
            var label1 = ["下单量", "下单金额"];
            var label2 = ["个", "元"];
            var t2 = params[1] + '  :  <b style="color:yellow;font-size:14px;">' + params[2] + '</b>  ' + label2[type];
            var html = ['<div style="text-align:left;">', label1[type], '<br />', t2, '</div>'];
            return html.join('');
        }
    },
    dataRange: {
        min: 0,
        max: 1,
        x: 'left',
        y: 'bottom',
        text: ['高', '低'],           // 文本，默认为数值文本
        calculable: true
    },
    series: [
        {
            type: 'map',
            mapType: 'china',
            roam: false,
            itemStyle: {
                normal: {
                    borderWidth: 1,
                    borderColor: '#FFFFFF',
                    color: '#DEA6ED',
                    label: {
                        show: false
                    }
                }
            },
            data: []
        }
    ]
};

$(function () {
    $("#SearchBtn").click(function () {
        $("#inputStartDate").val('');
        $("[name=begin]").val('');
        $("[name=end]").val('');
        LoadChart();
    });
    $('#inputStartDate').daterangepicker({ format: 'YYYY-MM-DD' }, function (begin, end) {
        $("[name=begin]").val(begin.format("YYYY-MM-DD"));
        $("[name=end]").val(end.format("YYYY-MM-DD"));
        LoadChart();
    });
    $(".btn-group button").click(function () {
        $(".btn-default").each(function () {
            $(this).removeClass('active');
        });
        $(this).addClass('active');
        LoadChart();
    });
});

function LoadChart() {
    myChart.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    var params = {
        dimension: $("button.active").val(),
        year: $("#year option:selected").val(),
        month: $("#month option:selected").val(),
        begin: $("[name=begin]").val(),
        end: $("[name=end]").val()
    };
   
    $.ajax({
        url: "./GetAreaMapBySearch",
        data: params,
        success: function (data) {
            myChart.hideLoading();
            if (data.success) {
                var chart = data.chart;
                option.dataRange.min = 0;
                option.dataRange.max = Math.ceil(chart.maxValue);
                option.series[0].data = chart.data;
                var type = parseInt($("button.active").val());
                var sum = chart.totalValue;
                switch (type) {
                    case 1: $("#diaplayName").text('下单量: '); $("#displayValue").text(sum + ' 个'); break;
                    case 2: $("#diaplayName").text('下单金额: '); $("#displayValue").text(sum + ' 元'); break;
                }
                myChart.clear();
                myChart.setOption(option);
            }
        }, error: function () {
            myChart.hideLoading();
        }
    });
}

function ExportExeclByArea() {
    var year = $("#year option:selected").val();
    var month = $("#month option:selected").val();
    var href = "/Admin/Statistics/ExportAreaMap?year=" + year + "&month=" + month;
    $("#aMonthExport").attr("href", href);
}