


function SellerRemark(id, remark, flag) {
    $.dialog({
        title: '商家备注',
        lock: true,
        id: 'SellerRemark',
        content: document.getElementById("remark-form"),
        padding: '0 40px',
        okVal: '保存',
        init: function () {
            $("#remarkContent").focus();

            $("#remarkContentTip").hide();
            $("#remarkContent").css({ border: '1px solid #ccc' });

            if (remark != null && remark != 'null') {
                $("#remarkContent").val(remark);
            }
            else {
                $("#remarkContent").val('');
                $("#remarkContent").attr("placeholder", "最多可输入200个字");
            }
            if (flag != null && flag != 'null') {
                $("input[name='radflag'][value='" + flag + "']").trigger("click");
            }
            else {
                $("input[name='radflag'][value='1']").trigger("click");
            }
        },
        ok: function () {
            var remark = $("#remarkContent").val();
            var flag = $("input[name='radflag']:checked").val();
            if (remark.trim() == "" || remark.length > 200) {
                $("#remarkContentTip").show();
                $("#remarkContentTip").text("回复内容在200个字符以内！");
                $("#remarkContent").css({ border: '1px solid #f60' });
                return false;
            }
            else {
                $("#remarkContentTip").hide();
                $("#remarkContent").css({ border: '1px solid #ccc' });
            }
            var loading = showLoading();
            $.post("./SellerRemark",
                { id: id, remark: remark, flag: flag },
                function (data) {
                    loading.close();
                    if (data.success) {
                        $.dialog.succeedTips("备注成功", function () {
                            $("#remarkContent").val("");
                            var pageNo = $("#list").MallDatagrid('options').pageNumber;
                            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
                        });
                    }
                    else
                        $.dialog.errorTips("备注失败:" + data.msg);
                });
        }
    });
}


