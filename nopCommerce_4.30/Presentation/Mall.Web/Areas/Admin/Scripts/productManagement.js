/// <reference path="../../../Scripts/jqeury.MallLinkage.js" />
var categoryId;
$(function () {
    $(".MallDatagrid-cell ").css({ "max-height": "71px", "overflow": "hidden" });
    $('#category1,#category2,#category3').MallLinkage({
        url: '../category/getCategory',
        enableDefaultItem: true,
        defaultItemsText: '全部',
        onChange: function (level, value, text) {
            categoryId = value;
        }
    });
    var status = GetQueryString("status");
    var brandName = GetQueryString("brandName") || '';
    brandName = decodeURI(escape(brandName));
    var li = $("li[value='" + status + "']");
    if (li.length > 0) {
        typeChoose(status, brandName);
    } else {
        typeChoose('', brandName);
    }

    function typeChoose(val, defaultBrandName) {
        $('.nav-tabs-custom li').each(function () {
            var _t = $(this);
            if (_t.val() == val) {
                _t.addClass('active').siblings().removeClass('active');
            }
        });
        defaultBrandName = defaultBrandName || '';//为空时，查询将忽略此条件
        var params = { auditStatus: val, brandName: defaultBrandName };
        if (val == 2) {
            params.saleStatus = 1;
        }
        params.productType = $("#productType").val();

        $("#list").MallDatagrid({
            url: './list',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "id",
            pageSize: 15,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: params,
            //operationButtons: (parseInt(val) == "" ? "#saleOff" : null),
            operationButtons: "#saleOff",
            columns:
            [[
                {
                    checkbox: true, width: 40,
                },
                {
                    field: "DisplaySequence", sort: true, title: '序号', width: 50,
                    formatter: function (value, row, index) {
                        return ' <input class="text-order text-sort" style="width:40px;margin:0;" type="text" value="' + value + '" productid="' + row.id + '" />';
                    }
                },
                {
                    field: "name", title: '商品', width: 250, align: "left",
                    formatter: function (value, row, index) {
                        var html = '<img width="40" height="40" src="' + row.imgUrl + '" style="" /><span class="overflow-ellipsis" style="width:150px"><a title="' + value + '" href="/product/detail/' + row.id + '" target="_blank" href="' + row.url + '">' + value + '</a>';
                        html = html + '<p>￥' + row.price.toFixed(2) + '</p></span>';
                        if (row.ProductType == 1) {
                            html = html + '<p class="lh20">(虚拟)</p>';
                        }
                        return html;
                    }
                },
                { field: "brandName", title: "品牌", width: 55, align: "center" },
                { field: "state", title: "状态", width: 80, align: "center" },
                { field: "categoryName", title: "商城分类", width: 100, align: "center" },
                { field: "shopName", title: "店铺名称", align: "center", width: 100 },
                {
                    field: "productCode", title: '货号', width: 50,
                    formatter: function (value, row, index) {
                        var html = '<span style="max-height:70px;width:70px;overflow:hidden;display:block;">' + value + '</span>';
                        return html;
                    }
                },
                {
                    field: "saleCounts", title: '实际销量', width: 96, sort: true,
                    formatter: function (value, row, index) {
                        var html = '<span style="max-height:70px;overflow:hidden;display:block;">' + value + '</span>';
                        return html;
                    }
                },
                {
                    field: "VirtualSaleCounts", title: '虚拟销量', width: 70,
                    formatter: function (value, row, index) {
                        return ' <input class="text-virtualSaleCount text-order" style="width:40px;margin:0;" type="text" value="' + (value || 0) + '" productid="' + row.id + '" />';
                    }
                },
                { field: "AddedDate", title: '发布时间', width: 80, align: "center", sort: true },
                {
                    field: "s", title: "操作", width: 90, align: "center",
                    formatter: function (value, row, index) {
                        var html = "";
                        html += '<span class="btn-a">';
                        html += '<a class="good-check view-mobile-product" title="预览" data-url="/m/product/detail/' + row.id + '?sv=True" style="font-size:13px;text-decoration: none; cursor:pointer;">预览</a>';
                        if (row.auditStatus == 1 && row.saleStatus == 1)//仅未审核的商品需要审核
                            html += '<a class="good-check" onclick="audit(' + row.id + ')">审核</a>';
                        else if (row.auditStatus == 2)//
                            html += '<a class="good-break" onclick="infractionSaleOffDialog(' + row.id + ')">违规下架</a>';
                        else if (row.auditStatus >= 3) {
                            html += '<a class="good-break" onclick="$.dialog.tips(\'' + (row.auditReason ? row.auditReason.replace(/\'/g, "’").replace(/\"/g, "“") : '无') + '\');">查看原因</a>';
                        }
                        html += '</span>';
                        return html;
                    }
                }
            ]],
            onLoadSuccess: function () {
                OprBtnShow(val);
            }
        });
        orderTextEventBind();
    }

    function orderTextEventBind() {
        var _order = 0; var virtualSaleCount = 0;
        $('.container').on('focus', '.text-sort', function () {
            _order = parseInt($(this).val());
        });
        $('.container').on('focus', '.text-virtualSaleCount', function () {
            virtualSaleCount = parseInt($(this).val());
        });
        $('.container').on('blur', '.text-sort', function () {
            var id = $(this).attr("productid");
            if ($(this).hasClass('text-sort')) {
                if (isNaN($(this).val()) || parseInt($(this).val()) < 0) {
                    $.dialog.errorTips("您输入的数字不合法,此项只能是大于零的整数！");
                    $(this).val(_order);
                } else {
                    if (parseInt($(this).val()) === _order) return;

                    //更新平台商品序号
                    var loading = showLoading();
                    ajaxRequest({
                        type: 'POST',
                        url: "/admin/product/updateDisplaySequence",
                        param: { id: id, order: parseInt($(this).val()) },
                        dataType: "json",
                        success: function (data) {
                            loading.close();
                            if (data.success == true) {
                                $.dialog.tips('更新成功！');
                                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                                $("#list").MallDatagrid('reload', { pageNumber: pageNo });
                            }
                            else {
                                $.dialog.errorTips(data.msg, function () { location.reload(); });
                            }
                        }
                    });
                    //更新平台商品序号
                }
            }
        });
        $('.container').on('blur', '.text-virtualSaleCount', function () {
            var id = $(this).attr("productid");
            if ($(this).hasClass('text-virtualSaleCount')) {
                if ($(this).val().length < 1 || isNaN($(this).val()) || isNaN(parseInt($(this).val())) || parseInt($(this).val()) < 0) {
                    $.dialog.errorTips("您输入的销量不合法,此项只能是大于零的整数！");
                    $(this).val(virtualSaleCount);
                } else {
                    if (parseInt($(this).val()) === virtualSaleCount) return;
                    var ids = [];
                    ids.push(id);
                    //更新平台商品虚拟销量
                    var loading = showLoading();
                    $.ajax({
                        type: "post",
                        url: "btachUpdateSaleCount",
                        data: { ids: ids.join(',').toString(), virtualSaleCounts: parseInt($(this).val()) },
                        dataType: "json",
                        async: false,
                        success: function (result) {
                            loading.close();
                            if (result.success) {
                                $.dialog.tips('设置成功');
                                close = true;
                            }
                            else
                                $.dialog.alert('设置异常!' + result.msg);
                        },
                        error: function () {
                            loading.close();
                            $.dialog.alert('设置出错！');
                        }
                    });
                    //更新平台商品虚拟销量
                }
            }
        });
        $('.container').on('keyup', '.text-virtualSaleCount', function () {
            $(this).val($(this).val().replace(/[^0-9]/g, ''));
        });
    }

    function OprBtnShow(val) {
        $(".check-all").parent().show();
        $(".td-choose").show();
        $("#saleOff").show();
        $("#auditProductBtn").hide();

        if (val != "" && val != null) {
            $(".check-all").parent().hide();
            $(".td-choose").hide();
            $("#saleOff").hide();
        }
        if (val == 2) {
            $(".check-all").parent().show();
            $(".td-choose").show();
            $("#saleOff").show();
        }
        if (val == 1) {
            $(".check-all").parent().show();
            $(".td-choose").show();
            $("#saleOff").show();
            $("#auditProductBtn").show();
        }
        if (val == 4) {
            $("#infractionSaleOffBtn").hide();//违规下架
            $("#updateSaleCount").hide();//批量设置虚拟销量
        }
        if (val == 1) {
            $("#infractionSaleOffBtn").hide();
        }
    }



    //autocomplete
    $('#brandBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("../brand/getBrands", { "keyWords": $('#brandBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            if (item.envalue != null) {
                return item.value + "(" + item.envalue + ")";
            }
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });

    $('#searchButton').click(function () {
        var brandName = $.trim($('#brandBox').val());
        var keyWords = $.trim($('#searchBox').val());
        var productId = $.trim($('#productId').val());
        var shopName = $.trim($('#shopName').val());
        var productType = $("#productType").val();
        var categoryId1 = $("#category1").val();
        var categoryId2 = $("#category2").val();
        var categoryId3 = $("#category3").val();
        if (categoryId1 != "" && categoryId2 != null) {
            categoryId = categoryId1;
        }
        if (categoryId2 != "" && categoryId2!=null) {
            categoryId = categoryId2;
        }
        if (categoryId3 != "" && categoryId3 != null) {
            categoryId = categoryId3;
        }
        $("#list").MallDatagrid('reload', { brandName: brandName, keyWords: keyWords, categoryId: categoryId, productCode: productId, shopName: shopName, productType: productType });
    })


    $('.nav-tabs-custom li').click(function (e) {
        searchClose(e);
        $(this).addClass('active').siblings().removeClass('active');
        if ($(this).attr('type') == 'statusTab') {//状态分类
            $('#brandBox').val('');
            $('#searchBox').val('');
            $("#productId").val('');
            $("#category1").val('');
            $("#category1").trigger("change");
            $("#shopName").val('');
            $('#divAudit').hide();
            $('#divList').show();
            $(".search-box form")[0].reset();
            typeChoose($(this).attr('value') || null);
            // $("#list").MallDatagrid('reload', { auditStatus: $(this).attr('value') || null, brandName: '', keyWords: ''});
        }
        else if ($(this).attr('type') == 'audit-on-off') {
            $.post('GetProductAuditOnOff', {}, function (result) {
                if (result.value == 1) {
                    $('#radio1').attr('checked', 'checked');
                }
                else {
                    $('#radio2').attr('checked', 'checked');
                }
                if (result.value2 == 1) {
                    $('#radio3').attr('checked', 'checked');
                }
                else {
                    $('#radio4').attr('checked', 'checked');
                }
            });
            $('#divAudit').show();
            $('#divList').hide();
        }
    });
    $('#btnSubmit').click(function () {
        var data = $('#radio1').get(0).checked == true ? 1 : 0;
        var data2 = $('#radio3').get(0).checked == true ? 1 : 0;
        $.post('SaveProductAuditOnOff', { value: data, value2: data2 }, function (result) {
            if (result.success) {
                if (result.code == 1) {
                    $.dialog.succeedTips("提交成功，有待审核的商品，请记得进行处理！");
                }
                else {
                    $.dialog.succeedTips("提交成功！");
                }
            }
            else {
                $.dialog.errorTips("提交出现异常！");
            }
        });
    });

});

