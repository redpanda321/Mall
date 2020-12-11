// JavaScript source code

function checkdata() {
    if ($.trim($("#txtProductName").val()) == "") {
        $.dialog.errorTips("商品不能为空 ");
        return false;
    }
    if ($.trim($("#txtBeginDate").val()) >= $.trim($("#txtEndDate").val())) {
        $.dialog.errorTips('开始时间不能大于或等于结束时间！');
        return false;
    }
    if ($.trim($("#txtEndDate").val()) == "") {
        $.dialog.errorTips("结束时间不能为空 ");
        return false;
    }
    var count = $.trim($("#txtCount").val());
    if (count == "") {
        $.dialog.errorTips("限制数量不能为空 ");
        return false;
    }
    if (count.length < 1 || !/^\d+$/.test(count)) {
        $.dialog.tips('错误的限制数量！');
        return;
    }
    if (parseInt(count) <= 0) {
        $.dialog.errorTips("限制数量必须大于0 ");
        return false;
    }

    var eachResult = true;
    var eachMsg = "";
    var priceTexts = $("#tbl input[type=text][t=salePrice]");
    for(var i=0;i<priceTexts.length;i++){
        var item = priceTexts[i];
        var p = $.trim($(item).val());
        if (p == "") {
            eachMsg = "价格不能为空 ";
            eachResult = false;
            $(item).focus();
            break;
        }

        if (isNaN(parseFloat(p))) {
            eachMsg = "价格只能为数字 ";
            eachResult = false;
            $(item).focus();
            break;
        }

        if (parseFloat(p) < 0.01) {
            eachMsg = "价格不能小于0.01 ";
            eachResult = false;
            $(item).focus();
            break;
        }

        var oldP = $(item).parent().prev().html();
        if (parseFloat(p) > parseFloat(oldP)) {
            eachMsg = "价格不能大于原始价格 ";
            eachResult = false;
            $(item).focus();
            break;
        }
    }

    if (!eachResult) {
        $.dialog.errorTips(eachMsg);
        return false;
    }
       
    eachMsg = "";
    eachResult = true;

    var totalCountTexts = $("#tbl input[type=text][t=totalCount]");
    var totalCountReg = /^[0-9]\d*$/;
    var sumTotalCount = 0;
    for (var y = 0; y < totalCountTexts.length; y++) {
        var item = totalCountTexts[y];
        var p = $.trim($(item).val());
        if (p == "") {
            eachMsg = "活动库存不能为空 ";
            eachResult = false;
            $(item).focus();
            break;
        }
     
        if (!totalCountReg.test(p)) {
            eachMsg = "活动库存只能是大于等于0的正整数 ";
            eachResult = false;
            $(item).focus();
            break;
        }

        var oldP = $(item).parent().prev().html();
        if (parseInt(p) > parseInt(oldP)) {
            eachMsg = "活动库存不能大于商品库存 ";
            eachResult = false;
            $(item).focus();
            break;
        }

        sumTotalCount += parseInt(p);
    }

    if (!eachResult) {
        $.dialog.errorTips(eachMsg);
        return false;
    }

    if (sumTotalCount == 0) {
        $.dialog.errorTips("总的活动库存必须大于0");
        return false;
    }

    return true;
}