$(function () {
    var status = GetQueryString('status');
    var shopBranchId = GetQueryString('shopBranchId');
    if (status == 1) {
        typeChoose('1');
    }
    else if (status == 2) {
        typeChoose('2');
    } else {
        typeChoose()
    }
    
    function typeChoose(val) {
        var isOpenStore = { field: "门店未授权" };
        if ($("#isOpenStore").val() == "True") {
            isOpenStore = { field: "ShopBranchName", title: "商家/门店", width: 140, align: "center" };
        }
        //订单表格
        $("#list").MallDatagrid({
            url: './list',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "OrderId",
            pageSize: 14,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { status: val, ShopBranchId: shopBranchId },
            rowHeadFormatter: function (target, index, row) {
                return '<tr class="child-title"><td colspan="8" style="padding:10px 15px; background:#fff; border:0;font-size: 12px; color:#6b6c6e;"><img src="' + row.IconSrc + '" title="' + (row.PlatformText == 'Android' ? 'App' : row.PlatformText) + '订单' + '" width="16" style="margin:0 8px 0 0;position:relative;top:-1px;" /> 订单号:' + row.OrderId + (row.IsVirtual == true ? '&nbsp;<span>(虚拟)</span>' : '') + ' &nbsp;&nbsp;&nbsp;&nbsp;  ' + row.OrderDate + '<span class="pull-right">' + (row.PaymentTypeStr || '') + '</span></td></tr>';
            },
            rowFootFormatter: function (target, index, row) {
                var html = [];
                var Remaval = row.SellerRemarkFlag;
                if (row.SellerRemark) {
                    html.push('<tr class="child-title"><td colspan="8" style="background:#fff; padding:5px 10px 5px 45px; word-break:break-all;"><div class="form-group mb0">');
                    if (Remaval == 1) {
                        html.push("<span class='iconfont f01' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
                    } else if (Remaval == 2) {
                        html.push("<span class='iconfont f02' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
                    } else if (Remaval == 3) {
                        html.push("<span class='iconfont f03' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
                    } else if (Remaval == 4) {
                        html.push("<span class='iconfont f04' style='padding-right: 2px;'>&#xe630;</span> <em class='flag-states'></em>");
                    }
                    html.push('备注:' + row.SellerRemark);
                    html.push('</div></td></tr>');
                }
                html.push('<tr class="child-title"><td colspan="8" style="background:#f0f0f0; border-top:1px solid #e7e7e7; padding:4px 0 3px;border-bottom:0;"></td></tr>');
                return html.join('');
            },
            operationButtons: "#orderOperate",
            onLoadSuccess: CheckStatus,
            columns:
            [[
                {
                    checkbox: true, width: 35, formatter: function (value, row, index) {
                        var html = "<input type=\"checkbox\" data-isbranch=\"" + (row.ShopBranchId > 0) + "\"";
                        if (row.CanSendGood == false || row.OrderStatus == "待付款" || row.OrderStatus == "已关闭") {
                            html += ' disabled ';
                        }
                        html += ">";
                        return html;
                    }
                },
                {
                    field: "ProductName", title: '商品', width: 350,
                    formatter: function (value, row, index) {
                        var html = [];
                        for (var i = 0; i < row.OrderItems.length; i++) {
                            var showUnit = row.OrderItems[i].Unit || "";
                            html.push('<div class="img-list">' +
                                '<img src="' + row.OrderItems[i].ThumbnailsUrl + '">' +
                                '<span class="overflow-ellipsis" style="width:220px"><a title="' + row.OrderItems[i].ProductName + '" href="/product/detail/' + row.OrderItems[i].ProductId + '" target="_blank" >' + row.OrderItems[i].ProductName + '</a>' +
                                '<p>￥' + row.OrderItems[i].SalePrice.toFixed(2) + ' &nbsp; ' + row.OrderItems[i].Quantity + showUnit + '</p>' +
                                '</span></div>');
                        }
                        return html.join('');
                    }
                },
                {
                    field: "TotalPrice", title: "订单总额", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        var html = "<span class='ftx-04'>" + '￥' + value.toFixed(2) + "</span>";
                        return html;
                    }
                },
                {
                    field: "RefundStatusText", title: "售后", width: 120, align: "center",
                    formatter: function (value, row, index) {
                        if (value != null) {
                            return '<a href=\"javascript:;\" onclick=\"GetRrefundType(' + row.OrderId + ',' + row.ShopId + ')\">' + value + '</a>';
                        }
                    }
                },
                {
                    field: "UserName", title: "买家", width: 70, align: "center", formatter: function (value, row, index) {
                        return row.UserName + '<br/>' + row.CellPhone;
                    }
                },
                isOpenStore,
                {
                    field: "OrderStatus", title: "订单状态", width: 70, align: "center",
                    formatter: function (value, row, index) {
                        var html = ["<span class='ordstbox'>"];
                        html.push(row.OrderStatus);
                        html.push("</span>");
                        return html.join("");
                    }
                },
                {
                    field: "operation", operation: true, title: "操作", width: 120,
                    formatter: function (value, row, index) {
                        var id = row.OrderId.toString();
                        var html = ["<span class=\"btn-a\">"];
                        html.push("<a href='./Detail/" + id + "' target=\"_blank\">查看</a>");
                        if (row.OrderStatus == "待付款" && row.OrderType != 3) {
                            html.push("<a href='./Detail/" + id + "?&updatePrice=" + true + "' target=\"_blank\">改价</a>");
                            if (row.OrderType != 3) {
                                html.push("<a class=\"good-check\" onclick=\"OpenCloseOrder('" + id + "')\">取消</a>");
                            }
                        }
                        if (row.OrderStatus == "待发货" && row.ShopBranchId <= 0) {
                            if (row.CanSendGood) {
                                html.push("<a href='./SendGood?ids=" + id + "' target=\"_blank\">发货</a>");
                            }
                            if (row.PaymentType == 3 && row.OrderType != 3) {
                                html.push("<a class=\"good-check\" onclick=\"OpenCloseOrder('" + id + "')\">取消</a>");
                            }
                        }
                        if (row.OrderStatus == "待收货") {
                            //if (row.PaymentType == 3 && row.OrderType != 3) {
                            //    html.push("<a class=\"good-check\" onclick=\"OpenCloseOrder('" + id + "')\">取消</a>");
                            //    if (row.ShipOrderNumber && row.ShipOrderNumber.length > 0)
                            //        html.push("<a href='./UpdateExpress?id=" + id + "'>修改运单号</a>");
                            //} else
                            if (row.ShipOrderNumber && row.ShipOrderNumber.length > 0) {
                                html.push("<a href='./UpdateExpress?id=" + id + "'>修改运单号</a>");
                            }
                        }
                        var strSellerRemark = (row.SellerRemark == null) ? "" : (row.SellerRemark+"").replace("'", "").replace(/[\r\n]/g, "");//过滤回车和单引号
                        html.push("<a class=\"good-check\" href='javascript:void(0)' onclick=\"SellerRemark('" + id + "','" + strSellerRemark + "','" + row.SellerRemarkFlag + "')\">备注</a>");
                        if ($("#isOpenStore").val() == "True" && row.IsVirtual == false) {
                            if ((row.OrderStatus == "待付款" || row.OrderStatus == "待发货") && row.DeliveryType != 1) { //到店自提不允许更改和分配门店
                                var text = row.ShopBranchId > 0 ? "更改门店" : "分配门店";
                                html.push("<a class=\"good-check\" onclick=\"ProcessStore('" + id + "','" + row.RegionId + "','" + row.RegionFullName + "','" + row.Address + "','" + row.ShopId + "','" + row.RefundStatusText + "','" + row.LatAndLng + "','" + row.DeliveryType + "')\">" + text + "</a>");
                            }
                        }
                        if (row.OrderType == 3) {
                            if (row.FightGroupJoinStatus == 0 || row.FightGroupJoinStatus == 1) {
                                html.push("<br/><span>组团中，不可发货</span>");
                            }
                            else if (row.FightGroupJoinStatus == 2) {
                                html.push("<br/><span>拼团失败</span>");
                            }
                        }
                        html.push("</span>");
                        return html.join("");
                    }
                }
            ]]
        });
    }


    function CheckStatus() {
        var status = $(".nav li[class='active']").attr("value");
        if (status && status != '0' && status != '2') {
            $(".tabel-operate a").last().siblings().remove(); //只留下导出按钮
            $(".td-choose").hide();
        }
        else {
            $(".td-choose").show();
        }
    }

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        if (isNaN(orderId)) {
            $.dialog.errorTips("请输入正确的查询订单号"); return false;
        }
        var userName = $.trim($('#txtUserName').val());
        var orderType = $("#orderType").val();
        var invoiceType = $('#invoiceType').val();
        var paymentTypeGateway = $.trim($('#selectPaymentTypeName').val());
        var selectPaymentType = $.trim($("#selectPaymentType").val());
        var shopBranchName = $('#txtShopBranchName').val();
        var txtUserContact = $.trim($('#txtUserContact').val());
        var allotStore = $("#allotStore").val();
        var shopBranchId = $("#shopBranchId").val();
        var fgvirtual = $('.fg-virtual input:checkbox').get(0);
        var isVirtual = fgvirtual.checked;
        if (!isVirtual) {
            isVirtual = null;
        }
        $(".tabel-operate").find("label").remove();
        $("#list").MallDatagrid('reload', {
            startDate: startDate, endDate: endDate, orderId: orderId, userName: userName,
            OrderType: orderType, paymentTypeGateway: paymentTypeGateway, paymentType: selectPaymentType, shopBranchName: shopBranchName,
            userContact: txtUserContact, AllotStore: allotStore, ShopBranchId: shopBranchId, IsVirtual: isVirtual, InvoiceType: invoiceType
        });
    });


    $('.nav-tabs-custom li').click(function (e) {
        try {
            searchClose(e);
            $(this).addClass('active').siblings().removeClass('active');
            if ($(this).attr('type') == 'statusTab') {//状态分类
                $('#txtOrderId').val('');
                $('#txtuserName').val('');
                $(".search-box form")[0].reset();
                ///无效会重新加载
                //if ($(this).attr('value') == 0 || $(this).attr('value') == 2) {
                //    var html = '<a href="javascript:downloadOrderList()" class="btn btn-default btn-ssm">订单配货表</a><a href="myPreview()" class="btn btn-default btn-ssm">打印发货单</a><a href="printOrders()" class="btn btn-default btn-ssm">打印快递单</a><a href="sendGood()" class="btn btn-default btn-ssm">批量发货</a>';
                //    $(".tabel-operate").html('');
                //    $(".tabel-operate").append(html);
                //}
                //else {
                //    //  $(".tabel-operate").children().remove();
                //   // $(".tabel-operate a").last().siblings().remove(); //只留下导出按钮
                //}
                $("#list").MallDatagrid('clearReload', { status: $(this).attr('value') || null});
            }
        }
        catch (ex) {
            alert();
        }
    });

    $("#aDownloadProductList").attr("href", "./DownloadProductList?ids=" + getSelectedIds())
    $("#bt_orderprint").click(function () {
        $.post('/SellerAdmin/Order/GoExpressBills', null, function (result) {
            if (result.success) {
                window.open(result.msg);
            } else {
                $.dialog.errorTips(result.msg);
            }
        });
    });

    $("#remarkContent").keyup(function () {
        var remark = $("#remarkContent").val();
        if (remark.trim() == "" || remark.length > 200) {
            $("#remarkContentTip").show();
            $("#remarkContentTip").text("回复内容在200个字符以内！");
            $("#remarkContent").css({ border: '1px solid #f60' });
            return false;
        }
        else {
            $("#remarkContentTip").hide();
            $("#remarkContent").css({ border: '1px solid #ccc' });
        }
    });
    $("#ddlStores").change(function () {
        GetShopBranchStock();
    });
    $("#verificationBtn").click(function () {
        $.dialog({
            title: '订单核销',
            lock: true,
            id: 'OrderVerification',
            content: document.getElementById("verification-form"),
            okVal: '验证核销码',
            width: 300,
            height:100,
            init: function () {
            },
            ok: function () {
                var verificationCode = $.trim($("#txtVerificationCode").val());
                if (verificationCode == '') {
                    $.dialog.tips("请输入核销码");
                    return false;
                }
                $.ajax({
                    type: "Post",
                    url: '/selleradmin/order/verifyverification',
                    data: { verificationCode: verificationCode, shopId: $("#shopId").val() },
                    async: false,
                    dataType: "json",
                    success: function (data) {
                        if (data.success == false) {
                            switch (data.code) {
                                case 1:
                                    $.dialog.tips("该核销码无效");
                                    break;
                                case 2:
                                    $.dialog.tips("该核销码于" + data.msg + "已核销");
                                    break;
                                case 3:
                                    $.dialog.tips("非本店核销码，请买家核对信息");
                                    break;
                                case 4:
                                    $.dialog.tips("此核销码已过期，无法核销");
                                    break;
                                case 5:
                                    $.dialog.tips("此核销码正处于退款中，无法核销");
                                    break;
                                case 6:
                                    $.dialog.tips("此核销码已经退款成功，无法核销");
                                    break;
                                case 7:
                                    $.dialog.tips("该核销码暂时不能核销，请留意生效时间！");
                                    break;
                            }
                        } else {
                            commitVerification(data.data, verificationCode); //验证成功则进入核销详情页
                        }
              
                    },
                    error: function () {
                        $.dialog.tips("系统繁忙，请刷新重试");
                    }
                });
                return true;
            }
        });
    });
});