function batchInfractionSaleOff() {
    var productIds = getSelectedIds();
    if (productIds.length == 0) {
        $.dialog.errorTips("请至少选择一件销售中的商品");
        return;
    }
    infractionSaleOffDialog(productIds);
}


function batchAuditProduct() {
    var productIds = getWaitForAuditingSelIds();
    if (productIds.length == 0) {
        $.dialog.errorTips("请至少选择一件待审核的商品");
        return;
    }
    audit(productIds);
}
//批量设置虚拟销量
function batchUpdateSaleCount() {
    var productIds = getAllSelectedIds();
    if (productIds.length == 0) {
        $.dialog.errorTips("请至少选择一件商品");
        return;
    }
    updateSaleCount(productIds);
}


function getSelectedIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        if (this.state == "销售中") {
            ids.push(this.id);
        }
    });
    return ids;
}
function getAllSelectedIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        ids.push(this.id);
    });
    return ids;
}

function getWaitForAuditingSelIds() {
    var selecteds = $("#list").MallDatagrid('getSelections');
    var ids = [];
    $.each(selecteds, function () {
        if (this.state == "待审核") {
            ids.push(this.id);
        }
    });
    return ids;
}

function updateSaleCount(ids) {
    $('input[name="virtualSaleCounts"]').eq(0).prop("checked", true);
    $(".j_salecount").val('');
    $(".j_salecount").bind("keyup", function () {
        $(this).val($(this).val().replace(/[^0-9]/g, ''));
    });
    $('input[name="virtualSaleCounts"]').unbind('click').bind('click', function () {
        $(".j_salecount").val('');
        $(this).parent().parent().find(".j_salecount").eq(0).focus();
    });
    $(".j_salecount").focus(function () {
        $(this).parent().siblings().find(".j_salecount").val('');
        $("input[type=radio]").prop("checked", false)
        $(this).parent().find("input[type=radio]").prop("checked", true)
    });
    $.dialog({
        title: '批量设置虚拟销量',
        lock: true,
        id: 'btachUpdateSaleCount',
        content: document.getElementById("btachUpdateSaleCount"),
        padding: '0 40px',
        button: [
            {
                name: '取消'
            }, {
                name: '确定',
                callback: function () {
                    var type = $('input[name="virtualSaleCounts"]:checked ').val();
                    var min = 0;
                    var max = 0;
                    var virtualSaleCounts = 0;
                    if (type == 1) {
                        virtualSaleCounts = Number($("#virtualSaleCount").val());
                    } else if (type == 2) {
                        min = Number($("#virtualSaleCount2").val());
                        max = Number($("#virtualSaleCount3").val());
                        if (min <= 0 || max <= 0) {
                            $.dialog.errorTips("请输入合法销量随机范围！");
                            return false;
                        }
                        if (min >= max) {
                            $.dialog.errorTips("请输入合法销量随机范围！");
                            return false;
                        }
                        virtualSaleCounts = parseInt(Math.random() * (max - min + 1) + min);//生成一个从 m - n 之间的随机整数
                    }
                    if (virtualSaleCounts < 0) {
                        $.dialog.errorTips("请输入合法销量！");
                        return false;
                    }
                    var loading = showLoading();
                    $.ajax({
                        type: "post",
                        url: "btachUpdateSaleCount",
                        data: { ids: ids.join(',').toString(), virtualSaleCounts: virtualSaleCounts, minSaleCount: min, maxSaleCount: max, virtualType: type },
                        dataType: "json",
                        async: false,
                        success: function (result) {
                            loading.close();
                            if (result.success) {
                                $.dialog.tips('设置成功');
                                var pageNo = $("#list").MallDatagrid('options').pageNumber;
                                $("#list").MallDatagrid('reload', { pageNumber: pageNo });
                                close = true;
                            }
                            else
                                $.dialog.alert('设置异常!' + result.msg);
                        },
                        error: function () {
                            loading.close();
                            $.dialog.alert('设置出错！');
                        }
                    });
                },
                focus: true
            }]
    });
    $(".j_salecount").eq(0).focus();
}
function audit(productId) {

    $.dialog({
        title: '商品审核',
        lock: true,
        id: 'goodCheck',
        padding: '0 40px',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<p class="help-esp">备注</p>',
            '<textarea id="auditMsgBox" class="form-control" cols="40" rows="2"  ></textarea>\
                 <p id="valid" style="display:none;color:red;position:relative;line-height: 18px;padding:0">请填写未通过理由</p><p id="validateLength" style="display:none;color:red;position:relative;line-height: 18px;padding:0">备注在40字符以内</p> ',
            '</div>',
            '</div>'].join(''),

        init: function () { $("#auditMsgBox").focus(); },
        button: [
            {
                name: '通过审核',
                callback: function () {
                    if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('display', 'block');
                        return false;
                    }
                    auditProduct(productId, 2);
                },
                focus: true
            },
            {
                name: '拒绝',
                callback: function () {
                    if (!$.trim($('#auditMsgBox').val())) {
                        $('#valid').css('display', 'block');
                        return false;
                    }
                    else if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('display', 'block');
                        return false;
                    }
                    else {
                        $('#valid').css('display', 'none');
                        auditProduct(productId, 3, $('#auditMsgBox').val());
                    }
                }
            }]
    });
}