var p_data = null;
$(function () {
    initDate();

    $("#SelectProduct").click(function () {
        $.productSelector.params.isShopCategory = true;
        $.productSelector.show(null, function (selectedProducts) {
            console.log(selectedProducts);

            $.post("./IsAdd", { productId: selectedProducts[0].id }, function (result) {
                if (result) {
                    $("#txtProductId").val(selectedProducts[0].id);
                    $("#txtProductName").val(selectedProducts[0].name);
                    skuShow(selectedProducts[0].id);
                }
                else {
                    $.dialog.errorTips("此商品已参与限时购或其他活动");
                }
            })

        }, 'selleradmin', false);
    });

    $("#submit").click(function () {
        if ($("#txtCategoryName option").size() == 0) {
            $.dialog.errorTips("平台还未设置限时购活动分类");
            return;
        }
        var loading = showLoading();
        //$(this).attr("disabled", "");
        if (!checkdata()) {
            //$(this).removeAttr("disabled");
            loading.close();
            return;
        }
        p_data.BeginDate = $("#txtBeginDate").val();
        p_data.EndDate = $("#txtEndDate").val();
        p_data.CategoryName = $("#txtCategoryName").val();
        p_data.LimitCountOfThePeople = $("#txtCount").val();
        p_data.ProductId = $("#txtProductId").val();
        p_data.Title = $("#txtTitle").val();

        for (var i = 0; i < p_data.Details.length; i++) {
            var price = $("#tbl input[t=salePrice]:eq(" + i + ")").val();
            var totalCount = $("#tbl input[t=totalCount]:eq(" + i + ")").val();
            p_data.Details[i].Price = parseFloat(price);
            p_data.Details[i].TotalCount = parseInt(totalCount);
        }

        $.post("./AddFS", { fsmodel: JSON.stringify(p_data) }, function (result) {
            if (result.success) {
                loading.close();
                $.dialog.succeedTips("保存成功！", function () {
                    window.location.href = $("#UAm").val();
                }, 0.5);
            }
            else {
                $.dialog.errorTips(result.msg);
                //$(this).removeAttr("disabled");
            }
            loading.close();
            //$(this).removeAttr("disabled");
        })
        console.log(p_data);
    })

})

function skuShow(productid) {
    $.post("./GetDetailInfo", { productId: productid }, function (result) {
        p_data = result;
        var html = "";
        for (var i = 0; i < result.Details.length; i++) {
            if (i == 0 && result.Details.length > 1) {
                html += "<tr><td rowspan='" + result.Details.length + "' style='text-align:center'><img src='" + result.ProductImg + "'/></td>";
                html += "<td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' t='salePrice' class='form-control input-sm input-int-num' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td>";
                html += "<td><input type='text' t='totalCount' class='form-control input-sm input-int-num' value='" + result.Details[i].Stock + "'/></td></tr>";
            }
            else if (i == 0 && result.Details.length == 1) {
                html += "<tr><td style='text-align:center'><img src='" + result.ProductImg + "'/></td>";
                html += "<td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' t='salePrice' class='form-control input-sm input-int-num' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td>";
                html += "<td><input type='text' t='totalCount' class='form-control input-sm input-int-num' value='" + result.Details[i].Stock + "'/></td></tr>";
            }
            else {
                html += "<tr><td>" + (result.Details[i].Color == null ? "" : result.Details[i].Color) + " " + (result.Details[i].Size == null ? "" : result.Details[i].Size) + " " + (result.Details[i].Version == null ? "" : result.Details[i].Version) + "</td>";
                html += "<td>" + result.Details[i].SalePrice + "</td>";
                html += "<td><input type='text' t='salePrice' class='form-control input-sm input-int-num' value='" + result.Details[i].SalePrice + "'/></td>";
                html += "<td>" + result.Details[i].Stock + "</td>";
                html += "<td><input type='text' t='totalCount' class='form-control input-sm input-int-num' value='" + result.Details[i].Stock + "'/></td></tr>";
            }
        }

        $("#tbl tbody").html(html);
    })
}

function initDate() {
    $(".start_datetime").val($("#DNT0").val());

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1,
        minView: 0
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd hh:ii:ss',
        autoclose: true,
        weekStart: 1,
        minView: 0
    });
    $('.end_datetime').datetimepicker('setEndDate', $("#VBET").val());
    $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    $('.start_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    $('.start_datetime').datetimepicker('setEndDate', $("#VBET").val());


    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });
    $('.end_datetime').on('changeDate', function () {
        $('.start_datetime').datetimepicker('setEndDate', $(".end_datetime").val());
    });
}