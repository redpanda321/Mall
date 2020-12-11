var showtype;
$(function () {
    $("input[type='text']").on("postpaste", function () {
        $(this).val($.trim($(this).val()));
    }).pasteEvents();
    var status = GetQueryString("status");
    showtype = $("#SType").val();
    var OrderId = GetQueryString('orderid');
    var li = $("li[value='" + status + "']");
    if (li.length > 0) {
        typeChoose('5')
    } else {
        typeChoose('')
    }

    //订单表格

    function typeChoose(val) {
        $('.nav-tabs-custom li').each(function () {
            if ($(this).val() == val) {
                $(this).addClass('active').siblings().removeClass('active');
            }
        });
        //组合显示字段
        try {
            showtype = parseInt(showtype, 10);
        } catch (ex) {
            showtype = 0;
        }
        datacols = [[]];
        //var opButton = '';
        var haveCheck = false;
        if (val == '5') {//待处理状态 显示选择、批处理按钮
            //opButton = '#btnBatchOperate';
            haveCheck = true;
        }
        datacols[0] = datacols[0].concat([
        		{ checkbox: true, width: 39 },
                {
                    field: "OrderId", title: '订单号', width: 100,
                    formatter: function (value, row, index) {
                        if (row.IsVirtual) {
                            return '<a href="/Admin/order/Detail/' + value + '" target="_blank">' + value + '</a><span>(虚拟)</span>';
                        } else {
                            return '<a href="/Admin/order/Detail/' + value + '" target="_blank">' + value + '</a>';
                        }
                    }
                },
                    { field: "ShopName", title: "店铺", width: 120, align: "center" },
                    {
                        field: "ProductName", title: "商品", width: 280, align: "center",
                        formatter: function (value, row, index) {
                            var html = ""
                            if (row.RefundMode == 1) {
                                html = "订单所有商品";
                            } else {
                                html = '<img style="margin-left:15px;" width="40" height="40" src="' + row.ThumbnailsUrl + '"/>' + '<span class="overflow-ellipsis" style="width:180px;text-align:left"><a title="' + value + '" href="/product/detail/' + row.ProductId + '" target="_blank">' + value + '</a></span>';
                            }
                            return html;
                        }
                    },

                    { field: "UserName", title: "买家", width: 80, align: "center" },
                    { field: "ApplyDate", title: "申请日期", width: 70, align: "center" },
                    {
                        field: "Amount", title: "退款", width: 90, align: "center",
                        formatter: function (value, row, index) {
                            var html = "<span class='ftx-04'>" + '￥' + value + "</span>";
                            return html;
                        }
                    }]);
        switch (showtype) {
            case 0:
            case 3:
                datacols[0].push({ field: "ReturnQuantity", title: "退货", width: 50, align: "center" });
                break;
        }

        datacols[0] = datacols[0].concat([
            {
                field: "RefundStatus", title: "处理状态", width: 100, align: "center",
                formatter: function (value, row, index) {
                    if (row.IsVirtual) {
                        if (row.RefundPayStatus != 1 && row.RefundBatchNo != "") {
                            return "<span class='ftx-04'>异常</span>";
                        } else {
                            return value;
                        }
                    } else {
                        return value;
                    }
                }
            },
            {
                field: "operation", operation: true, title: "操作", width: 100,
                formatter: function (value, row, index) {
                    var html = ["<span class=\"btn-a\">"];
                    // html.push("<input type=\"hidden\" name=\"rowdata\" id=\"rowdata-" + row.RefundId + "\" value='" + jQuery.toJSON(row) + "'>");
                    if (row.AuditStatus == "商家通过审核" || row.AuditStatus == "待平台确认" || row.AuditStatus == "退款中") {
                        html.push("<a class=\"good-check\" onclick=\"CheckRefund('" + index + "')\">确认退款</a>");
                    } else {
                        html.push("<a class=\"good-check\" onclick=\"OpenRefundReason('" + index + "')\">查看原因</a>");
                    }
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]);

        $("#list").MallDatagrid({
            url: './list?showtype=' + showtype,
            nowrap: true,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "RefundId",
            pageSize: 15,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { auditStatus: val, orderId: OrderId },
            columns: datacols,
            operationButtons: "#batchOperate",
            hasCheckbox: haveCheck,
            onLoadSuccess: function () {
                if (val == '5') {//待处理状态 显示选择、批处理按钮
                    $('#btnBatchOperate').show();
                    //$('.tabel-operate').show();
                    $('.tabel-operate .checkbox').show();
                    $('#list .td-choose').show();
                }
                else {
                    $('#btnBatchOperate').hide();
                    //$('.tabel-operate').hide();
                    $('.tabel-operate .checkbox').hide();
                    $('#list .td-choose').hide();
                }
            }
        });


    }

    $('#searchButton').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var orderId = $.trim($('#txtOrderId').val());
        var shopName = $.trim($('#txtShopName').val());
        var productName = $.trim($('#txtProductName').val());
        var userName = $.trim($('#txtUserName').val());
        $("#list").MallDatagrid('reload', { startDate: startDate, endDate: endDate, orderId: orderId, shopName: shopName, productName: productName, userName: userName });
    })


    $('.nav-tabs-custom li').click(function (e) {
        searchClose(e);
        $(this).addClass('active').siblings().removeClass('active');
        if ($(this).attr('type') == 'statusTab') {//状态分类
            //$('#txtOrderId').val('');
            //$('#txtShopName').val('');
            //$('#txtUserName').val('');
            //$("#txtProducdName").val('');
            $(".search-box form")[0].reset();
            //$("#list").MallDatagrid('clearReload', { auditStatus: $(this).attr('value') || null });
            typeChoose($(this).attr('value') || null);
        }
    });
});