function InitAreaSelect(areaId, latAndLng) {
    $("#StoreAddressId").val(areaId);
    $("#area-selector-store").RegionSelector({
        selectClass: "form-control input-sm",
        valueHidden: "#StoreAddressId"
    });
    $("#area-selector-store select").change(function () {
        GetArealShopBranchs(latAndLng);
    });
}

function downloadProductList() {
    var ids = getSelectedIds();
    if (ids.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }

    loadIframeURL("./DownloadProductList?ids=" + ids.toString());
}

function hasSelectBranchOrder() {
    return $("#list input[type='checkbox'][data-isbranch='true']:checked").length > 0;
}

function downloadOrderList() {
    var ids = getSelectedIds();
    if (ids.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }
    //判断是否有分店订单
    if (hasSelectBranchOrder()) {
        $.dialog.tips('不可以操作门店订单');
        return false;
    }

    $.dialog({
        title: '下载配货批次表',
        lock: true,
        fixed: true,
        id: 'dialogPrepareGoods',
        width: '630px',
        content: document.getElementById("prepareGoods"),
        padding: '0 40px'
    });

    //loadIframeURL("/SellerAdmin/Order/DownloadOrderList?ids=" + ids.toString());
}
function exportOrderList() {
    var ids = getSelectedIds();
    if (ids.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }
    window.open("/SellerAdmin/Order/DownloadOrderList?ids=" + ids.toString());
}
function exportOrderProductList() {
    var ids = getSelectedIds();
    if (ids.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }
    window.open("/SellerAdmin/Order/DownloadOrderProductList?ids=" + ids.toString());
}

