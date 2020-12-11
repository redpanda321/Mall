var loadingTicket;
var myLineChart;
var lineChartLineNum = 5;//显示的网格线条数
var lineOption = {};
var lineLegendData = ['付款金额', '付款人数', '付款件数', '下单转化率', '付款转化率', '成交转化率'];
var lineLegendSeries = [];
var lineLegendSeriesMax = 100;
var defaultDate = new Date();
defaultDate.setDate(defaultDate.getDate() - 1);
var endDate = startDate = defaultDate;

$(function () {
    var countData = [];
    var amountData = [];
    var lineSeriesData = [];
    var funnelSeriesData = [];
    lineOption = {
        title: {
            text: ''
        },
        tooltip: {
            trigger: 'axis',
            formatter:function(a){
            	var str='';
            	a.forEach(function(item){
            		str+=item.seriesName+':'+item.value+(item.seriesIndex>2?'%':'')+'<br/>';
            	});
            	return str;
            }
            //formatter: '{b}<br/>{a0}:{c0}<br/>{a1}:{c1}<br/>{a2}:{c2}<br/>{a3}:{c3}%<br/>{a4}:{c4}%<br/>{a5}:{c5}%'
        },
        legend: {
            orient: 'horizontal',
            bottom: 20,
            icon: 'circle',
            data: lineLegendData
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '65',
            containLabel: true
        },
        //toolbox: {
        //    feature: {
        //        saveAsImage: {}
        //    }
        //},
        xAxis: {
            type: 'category',
            axisLabel: {
                rotate: 30
            },
            boundaryGap: false,
            data: []
        },
        yAxis: [
         {
             type: 'value',
             name: '金额',
             min: 0,
             max: 250,
             interval: 50
         },
         {
             type: 'value',
             name: '转化率',
             min: 0,
             max: 25,
             interval: 5,
             axisLabel: {
                 formatter: '{value}%'
             }
         }
        ],
        series: [
            {
                name: '付款金额',
                type: 'line',
                data: []
            },
            {
                name: '付款人数',
                type: 'line',
                data: []
            },
            {
                name: '付款件数',
                type: 'line',
                data: []
            },
            {
                name: '下单转化率',
                type: 'line',
                yAxisIndex: 1,
                data: []
            },
            {
                name: '付款转化率',
                type: 'line',
                yAxisIndex: 1,
                data: []
            },
            {
                name: '成交转化率',
                type: 'line',
                yAxisIndex: 1,
                data: []
            }
        ]
    };

    bindChart(lineOption);
    $('#defaultBtn').click();

    $('#inputStartDate').daterangepicker(
        {
            format: 'YYYY-MM-DD',
            startDate: defaultDate, endDate: defaultDate
        }
    ).bind('apply.daterangepicker', function (event, obj) {
        if ($('#inputStartDate').val() == "" || $('#inputStartDate').val() == "YYYY/MM/DD — YYYY/MM/DD") {
            return false;
        }
        var strArr = $('#inputStartDate').val().split("-");
        var startDate = strArr[0] + "-" + strArr[1] + "-" + strArr[2];
        var endDate = strArr[3] + "-" + strArr[4] + "-" + strArr[5];
        if (startDate != '' && endDate != '')
            LoadChartData(startDate, endDate);
        else {
            $.dialog.tips('请选择时间范围');
        }
    });
    $('#shopBranchId').change(function () {
        LoadChartData(startDate, endDate);
    });
});