function CheckRefund(rowIndex) {
    var data = $("#list").MallDatagrid('getRowByIndex', rowIndex);
    var loading = showLoading();
    $.post('./CheckRefund', { refundId: data.RefundId }, function (result) {
        loading.close();
        if (!result.success) {
            $.dialog.confirm('商家当前帐户余额不足以支付该笔退款金额，退款后，商家帐户余额将为负数，是否确认执行退款操作？', function () {
                OpenConfirmRefund(rowIndex);
            });
        }
        else {
            OpenConfirmRefund(rowIndex);
        }
    });
}

function OpenConfirmRefund(rowIndex) {
    // var dobj = $("#rowdata-" + refundId);
    // var data = jQuery.parseJSON(dobj.val());
    var data = $("#list").MallDatagrid('getRowByIndex', rowIndex);
    $.dialog({
        title: '确认退款',
        width: 466,
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<label class="label-inline fl">售后编号</label>',
                '<p class="only-text">' + data.RefundId + '</p>',
            '</div>',
            '<div class="form-group">',
                '<label class="label-inline fl">理由</label>',
                '<p class="help-top">' + data.Reason.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
            '</div>',
             '<div class="form-group">',
                '<label class="label-inline fl">说明</label>',
                '<p class="help-top">' + data.ReasonDetail + '</p>',
            '</div>',

             '<div class="form-group">',
                '<label class="label-inline fl">联系人</label>',
                '<p class="help-top">' + data.ContactPerson.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
           ' </div>',
            '<div class="form-group">',
                '<label class="label-inline fl">联系方式</label>',
                '<p class="help-top">' + data.ContactCellPhone + '</p>',
           ' </div>',
            '<div class="form-group">',
                '<label class="label-inline fl">退款金额</label>',
                '<p class="only-text"><span class="cor-red">' + data.Amount + '</span></p>',
           ' </div>',
            '<div class="form-group">',
                '<label class="label-inline fl">退款方式</label>',
                '<p class="help-top">' + data.RefundPayType + '</p>',
           ' </div>',
           '<div class="form-group">',
                    '<label class="label-inline fl">售后凭证</label>',
                    '<p class="after-service-img">',
                    data.CertPic1 ? '<img src="' + data.CertPic1 + '">' : '',
                    data.CertPic2 ? '<img src="' + data.CertPic2 + '">' : '',
                    data.CertPic3 ? '<img src="' + data.CertPic3 + '">' : '',
                   '</p>',
               '</div>',
           '<div class="form-group" style="position:relative">',
                '<label class="label-inline fl">退款备注</label>',
                '<p class="only-text"><input class="form-control" type="text" name="txtRefundRemark" id="txtRefundRemark" value=\"' + data.ManagerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '\"/></p>\
                <span class="field-validation-error" id="orderRefundCotentTip"></span> ',
            '</div>',
        '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txtRefundRemark").focus(); },
        button: [
        {
            name: data.RefundPayStatus == 0 ? '完成支付' : '确认退款',
            callback: function () {
                var replycontent = $("#txtRefundRemark").val();
                if (replycontent.length > 200) {
                    $("#orderRefundCotentTip").text("回复内容在200个字符以内");
                    $("#txtRefundRemark").css({ border: '1px solid #f60' });
                    return false;
                }
                ConfirmRefund(data.RefundId, $('#txtRefundRemark').val());
            },
            focus: true
        }]
    });
}