function loadIframeURL(url) {
    var iFrame;
    iFrame = document.createElement("iframe");
    iFrame.setAttribute("src", url);
    iFrame.setAttribute("style", "display:none;");
    iFrame.setAttribute("height", "0px");
    iFrame.setAttribute("width", "0px");
    iFrame.setAttribute("frameborder", "0");
    document.body.appendChild(iFrame);
    // 发起请求后这个iFrame就没用了，所以把它从dom上移除掉
    //iFrame.parentNode.removeChild(iFrame);
    iFrame = null;
}

function myPreview() {
    var orderIds = getSelectedIds();
    if (orderIds.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }
    //判断是否有分店订单
    if (hasSelectBranchOrder()) {
        $.dialog.tips('不可以操作门店订单');
        return false;
    }

    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    var strBodyStyle = "<style>body{margin:0; padding:0;font-family: 'microsoft yahei',Helvetica;font-size:12px;color: #333;}.table-hd{ margin:0;line-height:30px; float:left; background: #f5f5f5;padding:0 10px;  margin-top:30px;}.table-hd strong{font-size:14px;font-weight:normal; float:left}.table-hd span{ font-weight:normal; font-size:12px;float:right}table{border: 1px solid #ddd;width:100%;border-collapse: collapse;border-spacing: 0; font-size:12px; float:left}table th,table td{border:1px solid #ddd;padding: 8px; text-align:center}table th{border-top:0;}</style>";
    $.post('./GetOrderPrint', { ids: orderIds.toString() }, function (result) {
        if (result.success) {
            var strFormHtml = strBodyStyle + "<body>" + result.msg + "</body>"; //这里的”divdiv1“是标签的名称。
            //LODOP.PRINT_INIT("高青公路养护");
            LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //A4纸张纵向打印
            //LODOP.SET_PRINT_STYLE("FontSize", 9);
            LODOP.ADD_PRINT_HTM("0%", "0%", "100%", "100%", strFormHtml);//四个数值分别表示Top,Left,Width,Height
            LODOP.PREVIEW(); //打印预览
            //LODOP.PRINT(); //直接打印
        }
    });
}

