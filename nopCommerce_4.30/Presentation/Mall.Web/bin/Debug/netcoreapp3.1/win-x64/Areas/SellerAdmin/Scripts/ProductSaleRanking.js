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
            if (1 == type) {
                var t1 = '<span style="text-align:left;">商品：<b style="color:yellow;font-size:14px;">' + mapName[params[0][1] - 1] + '</b></span>';
                var t2 = '<span style="text-align:left;">销售量：<b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>个</span>';
                html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
            } else {
                var t1 = '<span style="text-align:left;">商品：<b style="color:yellow;font-size:14px;">' + mapName[params[0][1] - 1] + '</b></span>';
                var t2 = '<span style="text-align:left;">销售额：<b style="color:yellow;font-size:14px;">' + params[0][2] + '</b>元</span>';
                html = ['<div style="text-align:left;">', t1, '<br />', t2, '</div>'];
            }

            return html.join('');
        }
    },
    legend: {
        data: ['2']
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
            type: 'bar',
            data: [],
            smooth: true,
            symbol: 'emptyCircle',
            markPoint: {
                data: [
                    { type: 'max', name: '最多' },
                    { type: 'min', name: '最少' }
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
	$("#queryType").change(function () {
        var type = parseInt($(this).val());
        switch (type) {
            case 0: $("#inputStartDate").show(); $("#yearDrop").hide(); $("#monthDrop").hide(); $("#weekDrop").hide(); break; //按天
            case 1: $("#inputStartDate").hide(); $("#yearDrop").show(); $("#monthDrop").show(); $("#weekDrop").show(); break; //按周
            case 2: $("#inputStartDate").hide(); $("#yearDrop").show(); $("#monthDrop").show(); $("#weekDrop").hide(); break; //按月
        }
    });
	$("#yearDrop,#monthDrop").change(function () {
        if ($("#weekDrop").is(":hidden")) return;
        else {
            var month = $("#monthDrop").val();
            var year = $("#yearDrop").val();
            var loading = showLoading();
            ajaxRequest({
                type: 'GET',
                url: "./GetWeekList",
                param: { year: year, month: month },
                dataType: "json",
                success: function (data) {
                    loading.close();
                    if (data.success == true) {
                        $("#weekDrop").empty();
                        var html = [];
                        for (var i = 0; i < data.week.length; i++) {
                            html.push('<option value="' + data.week[i].Value + '">');
                            html.push(data.week[i].Text);
                            html.push('</option>');
                        }
                        $("#weekDrop").append($(html.join("")));
                    }
                }
            });
        }
    });
    $('#inputDate').daterangepicker({ format: 'YYYY-MM-DD',opens:'left',startDate:$("[name=begin]").val() }, function (begin, end) {
        $("[name=begin]").val(begin.format("YYYY-MM-DD"));
        $("[name=end]").val(end.format("YYYY-MM-DD"));
        load();
    });

    $("#SearchBtn").click(function () {
        $('#inputDate').val('');//清空时间
        $("[name=begin]").val("");
        $("[name=end]").val("");
        load();
    });

    $(".btn-default").click(function () {
        $(".btn-default").each(function () {
            $(this).removeClass('active');
        });
        $(this).addClass('active');
        load();
    });

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
});

function load() {
    myChart.showLoading({ text: '正在加载图表...', effect: 'bubble', textStyle: { fontSize: 20 } });
    var params = {
        year: $("#yearDrop").is(":hidden") ? 0 : $("#yearDrop").val(),
        month: $("#monthDrop").is(":hidden") ? 0 : $("#monthDrop").val(),
        weekIndex: $("#weekDrop").is(":hidden") ? 0 : $("#weekDrop").val(),
        day: $("#inputStartDate").is(":hidden") ? "" : $("#inputStartDate").val(),
        dimension: $("button.active").val(),
        begin: $("[name=begin]").val(),
        end: $("[name=end]").val()
    };
    $.ajax({
        url: "./GetSaleRankingChart",
        data: params,
        success: function (data) {
            myChart.hideLoading();
            if (data.success) {
                option.series[0].data = [];
                option.xAxis[0].data = data.chart.XAxisData;
                option.series[0].data = data.chart.SeriesData[0].Data;
                option.series[0].name = data.chart.SeriesData[0].Name;
                option.legend.data[0] = data.chart.SeriesData[0].Name;
                var _mapName = [];
                if (data.chart.ExpandProp.length > 0) {
                    _mapName = data.chart.ExpandProp.map(function (item, index) {
                        return item == null ? "" : item;
                    });
                }
                mapName = _mapName;

                myChart.hideLoading();
                myChart.setOption(option);
            }
        }, error: function () {
            myChart.hideLoading();
        }
    });
}