function OpenRefundReason(rowIndex) {
    // var dobj = $("#rowdata-" + refundId);
    //   var data = jQuery.parseJSON(dobj.val());
    var data = $("#list").MallDatagrid('getRowByIndex', rowIndex);
    var codes = data.VerificationCodeIds.split(','),codestr='';
    for(var i=0;i<codes.length;i++){
    	if(i%2==1){
    		codestr = codestr+codes[i];
    		
    	}else{
    		codestr = (i==codes.length-1)?codestr+codes[i]+'&nbsp;&nbsp;&nbsp;':codestr+codes[i]+',&nbsp;&nbsp;&nbsp;';
    	}
    }
    codestr=(codestr.substring(codestr.length-1)==',')?codestr.substring(0,codestr.length-1):codestr;
    if (data.IsVirtual) {
        dlgcontent = ['<div class="dialog-form">',
          '<div class="form-group">',
              '<label class="label-inline fl">售后编号</label>',
              '<p class="only-text">' + data.RefundId + '</p>',
          '</div>',
          '<div class="form-group">',
              '<label class="label-inline fl">核销码</label>',
              '<p class="only-text">' + codestr + '</p>',
                '<div class="form-group">',
              '<label class="label-inline fl">商品名称</label>',
              '<p class="only-text">' + data.ProductName + '</p>',

          '</div>'];
        dlgcontent = dlgcontent.concat(['<div class="form-group">',
                    '<label class="label-inline fl">退款金额</label>',
                    '<p class="only-text"><span class="cor-red">￥' + data.Amount + '</span>（实付：' + data.SalePrice + '）</p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">理由</label>',
                    '<p class="help-top">' + data.Reason.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">说明</label>',
                    '<p class="help-top">' + data.ReasonDetail.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
               ' </div>',
                 '<div class="form-group">',
                    '<label class="label-inline fl">联系人</label>',
                    '<p class="help-top">' + data.ContactPerson.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '(' + data.ContactCellPhone + ')</p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">期望退款方式</label>',
                    '<p class="help-top">' + data.RefundPayType + '</p>',
               ' </div>']);
        //if (data.CertPic1 || data.CertPic2 || data.CertPic3) {
        //    dlgcontent = dlgcontent.concat([
        //            '<div class="form-group">',
        //                '<label class="label-inline fl">售后凭证</label>',
        //                '<p class="after-service-img">']);
        //    if (data.CertPic1) {
        //        dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic1 + '">']);
        //    }
        //    if (data.CertPic2) {
        //        dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic2 + '">']);
        //    }
        //    if (data.CertPic3) {
        //        dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic3 + '">']);
        //    }
        //    dlgcontent = dlgcontent.concat([
        //                '</p>',
        //            '</div>'
        //    ]);
        //}
        if (data.SellerRemark) {
            dlgcontent = dlgcontent.concat([
                    '<div class="form-group">',
                        '<label class="label-inline fl">商家备注</label>',
                        '<p class="help-top">' + data.SellerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
                   ' </div>']);
        }
        var refundStatus = data.RefundStatus;
        if (data.RefundPayStatus != 1 && data.RefundBatchNo != "") {
            refundStatus = "退款中";
        }

        dlgcontent = dlgcontent.concat([
                   '<div class="form-group">',
                       '<label class="label-inline fl">当前状态</label>',
                       '<p class="only-text"><span class="cor-red">' + refundStatus + '</span></p>',
                  ' </div>']);
        dlgcontent = dlgcontent.concat(['</div>']);
    } else {
        dlgcontent = ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<label class="label-inline fl">售后编号</label>',
                    '<p class="only-text">' + data.RefundId + '</p>',
                '</div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">理由</label>',
                    '<p class="help-top">' + data.Reason.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
                      '<div class="form-group">',
                    '<label class="label-inline fl">说明</label>',
                    '<p class="help-top">' + data.ReasonDetail + '</p>',

                '</div>'];
        dlgcontent = dlgcontent.concat(['<div class="form-group">',
                    '<label class="label-inline fl">联系人</label>',
                    '<p class="help-top">' + data.ContactPerson.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">联系方式</label>',
                    '<p class="help-top">' + data.ContactCellPhone + '</p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">退款金额</label>',
                    '<p class="only-text"><span class="cor-red">' + data.Amount + '</span></p>',
               ' </div>',
                '<div class="form-group">',
                    '<label class="label-inline fl">退款方式</label>',
                    '<p class="help-top">' + data.RefundPayType + '</p>',
               ' </div>']);
        if (data.CertPic1 || data.CertPic2 || data.CertPic3) {
            dlgcontent = dlgcontent.concat([
                    '<div class="form-group">',
                        '<label class="label-inline fl">售后凭证</label>',
                        '<p class="after-service-img">']);
            if (data.CertPic1) {
                dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic1 + '">']);
            }
            if (data.CertPic2) {
                dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic2 + '">']);
            }
            if (data.CertPic3) {
                dlgcontent = dlgcontent.concat(['<img src="' + data.CertPic3 + '">']);
            }
            dlgcontent = dlgcontent.concat([
                        '</p>',
                    '</div>'
            ]);
        }
        if (data.SellerRemark) {
            dlgcontent = dlgcontent.concat([
                    '<div class="form-group">',
                        '<label class="label-inline fl">商家处理</label>',
                        '<p class="help-top">' + data.SellerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
                   ' </div>']);
        }
        if (data.ManagerRemark) {
            dlgcontent = dlgcontent.concat([
                    '<div class="form-group">',
                        '<label class="label-inline fl">平台备注</label>',
                        '<p class="help-top">' + data.ManagerRemark.replace(/>/g, '&gt;').replace(/</g, '&lt;') + '</p>',
                   ' </div>']);
        }
        dlgcontent = dlgcontent.concat(['</div>']);
    }
    $.dialog({
        title: '查看原因',
        lock: true,
        id: 'goodCheck',
        width: '466px',
        content: dlgcontent.join(''),
        padding: '0 40px',
        okVal: '确定',
        ok: function () {
        }
    });
};