function sendGood() {
    var orderIds = getSelectedIds();
    if (orderIds.length <= 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    }
    //判断是否有分店订单
    if (hasSelectBranchOrder()) {
        $.dialog.tips('不可以操作门店订单');
        return false;
    }

    location.href = "./SendGood?ids=" + orderIds.toString();
}

function OpenCloseOrder(orderId) {
    $.dialog({
        title: '取消订单',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<p style="padding:0">确认要取消订单吗？取消后订单将会是关闭状态。</p>',
            '</div>',
            '</div>'].join(''),
        padding: '20px 60px',
        button: [
            {
                name: '确认取消',
                callback: function () {
                    CloseOrder(orderId);
                },
                focus: true
            }]
    });
}

function CloseOrder(orderId) {
    var loading = showLoading();
    $.post('./CloseOrder', { orderId: orderId }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").MallDatagrid('options').pageNumber;
            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
        }
        else
            $.dialog.errorTips("操作失败");
    });
}

function getSelectedIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.OrderId);
    });
    return ids;
}

function printOrders() {
    var ids = getSelectedIds();
    if (ids.length == 0) {
        $.dialog.tips('请至少选择一个订单');
        return false;
    } else {
        //判断是否有分店订单
        if (hasSelectBranchOrder()) {
            $.dialog.tips('不可以操作门店订单');
            return false;
        }
        location.href = "print?orderIds=" + ids.toString();
    }
}

function ExportExecl() {
    var status = $('.nav-tabs-custom li[class="active"][type=statusTab]').attr("value");
    if (status == 0)
        status = null;
    var startDate = $("#inputStartDate").val();
    var endDate = $("#inputEndDate").val();
    var orderId = $.trim($('#txtOrderId').val());
    var userName = $.trim($('#txtUserName').val());
    var orderType = $("#orderType").val();
    var invoiceType = $('#invoiceType').val();
    var selectPaymentType = $.trim($("#selectPaymentType").val());
    var txtUserContact = $.trim($('#txtUserContact').val());
    var shopBranchId = $.trim($('#shopBranchId').val());
    if (shopBranchId == '全部')
        shopBranchId = '';
    var fgvirtual = $('.fg-virtual input:checkbox').get(0);
    var isVirtual = fgvirtual.checked;
    if (!isVirtual)
        isVirtual = null;//页面没有单独搜索实物订单，当不是虚拟搜索时，默认null

    var href = "/SellerAdmin/Order/ExportToExcel?status={0}&startDate={1}&endDate={2}&orderId={3}&userName={4}&orderType={5}&paymentType={6}&userContact={7}&ShopBranchId={8}&IsVirtual={9}&invoiceType={10}"
        .format(status, startDate, endDate, orderId, userName, orderType, selectPaymentType, txtUserContact, shopBranchId, isVirtual, invoiceType);

    $("#aExport").attr("href", href);
}