function infractionSaleOffDialog(productIds) {
    $.dialog({
        title: '违规下架',
        lock: true,
        id: 'infractionSaleOff',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<p class="help-esp">下架理由</p>',
            '<textarea id="infractionSaleOffMsgBox" class="form-control" cols="40" rows="2" onkeyup="this.value = this.value.slice(0, 50)" ></textarea>\
                <p id="valid" style="display:none;color:red;position:relative;line-height: 18px;">请填写下架理由</p> ',
            '</div>',
            '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#infractionSaleOffMsgBox").focus(); },
        button: [
            {
                name: '违规下架',
                callback: function () {
                    if (!$.trim($('#infractionSaleOffMsgBox').val())) {
                        $('#valid').css('display', 'block')
                        return false;
                    }
                    else {

                        $('#valid').css('display', 'none');
                        auditProduct(productIds, 4, $('#infractionSaleOffMsgBox').val());

                    }
                },
                focus: true
            },
            {
                name: '取消'
            }]
    });
}


function auditProduct(productIds, auditState, msg) {
    var loading = showLoading();
    $.post('./BatchAudit', { productIds: productIds.toString(), auditState: auditState, message: msg }, function (result) {
        if (result.success) {
            $.dialog.succeedTips("操作成功！");
            var pageNo = $("#list").MallDatagrid('options').pageNumber;
            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
        }
        else {
            $.dialog.errorTips("操作失败");
        }
        loading.close();
    });
};


function ExportExecl() {
    var auditStatus = $('.nav-tabs-custom li[class="active"]').attr("value") == undefined ? null : $('.nav-tabs-custom li[class="active"]').attr("value");
    var brandName = $.trim($('#brandBox').val());
    var keyWords = $.trim($('#searchBox').val());
    var productId = $.trim($('#productId').val());
    var shopName = $.trim($('#shopName').val());
    var productType = $("#productType").val();

    var href = "/Admin/Product/ExportToExcel?auditStatus=" + auditStatus;
    href += "&categoryId=" + categoryId + "&brandName=" + brandName + "&keyWords=" + keyWords + "&productCode=" + productId + "&shopName=" + shopName + "&productType=" + productType;
    if (auditStatus == "2") {
        href += "&saleStatus=1";
    }
    $("#aExport").attr("href", href);
}

$(document).on('click', '.view-mobile-product', function () {
    $("#mobileshow").attr("src", $(this).attr("data-url"));
    $('.mobile-dialog').show();
    $('.cover').fadeIn();
});
$(function () {
    $('.cover').click(function () {
        $('.mobile-dialog').hide();
        $("#mobileshow").attr("src", "about:blank");
        $(this).fadeOut();
    });
})