/*$(function () {
    $(".after-service-img img").hover(function () {
        $(this).css({"width":"400px","height":"auto","position":"absolute"})
    }, function () {
        $(this).css({ "width": "50px", "height": "auto" })
    });
});*/
function ConfirmRefund(refundId, managerRemark) {
    var loading = showLoading();
    $.post('./ConfirmRefund', { refundId: refundId, managerRemark: managerRemark }, function (result) {
        loading.close();
        if (result.success) {
            if (result.status == 2) {
                jumpurl = result.msg;
                //var urlform = $(BuildGetForm('refundpay_form', jumpurl, '_blank'));
                //urlform.submit();
                //urlform.remove();
                window.open(jumpurl);
                $.dialog({
                    title: '登录平台完成退款',
                    lock: true,
                    content: [
                        '<p>请您在新打开的支付平台页面进行退款，退款完成前请不要关闭该窗口</p>',
                        '<p><a href="' + jumpurl + '" target="_blank">点击跳转支付</a></p>'
                    ].join(''),
                    padding: '20px 60px',
                    button: [
                    {
                        name: '已完成退款',

                        callback: function () {
                            var pageNo = $("#list").MallDatagrid('options').pageNumber;
                            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
                        },
                        focus: true
                    },
                    {
                        name: '支付遇到问题',
                        callback: function () { }
                    }]
                });
            } else {
                $.dialog.succeedTips("操作成功！");
                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                $("#list").MallDatagrid('reload', { pageNumber: pageNo });
            }
        }
        else {
            $.dialog.errorTips("操作失败");
        }
    });
}
function BatchConfirmRefund(ids, managerRemark) {
    var loading = showLoading();
    $.post('./BatchConfirmRefund', { ids: ids, managerRemark: managerRemark }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("批量操作成功！");
            var pageNo = $("#list").MallDatagrid('options').pageNumber;
            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
        }
        else {
            $.dialog.errorTips("批量操作遇到问题：" + result.msg);
        }
    });
}
function BatchDoOperate() {
    var selectedRows = $("#list").MallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].RefundId);
    }
    if (selectedRows.length == 0) {
        $.dialog.errorTips("你没有选择任何选项！");
    }
    else {
        $.dialog({
            title: '批量确认退款',
            width: 466,
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                '<div class="form-group" style="position:relative">',
                '<label class="label-inline fl">退款备注</label>',
                '<p class="only-text"><input class="form-control" type="text" name="txtRefundRemark" id="txtRefundRemark" "/></p>\
                <span class="field-validation-error" id="orderRefundCotentTip"></span> ',
                '</div>',
                '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#txtRefundRemark").focus(); },
            button: [
                {
                    name: '确认退款',
                    callback: function () {
                        var replycontent = $("#txtRefundRemark").val();
                        if (replycontent.length > 200) {
                            $("#orderRefundCotentTip").text("回复内容在200个字符以内");
                            $("#txtRefundRemark").css({ border: '1px solid #f60' });
                            return false;
                        }
                        BatchConfirmRefund(selectids.join(','), $('#txtRefundRemark').val());
                    },
                    focus: true
                }, {
                    name: '取消',
                    callback: function () {
                    }
                }
            ]
        });
    }
}