$(function () {
    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    //$(".start_datetime").click(function () {
    //    $('.end_datetime').datetimepicker('show');
    //});
    //$(".end_datetime").click(function () {
    //    $('.start_datetime').datetimepicker('show');
    //});
    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });


});
$(function () {
    var val = $("#RQS").val();
    $('.nav-tabs-custom li').each(function () {
        if ($(this).val() == val) {
            $(this).addClass('active').siblings().removeClass('active');
        }
    });
});
// 商家手动处理门店
var _shopId = 0, _orderId = 0;
function ProcessStore(orderId, regionId, regionFullName, address, shopId, refundStatusText, latAndLng, deliveryType) {
    $("#storeHelper").html('');
    if (refundStatusText != "null" && refundStatusText.length > 0) {
        $.dialog.tips("订单正在申请售后，请先处理售后申请");
        return;
    }
    _shopId = shopId;//商家ID
    _orderId = orderId;//订单ID
    $("#shippingAddresses").html(regionFullName + address);
    InitAreaSelect(Number(regionId), latAndLng);
    StoreOperate();
    GetArealShopBranchs(latAndLng, deliveryType);//默认加载与订单收货地址同区域门店
}
function StoreOperate() {
    $.dialog({
        title: '订单分配到门店',
        lock: true,
        id: 'StoreOperate',
        content: document.getElementById("storeSelect-form"),
        //padding: '0 5px',
        okVal: '保存',
        init: function () {
        },
        ok: function () {
            //开始分配门店
            var queryData = {
                orderId: _orderId, shopId: _shopId, shopBranchId: $("#ddlStores").val(), url: "AllotStore"
            }
            if (queryData.shopBranchId == -1) {
                $.dialog.tips("请先选择门店");
                return false;
            }
            $.ajax({
                type: "Post",
                url: queryData.url,
                data: queryData,
                async: false,
                dataType: "json",
                success: function (data) {
                    if (data.Success == false) {
                        $.dialog.tips(data.Message);
                    } else {
                        $.dialog.succeedTips(data.Message, function () {
                            //点击保存按钮后，关闭弹框。清空选择框给类状态。刷新页面
                            var pageNo = $("#list").MallDatagrid('options').pageNumber;
                            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
                        });
                    }
                },
                error: function () {
                    $.dialog.tips("系统繁忙，请刷新重试");
                }
            });
        }
    });
}
function GetArealShopBranchs(latAndLng, deliveryType) {
    var queryData = {
        areaId: Number($("#StoreAddressId").val()), latAndLng: encodeURIComponent(latAndLng), shopId: _shopId, url: "GetArealShopBranch"
    }
    if (queryData.areaId <= 0) {
        $.dialog.tips("请先选择区域");
        return;
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.Success == false) {
                $.dialog.tips(data.Message);
            } else {
                var databox = $("#ddlStores");
                databox.empty();
                if (deliveryType == 1) {
                    databox.append("<option value='-1'>请选择门店</option>");//到店自提的订单分配门店时不出现总店
                } else {
                    databox.append("<option value='-1'>请选择门店</option><option value='0'>" + $("#shopName").val() + "</option>");
                }
                if (data) {
                    if (data.Models && data.Models.length > 0) {
                        $.each(data.Models, function (i, model) {
                            databox.append("<option value=\"" + model.Id + "\">" + model.ShopBranchName + "</option>");
                        });
                    }
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function GetShopBranchStock() {
    $("#storeHelper").html('');
    var queryData = {
        orderId: _orderId, shopId: _shopId, shopBranchId: $("#ddlStores").val(), url: "GetShopBranchStock"
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.Success == false) {
                $("#storeHelper").html(data.Message);
                $(".aui_state_highlight").attr("disabled", "disabled"); //禁用保存按钮
            } else {
                $(".aui_state_highlight").removeAttr("disabled");
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function GetRrefundType(orderId, shopId) {
    var queryData = {
        orderId: orderId, shopId: shopId, url: "GetRrefundType"
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (data) {
            if (data.Success == false) {
                $.dialog.tips(data.Message);
            } else {
                location.href = "/SellerAdmin/orderrefund/management?showtype=" + data.showtype + "&orderid=" + orderId;
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function commitVerification(orderId, verficationCode) {
    $.dialog({
        title: '核销详情',
        lock: true,
        id: 'CommitVerification',
        content: document.getElementById("verification-detail-form"),
        okVal: '确认核销',
        init: function () {
            getVerficationDetail(orderId, verficationCode);
        },
        ok: function () {
            var arr = new Array();
            $.each($('input[name=j_VerificationCode]:checkbox:checked'), function (i) {
                arr[i] = $(this).val();
            });
            var vals = arr.join(",");
            $.ajax({
                type: "Post",
                url: '/selleradmin/order/commitverification',
                data: { verficationCodes: vals, shopId: $("#shopId").val(), orderId: orderId },
                async: false,
                dataType: "json",
                success: function (data) {
                    if (data.success) {
                        $.dialog.succeedTips("核销成功", function () {
                            window.location.reload();
                        });
                    } else {
                        $.dialog.errorTips(data.msg);
                    }
                }
            });
            return false;
        }
    });
}
function getVerficationDetail(orderId,verficationCode) {
    var html = '';
    $.ajax({
        type: "Post",
        url: '/selleradmin/order/getverficationdetail',
        data: { orderId: orderId, shopId: $("#shopId").val() },
        async: false,
        dataType: "json",
        success: function (result) {
            if (result.success == false) {
                $.dialog.tips(result.msg);
                return false;
            } else {
                var data = result.data;
                if (data.VerificationCodes != null) {
                    html = '<li class="form-control-static">' + verficationCode.replace(/(.{4})/g, "$1 ") + '<input type="checkbox" class="mt5" name="j_VerificationCode" value="' + verficationCode + '"  checked="checked" disabled="disabled"/>' + '</li>';
                    var arr = data.VerificationCodes.filter(function(item){  
                        return item.SourceCode != verficationCode;
                    });
                    $.each(arr, function (i) {
                        html += '<li class="form-control-static">' + arr[i].VerificationCode + '<input type="checkbox" class="mt5" value="' + arr[i].SourceCode + '" name="j_VerificationCode"/>' + '</li>';
                    });
                    $(".j_verificationCodes").html(html);
                }
                if (data.VirtualOrderItemInfos != null) {
                    html = '';
                    $.each(data.VirtualOrderItemInfos, function (i) {
                        html += '<label class="col-sm-4 form-control-static">' + data.VirtualOrderItemInfos[i].VirtualProductItemName + ':</label>';
                        if (data.VirtualOrderItemInfos[i].VirtualProductItemType == 6) {
                            var imgContent = data.VirtualOrderItemInfos[i].Content.split(',');
                            if (imgContent.length > 0) {
                                html += '<div class="col-sm-8 form-control-static"><div class="after-service-img">';
                                for (var i = 0; i < imgContent.length; i++) {
                                    html += '<a><img src="'+imgContent[i]+'"/></a>';
                                }
                                html += '</div></div>';
                            }
                        } else {
                            html += '<div class="col-sm-8 form-control-static"><span id="content" style="max-height:37px;overflow:hidden;">' + data.VirtualOrderItemInfos[i].Content + '</span></div>';
                        }
                    });
                    $(".j_memberInfo").html(html);
                }
                if (data.ProductInfo != null) {
                    $(".j_productPic").attr("src", data.ProductInfo.ImagePath);
                    $(".j_productName").html(data.ProductInfo.ProductName);
                    $(".j_price").html('￥'+data.OrderItemInfo.SalePrice);
                    $(".j_quantity").html('×' + data.OrderItemInfo.Quantity);
                    $(".j_specifications").html(data.OrderItemInfo.Color + ' ' + data.OrderItemInfo.Size + ' ' + data.OrderItemInfo.Version);
                }
                previewImg();
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function previewImg() {
    $(document).on("click", ".after-service-img img", function () {
        $(".preview-img").show();
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(".cover").show();
    });
    $(".preview-img").click(function () {
        $(this).hide()
        $(".cover").hide();
    });
    $(".cover").click(function () {
        $(".preview-img").hide();
        $(".cover").hide();
    })
}