function bindChart(lineop) {
    myLineChart = echarts.init(document.getElementById('mainChartLine'));
    myLineChart.showLoading({
        text: '正在加载图表...',
        effect: 'bubble',
        textStyle: {
            fontSize: 20
        }
    });

    clearTimeout(loadingTicket);
    loadingTicket = setTimeout(function () {
        myLineChart.hideLoading();
        myLineChart.setOption(lineop);
    }, 500);
    //注册事件
    myLineChart.on('legendselectchanged', function (param) {
        lineOption.yAxis[0].max = GetY0AxisMax(param.selected);
        lineOption.yAxis[0].interval = lineOption.yAxis[0].max / lineChartLineNum;
        lineOption.yAxis[1].max = GetY1AxisMax(param.selected);
        lineOption.yAxis[1].interval = lineOption.yAxis[1].max / lineChartLineNum;
        myLineChart.setOption(lineOption);
    });
}
function GetY0AxisMax(selected) {
    var maxAxis = 100;
    if (selected['付款金额'] && lineLegendSeries['付款金额']) {
        maxAxis = lineLegendSeries['付款金额'];
    }
    if (selected['付款人数'] && lineLegendSeries['付款人数']) {
        maxAxis = lineLegendSeries['付款人数'] > maxAxis ? lineLegendSeries['付款人数'] : maxAxis;
    }
    if (selected['付款件数'] && lineLegendSeries['付款件数']) {
        maxAxis = lineLegendSeries['付款件数'] > maxAxis ? lineLegendSeries['付款件数'] : maxAxis;
    }
    maxAxis = Math.ceil(maxAxis / lineChartLineNum) * lineChartLineNum;
    return maxAxis;
}
function GetY1AxisMax(selected) {
    var maxAxis = 100;
    if (selected['下单转化率'] && lineLegendSeries['下单转化率']) {
        maxAxis = lineLegendSeries['下单转化率'];
    }
    if (selected['付款转化率'] && lineLegendSeries['付款转化率']) {
        maxAxis = lineLegendSeries['付款转化率'] > maxAxis ? lineLegendSeries['付款转化率'] : maxAxis;
    }
    if (selected['成交转化率'] && lineLegendSeries['成交转化率']) {
        maxAxis = lineLegendSeries['成交转化率'] > maxAxis ? lineLegendSeries['成交转化率'] : maxAxis;
    }
    maxAxis = Math.ceil(maxAxis / lineChartLineNum) * lineChartLineNum;
    return maxAxis;
}
function LoadChartData(date1, date2, type) {
    if (myLineChart && myLineChart.dispose) {
        myLineChart.dispose();
    }
    var _type = 0;
    if (type >= 0)
        _type = type;
    else {
        $(".sdata-trade .pdata a").each(function () {
            var _t = $(this)
            if (_t.attr("class") == "active") {
                _type = _t.data('type');
            }
        });
    }
    var para = {};
    para.startDate = date1;
    para.endDate = date2;
    para.type = _type;
    para.shopbranchId = $('#shopBranchId').val() || null;

    //临时存取日期
    endDate = date2;
    startDate = date1;

    var loading = showLoading();
    ajaxRequest({
        type: 'GET',
        url: "./GetPlatTradeStatistic",
        param: para,
        dataType: "json",
        success: function (data) {
            loading.close();
            if (data.success) {
                //取左Y轴最大值
                var amountyAxisMax = getMax(data.model.ChartModelPayAmounts.SeriesData[0].Data);
                var payyAxisMax = getMax(data.model.ChartModelPayUsers.SeriesData[0].Data);
                var piecesyAxisMax = getMax(data.model.ChartModelPayPieces.SeriesData[0].Data);
                var OrderConversionsRatesMax = getMax(data.model.ChartModelOrderConversionsRates.SeriesData[0].Data);
                var PayConversionsRatesMax = getMax(data.model.ChartModelPayConversionsRates.SeriesData[0].Data);
                var TransactionConversionRateMax = getMax(data.model.ChartModelTransactionConversionRate.SeriesData[0].Data);
                //记录每条折线的Y轴最大值
                lineLegendSeries['付款金额'] = amountyAxisMax;
                lineLegendSeries['付款人数'] = payyAxisMax;
                lineLegendSeries['付款件数'] = piecesyAxisMax;
                lineLegendSeries['下单转化率'] = OrderConversionsRatesMax;
                lineLegendSeries['付款转化率'] = PayConversionsRatesMax;
                lineLegendSeries['成交转化率'] = TransactionConversionRateMax;
                var yAxisMax = GetY0AxisMax(lineLegendSeries);
                var y1AxisMax = GetY1AxisMax(lineLegendSeries);
                lineLegendSeriesMax = yAxisMax;
                //折线图
                lineOption.series[0].data = data.model.ChartModelPayAmounts.SeriesData[0].Data;
                lineOption.series[1].data = data.model.ChartModelPayUsers.SeriesData[0].Data;
                lineOption.series[2].data = data.model.ChartModelPayPieces.SeriesData[0].Data;
                lineOption.series[3].data = data.model.ChartModelOrderConversionsRates.SeriesData[0].Data;
                lineOption.series[4].data = data.model.ChartModelPayConversionsRates.SeriesData[0].Data;
                lineOption.series[5].data = data.model.ChartModelTransactionConversionRate.SeriesData[0].Data;
                lineOption.xAxis.data = data.model.ChartModelPayAmounts.XAxisData;
                lineOption.yAxis[0].max = yAxisMax;
                lineOption.yAxis[1].max = y1AxisMax;

                lineOption.yAxis[0].interval = yAxisMax / lineChartLineNum;
                lineOption.yAxis[1].interval = y1AxisMax / lineChartLineNum;

                bindChart(lineOption);
                bindOtherData(data.model);
            }
        }, error: function () {
            loading.close();
        }
    });
}
function bindOtherData(data) {
    $('#vistiCounts').text(data.VisitCounts);
    $('#orderUserCount').text(data.OrderUserCount);
    $('#orderCount').text(data.OrderCount);
    $('#orderProductCount').text(data.OrderProductCount);
    $('#orderAmount').text(data.OrderAmount.toFixed(2));
    $('#orderPayUserCount').text(data.OrderPayUserCount);
    $('#orderPayCount').text(data.OrderPayCount);
    $('#saleCounts').text(data.SaleCounts);
    $('#saleAmounts').text(data.SaleAmounts.toFixed(2));
    $('#orderConversionsRates').text(data.OrderConversionsRates + '%');
    $('#payConversionsRates').text(data.PayConversionsRates + '%');
    $('#transactionConversionRate').text(data.TransactionConversionRate + '%');
    $('#customPrice').text((data.OrderPayUserCount > 0 ? (data.SaleAmounts / data.OrderPayUserCount) : 0).toFixed(2));
    $('#OrderRefundProductCount').text(data.OrderRefundProductCount);
    $('#OrderRefundAmount').text(data.OrderRefundAmount.toFixed(2));
    $('#OrderRefundRate').text(data.OrderRefundRate + '%');
    $('#UnitPrice').text(data.UnitPrice.toFixed(2));
    $('#JointRate').text(data.JointRate + '%');
}
function getMax(arr) {
    var max = 0;
    if (arr.length) {
        for (var i = 0; i < arr.length; i++) {
            if (max < arr[i])
                max = arr[i];
        }
    }
    return max;
}
function ExportExecl() {
    var href = "/SellerAdmin/Statistics/ExportTradeStatistic?startDate=" + startDate + "&endDate=" + endDate;
    var shopbranchid = $('#shopBranchId').val();
    if (shopbranchid)
        href += "&shopbranchId=" + shopbranchid;
    $("#aMonthExport").attr("href", href);
}