function BuildGetForm(fm, url, target) {
    var e = null, el = [];
    if (!fm || !url)
        return e;
    target = target || '_blank';
    e = document.getElementById(fm);
    if (!e) {
        e = document.createElement('Form');
        e.Id = fm;
        document.body.appendChild(e);
    }
    alert(url);
    e.method = 'get';
    e.target = target;
    e.style.display = 'none';
    e.action = url;
    return e;
};

function BuildPostForm(fm, url, target) {
    var e = null, el = [];
    if (!fm || !url)
        return e;
    target = target || '_blank';
    e = document.getElementById(fm);
    if (!e) {
        e = document.createElement('Form');
        e.Id = fm;
        document.body.appendChild(e);
    }

    e.method = 'post';
    e.target = target;
    e.style.display = 'none';
    e.enctype = 'application/x-www-form-urlencoded';

    var idx = url.indexOf('?');
    var para = [], op = [];
    if (idx > 0) {
        para = url.substring(idx + 1, url.length).split('&');
        url = url.substr(0, idx);//截取URL
        var keypair = [];
        for (var p = 0 ; p < para.length; p++) {
            idx = para[p].indexOf('=');
            if (idx > 0) {
                el.push('<input type="hidden" name="' + para[p].substr(0, idx) + '" id="frm' + para[p].substr(0, idx) + '" value="' + para[p].substring(idx + 1, para[p].length) + '" />');
            }
        }
    }

    e.innerHTML = el.join('');
    e.action = url;
    return e;
};
$(function () {

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
});

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
$.fn.pasteEvents = function (delay) {
    if (delay == undefined) delay = 10;
    return $(this).each(function () {
        var $el = $(this);
        $el.on("paste", function () {
            $el.trigger("prepaste");
            setTimeout(function () { $el.trigger("postpaste"); }, delay);
        });
    });
};

function ExportExecl() {
    status = GetQueryString('status');
    OrderId = GetQueryString('orderid');
    showtype = $("#SType").val();

    var startDate = $("#inputStartDate").val();
    var endDate = $("#inputEndDate").val();
    var orderId = $.trim($('#txtOrderId').val());
    var productName = $.trim($('#txtProductName').val());
    var userName = $.trim($('#txtUserName').val());
    var shopName = $("#txtShopName").val();

    var href = "/Admin/OrderRefund/ExportToExcel?startDate={0}&endDate={1}&orderId={2}&productName={3}&userName={4}&shopName={5}&auditStatus={6}&ShowRefundType={7}"
        .format(startDate, endDate, orderId, productName, userName, shopName, status, showtype);

    $("#aExport").attr("href", href);
}